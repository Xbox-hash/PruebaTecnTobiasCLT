using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Application.Addresses.Commands;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.Addresses.Queries;

public record GetUserAddressesQuery(int UserId);

public class GetUserAddressesHandler : IQueryHandler<GetUserAddressesQuery, List<AddressResponse>?>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetUserAddressesHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<AddressResponse>?> HandleAsync(GetUserAddressesQuery query, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var userExists = await db.Users.AnyAsync(u => u.Id == query.UserId, ct);
        if (!userExists)
        {
            return null;
        }

        return await db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == query.UserId)
            .Select(a => new AddressResponse(
                a.Id,
                a.UserId,
                a.Street,
                a.City,
                a.Country,
                a.ZipCode
            ))
            .ToListAsync(ct);
    }
}