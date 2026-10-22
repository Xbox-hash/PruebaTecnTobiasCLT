
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
        }).AddEndpointFilter<ValidationFilter<CreateUserCommand>>();

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

        // PUT /users/{id} - modificar/actualizar user
        group.MapPut("/{id:int}", async (int id, UpdateUserRequest request, ICommandHandler<UpdateUserCommand, UpdateUserResult> handler, CancellationToken ct) =>
        {
            var command = new UpdateUserCommand(id, request.Name, request.Email, request.IsActive);
            var result = await handler.HandleAsync(command, ct);

            return result.Status switch
            {
                UpdateUserStatus.NotFound => Results.NotFound( 
                    new{ error = $"Usuario con id { id } no encontrado" }),
                UpdateUserStatus.EmailAlreadyInUse => Results.Conflict(new
                    {
                        error = "El email ya pertenece a otro usuario"
                    }), _ => Results.Ok(result.User)
            };
        }).AddEndpointFilter<ValidationFilter<UpdateUserRequest>>();

        // DELETE /users/{id} - Eliminar usuario
        group.MapDelete("/{id:int}", async (int id, ICommandHandler<DeleteUserCommand, bool> handler, CancellationToken ct) =>
        {
            var deleted = await handler.HandleAsync(new DeleteUserCommand(id), ct);

            return deleted
                ? Results.NoContent()
                : Results.NotFound(new
                {
                    error = $"Usuario con id {id} no encontrado"
                });
        });

        group.MapPost("/bulk", async (BulkCreateUsersCommand command, ICommandHandler<BulkCreateUsersCommand, BulkResult> handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return Results.Ok(result);
        }).AddEndpointFilter<ValidationFilter<BulkCreateUsersCommand>>();
        return group;
    }
}