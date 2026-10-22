using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Domain;
using UsersApi.Infrastructure.Persistence;
using static UsersApi.Application.Users.Commands.CreateUserCommands;

namespace UsersApi.Application.Users.Commands;

public record BulkCreateUsersCommand(List<CreateUserCommand> Users);

public record BulkItemError(int Index, string? Email, string Error);

public record BulkResult(int Total, int Created, int Failed, List<BulkItemError> Errors);

public class BulkCreateUsersValidator : AbstractValidator<BulkCreateUsersCommand>
{
    public BulkCreateUsersValidator()
    {
        RuleFor(x => x.Users).NotEmpty().WithMessage("La lista de usuarios es requerida.");
        RuleFor(x => x.Users).Must(u => u.Count <= 1000).WithMessage("El lote no puede superar los 1000 usuarios.");
    }
}

public class BulkCreateUsersHandler : ICommandHandler<BulkCreateUsersCommand, BulkResult>
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IValidator<CreateUserCommand> _itemValidator;

    public BulkCreateUsersHandler(
        IDbContextFactory<AppDbContext> factory,
        IValidator<CreateUserCommand> itemValidator)
    {
        _factory = factory;
        _itemValidator = itemValidator;
    }

    public async Task<BulkResult> HandleAsync(BulkCreateUsersCommand command, CancellationToken ct)
    {
        using var throttle = new SemaphoreSlim(8);

        var tasks = command.Users.Select((item, index) => ProcessItemAsync(item, index, throttle, ct));
        var results = await Task.WhenAll(tasks);

        var errors = results.Where(e => e is not null).Select(e => e!).ToList();

        return new BulkResult(
            Total: command.Users.Count,
            Created: command.Users.Count - errors.Count,
            Failed: errors.Count,
            Errors: errors
        );
    }

    private async Task<BulkItemError?> ProcessItemAsync(
        CreateUserCommand item,
        int index,
        SemaphoreSlim throttle,
        CancellationToken ct)
    {
        var validation = await _itemValidator.ValidateAsync(item, ct);
        if (!validation.IsValid)
        {
            return new BulkItemError(index, item.Email, validation.Errors[0].ErrorMessage);
        }

        await throttle.WaitAsync(ct);
        try
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            var normalizedEmail = item.Email.Trim().ToLowerInvariant();

            var exists = await db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
            if (exists)
            {
                return new BulkItemError(index, normalizedEmail, "El email ya existe");
            }

            var user = new User
            {
                Name = item.Name.Trim(),
                Email = normalizedEmail,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            return null;
        }
        catch (DbUpdateException)
        {
            return new BulkItemError(index, item.Email, "El email ya existe");
        }
        finally
        {
            throttle.Release();
        }
    }
}
