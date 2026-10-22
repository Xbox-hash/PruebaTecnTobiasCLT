using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.Addresses.Commands;

public record UpdateAddressCommand(int Id, AddressRequest Request);

public class UpdateAddressHandler : ICommandHandler<UpdateAddressCommand, AddressResponse?>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UpdateAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<AddressResponse?> HandleAsync(UpdateAddressCommand command, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == command.Id, ct);
        if (address is null)
        {
            return null;
        }

        address.Street = command.Request.Street.Trim();
        address.City = command.Request.City.Trim();
        address.Country = command.Request.Country.Trim();
        address.ZipCode = command.Request.ZipCode?.Trim();

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