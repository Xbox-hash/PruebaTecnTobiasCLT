using FluentValidation;
using Microsoft.AspNetCore.Http;
namespace UsersApi.Api;

// Debe ser generica: <T>
public class ValidationFilter<T> : IEndpointFilter
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    //Debe ser public, async y devolver ValueTask<object?>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arg = context.Arguments.OfType<T>().FirstOrDefault();
        if (arg is null)
        {
            return await next(context);
        }

        var result = await _validator.ValidateAsync(arg);
        if (!result.IsValid)
        {
            return TypedResults.ValidationProblem(result.ToDictionary());
        }

        return await next(context);
    }
}