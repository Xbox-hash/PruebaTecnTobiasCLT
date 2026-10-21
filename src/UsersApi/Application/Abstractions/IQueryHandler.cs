
namespace UsersApi.Application.Abstractions;

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}