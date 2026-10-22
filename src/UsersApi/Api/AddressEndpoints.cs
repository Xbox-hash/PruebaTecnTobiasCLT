using UsersApi.Application.Abstractions;
using UsersApi.Application.Addresses.Commands;
using UsersApi.Application.Addresses.Queries;

namespace UsersApi.Api;

public static class AddressEndpoints
{
    public static RouteGroupBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/").WithTags("Addresses");

        //POST
        group.MapPost("/users/{userId:int}/addresses", async (int userId, AddressRequest request,
            ICommandHandler<CreateAddressCommand, AddressResponse?> handler, CancellationToken ct) =>
        {
            var command = new CreateAddressCommand(userId, request);
            var result = await handler.HandleAsync(command, ct);

            return result is null
                ? Results.NotFound(new { error = $"Usuario con id {userId} no encontrado" })
                : Results.Created($"/users/{userId}/addresses/{result.Id}", result);
        }).AddEndpointFilter<ValidationFilter<AddressRequest>>();

        //GET LISTAR ADDRES POR USER ID
        group.MapGet("/users/{userId:int}/addresses", async (int userId, IQueryHandler<GetUserAddressesQuery,
            List<AddressResponse>?> handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetUserAddressesQuery(userId), ct);

            return result is null
                ? Results.NotFound(new { error = $"Usuario con id {userId} no encontrado" })
                : Results.Ok(result);
        });
        return group;
    }
}
