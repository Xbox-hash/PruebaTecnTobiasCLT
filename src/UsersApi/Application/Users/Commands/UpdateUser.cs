using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Infrastructure.Persistence;
using static UsersApi.Application.Users.Commands.CreateUserCommands;
namespace UsersApi.Application.Users.Commands
{
    public record UpdateUserRequest(string Name, string Email, bool IsActive);
    public record UpdateUserCommand(int Id, string Name, string Email, bool IsActive);

    public enum UpdateUserStatus
    {
        Success,
        NotFound,
        EmailAlreadyInUse
    }

    public record UpdateUserResult(UpdateUserStatus Status, UserResponse? User = null);

    //validamos con fluent validation
    public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El formato del email no es válido.")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage("El email debe contener un dominio con extensión válida (ej: .com).");
        }

    }
    // Handler de actualización
    public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, UpdateUserResult>
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public UpdateUserHandler(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<UpdateUserResult> HandleAsync(UpdateUserCommand cmd, CancellationToken ct)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            // buscar si el usuario existe
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == cmd.Id, ct);
            if (user is null)
            {
                return new UpdateUserResult(UpdateUserStatus.NotFound);
            }

            var normalizedEmail = cmd.Email.Trim().ToLowerInvariant();

            // Si cambio el correo, verificar que no pertenezca a OTRO usuario
            if (user.Email != normalizedEmail)
            {
                var emailTaken = await db.Users.AnyAsync(u => u.Email == normalizedEmail && u.Id != cmd.Id, ct);
                if (emailTaken)
                {
                    return new UpdateUserResult(UpdateUserStatus.EmailAlreadyInUse);
                }
            }

            //Aplicar cambios
            user.Name = cmd.Name.Trim();
            user.Email = normalizedEmail;
            user.IsActive = cmd.IsActive;

            await db.SaveChangesAsync(ct);

            return new UpdateUserResult(UpdateUserStatus.Success, new UserResponse(user. Id, user.Name, user.Email, user.IsActive));
        }
    }
}