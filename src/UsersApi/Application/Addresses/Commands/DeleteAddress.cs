using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.Addresses.Commands;

public record DeleteAddressCommand(int Id);

public class DeleteAddressHandler : ICommandHandler<DeleteAddressCommand, bool>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> HandleAsync(DeleteAddressCommand command, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == command.Id, ct);
        if (address is null)
        {
            return false;
        }

        db.Addresses.Remove(address);
        await db.SaveChangesAsync(ct);

        return true;
    }
}
