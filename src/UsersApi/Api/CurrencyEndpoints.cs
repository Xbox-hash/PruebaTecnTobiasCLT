using UsersApi.Application.Abstractions;
using UsersApi.Application.Currencies.Commands;
using UsersApi.Application.Currencies.Queries;
using UsersApi.Application.CurrencyConversion;

namespace UsersApi.Api;

public static class CurrencyEndpoints
{
    public static RouteGroupBuilder MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/").WithTags("Currencies");

        group.MapGet("/currencies", async (
            IQueryHandler<GetCurrenciesQuery, List<CurrencyResponse>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetCurrenciesQuery(), ct);
            return Results.Ok(result);
        });

        group.MapPost("/currencies", async (
            CreateCurrencyRequest request,
            ICommandHandler<CreateCurrencyRequest, CurrencyResponse?> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);

            return result is null
                ? Results.Conflict(new { error = $"La moneda con código '{request.Code}' ya existe." })
                : Results.Created($"/currencies/{result.Id}", result);
        })
        .AddEndpointFilter<ValidationFilter<CreateCurrencyRequest>>();

        group.MapPost("/currency/convert", async (
            ConvertCurrencyRequest request,
            ICommandHandler<ConvertCurrencyRequest, ConvertCurrencyResponse?> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);

            return result is null
                ? Results.NotFound(new { error = "Una o ambas monedas no existen en el sistema." })
                : Results.Ok(result);
        })
        .AddEndpointFilter<ValidationFilter<ConvertCurrencyRequest>>();

        return group;
    }
}
