using Shared.Kernel.Models;
using System.Linq.Expressions;

namespace Shared.Abstractions.Repositories;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    /// <summary>
    /// Retrieves an entity by its ID, optionally checks for tenant.
    /// </summary>
    /// <param name="id">The primary key of the entity.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The entity, or null if not found.</returns>
    Task<TEntity?> ReadAsync(TKey id, Guid? tenantId = null, bool asNoTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities for the current tenant, can limit by tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A list of entities.</returns>
    Task<IEnumerable<TEntity>> ReadAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task<PaginatedList<TEntity>> ReadAsPaged(PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities which fulfills certain criteria.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A list of entities.</returns>
    Task<IEnumerable<TEntity>> FilterAsync(Expression<Func<TEntity, bool>> predicate, Guid? tenantId = null, bool asNoTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifies an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    Task ModifyAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity by its ID.
    /// </summary>
    /// <param name="id">The primary key of the entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    Task RemoveAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entities satisfy the specified condition asynchronously.
    /// </summary>
    /// <param name="predicate">An expression that defines the condition to test each entity against.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if any
    /// entities satisfy the condition; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, Guid? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Used to get queryable of entity.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A queryable of entity.</returns>
    Task<IQueryable<TEntity>> GetQueryable(CancellationToken cancellationToken = default);

    /// <summary>
    ///     
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
