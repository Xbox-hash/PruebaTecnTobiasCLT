
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Application.Users.Commands;
using UsersApi.Infrastructure.Persistence;
using static UsersApi.Application.Users.Commands.CreateUserCommands;

namespace UsersApi.Application.Users.Queries;

// La Query (recibe el filtro opcional de si está activo o no)
public record GetUsersQuery(bool? IsActive);

//El Handler de lectura
public class GetUsersHandler : IQueryHandler<GetUsersQuery, List<UserResponse>>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetUsersHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<UserResponse>> HandleAsync(GetUsersQuery query,CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var q = db.Users.AsNoTracking();

        if (query.IsActive.HasValue)
        {
            q = q.Where(u => u.IsActive == query.IsActive.Value);
        }

        return await q
            .Select(u => new UserResponse(u.Id, u.Name, u.Email, u.IsActive))
            .ToListAsync(ct);
    }
}