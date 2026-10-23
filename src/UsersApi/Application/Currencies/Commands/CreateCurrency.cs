using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Domain;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.Currencies.Commands;

public record CreateCurrencyRequest(string Code, string Name, decimal RateToBase);

public record CurrencyResponse(int Id, string Code, string Name, decimal RateToBase);

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyRequest>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("El código es requerido.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.");
        RuleFor(x => x.RateToBase).GreaterThan(0).WithMessage("La tasa respecto a la base debe ser mayor a 0.");
    }
}

public class CreateCurrencyHandler : ICommandHandler<CreateCurrencyRequest, CurrencyResponse?>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateCurrencyHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<CurrencyResponse?> HandleAsync(CreateCurrencyRequest request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var exists = await db.Currencies.AnyAsync(c => c.Code == normalizedCode, ct);
        if (exists)
        {
            return null;
        }

        var currency = new Currency
        {
            Code = normalizedCode,
            Name = request.Name.Trim(),
            RateToBase = request.RateToBase
        };

        db.Currencies.Add(currency);
        await db.SaveChangesAsync(ct);

        return new CurrencyResponse(currency.Id, currency.Code, currency.Name, currency.RateToBase);
    }
}
