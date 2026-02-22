namespace Shared.Abstractions.Data;

public interface IDataSeedingService
{
    Task SeedAllAsync(CancellationToken ct = default);
}
