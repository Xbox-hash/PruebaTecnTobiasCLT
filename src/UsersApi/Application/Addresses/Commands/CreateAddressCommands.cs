using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Domain;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.Addresses.Commands;

public record AddressRequest(string Street, string City, string Country, string? ZipCode);

public record AddressResponse(int Id, int UserId, string Street, string City, string Country, string? ZipCode);

public record CreateAddressCommand(int UserId, AddressRequest Request);

public class AddressValidator : AbstractValidator<AddressRequest>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().WithMessage("La calle es requerida.");
        RuleFor(x => x.City).NotEmpty().WithMessage("La ciudad es requerida.");
        RuleFor(x => x.Country).NotEmpty().WithMessage("El país es requerido.");
    }
}

public class CreateAddressHandler : ICommandHandler<CreateAddressCommand, AddressResponse?>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<AddressResponse?> HandleAsync(CreateAddressCommand command, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var userExists = await db.Users.AnyAsync(u => u.Id == command.UserId, ct);
        if (!userExists)
        {
            return null;
        }

        var address = new Address
        {
            UserId = command.UserId,
            Street = command.Request.Street.Trim(),
            City = command.Request.City.Trim(),
            Country = command.Request.Country.Trim(),
            ZipCode = command.Request.ZipCode?.Trim()
        };

        db.Addresses.Add(address);
        await db.SaveChangesAsync(ct);

        return new AddressResponse(
            address.Id,
            address.UserId,
            address.Street,
            address.City,
            address.Country,
            address.ZipCode
        );
    }
}
