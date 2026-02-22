namespace Shared.Abstractions.Data;

public interface IEntitySeeder
{
    /// <summary>
    /// Seed the entity if missing from DB, based on JSON definitions.
    /// </summary>
    Task SeedAsync(CancellationToken ct = default);
}

public interface IEntitySeeder<TEntity> : IEntitySeeder 
    where TEntity : class
{
    /// <summary>
    /// Verify if seeding is required, based on JSON definitions.
    /// </summary>
    Task<(bool IsRequired, List<TEntity> Missing)> IsSeedingRequired(CancellationToken ct = default);
}
