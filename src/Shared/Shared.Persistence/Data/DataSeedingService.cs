using Shared.Abstractions.Data;

namespace Shared.Persistence.Data;

public class DataSeedingService(IEnumerable<IEntitySeeder> seeders) : IDataSeedingService
{
    public async Task SeedAllAsync(CancellationToken ct = default)
    {
        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(ct);
        }
    }
}
