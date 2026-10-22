
using UsersApi.Application.Abstractions;
using UsersApi.Application.Users.Commands;
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

        return group;
    }
}