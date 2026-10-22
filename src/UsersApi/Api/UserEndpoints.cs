
using UsersApi.Application.Abstractions;
using UsersApi.Application.Users.Commands;
using UsersApi.Application.Users.Queries;
using static UsersApi.Application.Users.Commands.CreateUserCommands;

namespace UsersApi.Api;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        // POST /users
        group.MapPost("/", async (CreateUserCommand command, ICommandHandler<CreateUserCommand, UserResponse?> handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);

            // Si el handler devuelve null significa que el email ya existia
            return result is null
                ? Results.Conflict(new { error = "El email ya existe" })
                : Results.Created($"/users/{result.Id}", result);
        })
            .AddEndpointFilter<ValidationFilter<CreateUserCommand>>();

        // GET /users - Listar todos (con filtro opcional ?isActive=true|false)
        group.MapGet("/", async (bool? isActive, IQueryHandler<GetUsersQuery, List<UserResponse>> handler, CancellationToken ct) =>
        {
            var users = await handler.HandleAsync(new GetUsersQuery(isActive), ct);
            return Results.Ok(users);
        });

        // GET /users/{id} - Obtener por Id (o 404 si no existe)
        group.MapGet("/{id:int}", async (int id, IQueryHandler<GetUserByIdQuery, UserResponse?> handler, CancellationToken ct) =>
        {
            var user = await handler.HandleAsync(new GetUserByIdQuery(id), ct);

            return user is null
                ? Results.NotFound(new
                {
                    error = $"Usuario con id {id} no encontrado"
                })
                : Results.Ok(user);
        });
        return group;
    }
}