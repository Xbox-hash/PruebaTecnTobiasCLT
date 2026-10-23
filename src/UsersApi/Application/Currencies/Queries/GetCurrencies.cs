using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Application.Currencies.Commands;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.Currencies.Queries;

public record GetCurrenciesQuery;

public class GetCurrenciesHandler : IQueryHandler<GetCurrenciesQuery, List<CurrencyResponse>>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetCurrenciesHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<CurrencyResponse>> HandleAsync(GetCurrenciesQuery query, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        return await db.Currencies
            .AsNoTracking()
            .Select(c => new CurrencyResponse(c.Id, c.Code, c.Name, c.RateToBase))
            .ToListAsync(ct);
    }
}
