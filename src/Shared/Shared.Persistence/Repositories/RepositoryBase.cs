using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.MultiTenancy;
using Shared.Abstractions.Repositories;
using Shared.Kernel.Abstractions.Domain;
using Shared.Kernel.Models;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Shared.Persistence.Repositories;

public abstract class RepositoryBase<TEntity, TKey>(DbContext dbContext, ITenantResolver tenantResolver) : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    protected readonly DbContext dbContext = Guard.Against.Null(dbContext);
    protected readonly ITenantResolver tenantResolver = Guard.Against.Null(tenantResolver);

    protected DbSet<TEntity> Entities => dbContext.Set<TEntity>();

    protected virtual IQueryable<TEntity> Query(bool asNoTracking = false)
    {
        IQueryable<TEntity> query = dbContext.Set<TEntity>();

        if (typeof(IMustHaveTenant).IsAssignableFrom(typeof(TEntity)))
        {
            var tenantId = tenantResolver.ResolveTenant();
            Guard.Against.Default(tenantId, nameof(tenantId));

            query = query.Where(e =>
                ((IMustHaveTenant)(object)e).IsGlobal || ((IMustHaveTenant)(object)e).Tenants.Contains(tenantId.Value));
        }

        return asNoTracking ? query.AsNoTracking() : query;
    }

    protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query)
    => query;

    private static IQueryable<TEntity> ApplyTenantFilter(IQueryable<TEntity> query, Guid? tenantId = null)
    {
        if (tenantId.HasValue && typeof(IMustHaveTenant).IsAssignableFrom(typeof(TEntity)))
            query = query.Where(e =>
                ((IMustHaveTenant)(object)e).IsGlobal || ((IMustHaveTenant)(object)e).Tenants.Contains(tenantId.Value));

        return query;
    }

    private static IQueryable<TEntity> ManageTracking(IQueryable<TEntity> query, bool asNoTracking = false)
    {
        if (asNoTracking)
            query = query.AsNoTracking();

        return query;
    }

    public async Task<TEntity?> ReadAsync(TKey id, Guid? tenantId = null, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(id, nameof(id));

        var query = Query();

        ApplyTenantFilter(query, tenantId);
        ManageTracking(query, asNoTracking);

        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> ReadAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var query = Query();

        ApplyTenantFilter(query, tenantId);

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<PaginatedList<TEntity>> ReadAsPaged(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request.Normalize();

        var queryable = await GetQueryable(cancellationToken);

        var allMembers = typeof(TEntity)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sortableMembers = typeof(TEntity)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filterFields = request.FilterOn?.Intersect(allMembers)?.ToList();

        if (filterFields?.Count > 0 && !string.IsNullOrWhiteSpace(request.Filter))
        {
            if (request.FilterStrict)
            {
                queryable = queryable.Where(e =>
                    filterFields.All(field =>
                        EF.Functions.ILike(
                            EF.Property<string>(e, field) ?? "",
                            $"%{request.Filter}%"
                        )));
            }
            else
            {
                queryable = queryable.Where(e =>
                    filterFields.Any(field =>
                        EF.Functions.ILike(
                            EF.Property<string>(e, field) ?? "",
                            $"%{request.Filter}%"
                        )));
            }
        }

        var count = await queryable.CountAsync(cancellationToken: cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.SortBy) && sortableMembers.Contains(request.SortBy))
            queryable = queryable.OrderBy(request.SortBy + $"{(request.SortDescending ? " DESC" : "")}");

        if (request.PageSize > 0 && request.PageNumber > 0)
            queryable = queryable.Skip(request.Skip)
                .Take(request.PageSize);

        queryable = ManageTracking(queryable, true);

        var items = await queryable.ToListAsync(cancellationToken);
        var result = new PaginatedList<TEntity>(items, count, request.PageNumber, request.PageSize);

        return result;
    }

    public async Task<IEnumerable<TEntity>> FilterAsync(Expression<Func<TEntity, bool>> predicate, Guid? tenantId = null, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(predicate, nameof(predicate));

        var query = Query();

        ApplyTenantFilter(query, tenantId);
        ManageTracking(query, asNoTracking);

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        if (entity is IMustHaveTenant mustHaveTenant)
        {
            var tenantId = tenantResolver.ResolveTenant();
            Guard.Against.Default(tenantId, nameof(tenantId));

            mustHaveTenant.Tenants = [tenantId.Value];
        }

        await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public Task ModifyAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        Entities.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        Entities.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task RemoveAsync(TKey id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(id, nameof(id));

        var entity = await ReadAsync(id, cancellationToken: cancellationToken);

        if (entity != null)
            Entities.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(predicate, nameof(predicate));

        var query = Query();

        ApplyTenantFilter(query, tenantId);

        return await query.AnyAsync(predicate, cancellationToken);
    }

    public Task<IQueryable<TEntity>> GetQueryable(CancellationToken cancellationToken = default) =>
        Task.FromResult(Query());

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

