using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Application.Users.Commands;
using UsersApi.Infrastructure.Persistence;
using static UsersApi.Application.Users.Commands.CreateUserCommands;

namespace UsersApi.Application.Users.Queries;

// la Query que recibe el ID
public record GetUserByIdQuery(int Id);

// el handler que busca por ID
public class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserResponse?>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetUserByIdHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<UserResponse?> HandleAsync(GetUserByIdQuery query, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == query.Id)
            .Select(u => new UserResponse(u.Id, u.Name, u.Email, u.IsActive))
            .FirstOrDefaultAsync(ct);
    }
}