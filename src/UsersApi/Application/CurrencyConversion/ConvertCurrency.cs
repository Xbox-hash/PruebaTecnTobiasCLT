using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Domain;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.CurrencyConversion;

public record ConvertCurrencyRequest(string FromCurrencyCode, string ToCurrencyCode, decimal Amount);

public record ConvertCurrencyResponse(string FromCurrency, string ToCurrency, decimal OriginalAmount, decimal ConvertedAmount);

public class ConvertCurrencyValidator : AbstractValidator<ConvertCurrencyRequest>
{
    public ConvertCurrencyValidator()
    {
        RuleFor(x => x.FromCurrencyCode).NotEmpty().WithMessage("El código de origen es requerido.");
        RuleFor(x => x.ToCurrencyCode).NotEmpty().WithMessage("El código de destino es requerido.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto a convertir debe ser mayor a 0.");
    }
}

public class ConvertCurrencyHandler : ICommandHandler<ConvertCurrencyRequest, ConvertCurrencyResponse?>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ConvertCurrencyHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<ConvertCurrencyResponse?> HandleAsync(ConvertCurrencyRequest request, CancellationToken ct)
    {
        var fromCode = request.FromCurrencyCode.Trim().ToUpperInvariant();
        var toCode = request.ToCurrencyCode.Trim().ToUpperInvariant();

        var fromTask = FindCurrencyAsync(fromCode, ct);
        var toTask = FindCurrencyAsync(toCode, ct);

        await Task.WhenAll(fromTask, toTask);

        var from = await fromTask;
        var to = await toTask;

        if (from is null || to is null)
        {
            return null;
        }

        var baseAmount = request.Amount * from.RateToBase;
        var convertedAmount = baseAmount / to.RateToBase;

        return new ConvertCurrencyResponse(
            FromCurrency: from.Code,
            ToCurrency: to.Code,
            OriginalAmount: request.Amount,
            ConvertedAmount: Math.Round(convertedAmount, 4)
        );
    }

    private async Task<Currency?> FindCurrencyAsync(string code, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code, ct);
    }
}
