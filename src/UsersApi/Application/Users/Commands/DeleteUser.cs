
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.Users.Commands;

public record DeleteUserCommand(int Id);

public class DeleteUserHandler : ICommandHandler<DeleteUserCommand, bool>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> HandleAsync(DeleteUserCommand cmd, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == cmd.Id, ct);
        if (user is null)
        {
            return false; // No encontrado
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);

        return true; // Eliminado exitosamente (y en cascada sus direcciones)
    }
}