namespace UsersApi.Application.Abstractions;

public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}