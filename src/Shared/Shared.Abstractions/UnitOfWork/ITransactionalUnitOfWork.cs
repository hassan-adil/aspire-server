namespace Shared.Abstractions.UnitOfWork;

public interface ITransactionalUnitOfWork : IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
