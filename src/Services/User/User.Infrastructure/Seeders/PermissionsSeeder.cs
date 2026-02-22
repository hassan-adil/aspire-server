using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Data;
using Shared.Abstractions.Dependencies;
using Shared.Abstractions.UnitOfWork;
using System.Text.Json;
using User.Core.Domain.Entities;

namespace User.Infrastructure.Seeders;

public class PermissionsSeeder(UserDbContext dbContext, IUnitOfWork unitOfWork, ILogger<PermissionsSeeder> logger) : IEntitySeeder<Permission>, IScopedDependency
{
    private const string FileName = "permissions-seed.json";

    public async Task<(bool IsRequired, List<Permission> Missing)> IsSeedingRequired(CancellationToken ct = default)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Seeds",
            FileName
        );

        if (!File.Exists(path))
            throw new FileNotFoundException($"Seed file not found: {path}");

        var json = await File.ReadAllTextAsync(path, ct);
        var records = JsonSerializer.Deserialize<List<Permission>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        }) ?? [];

        var existing = await dbContext.Permissions.Select(p => p.Name).ToHashSetAsync(cancellationToken: ct);

        var missing = records.Where(r => !existing.Contains(r.Name)).ToList() ?? [];

        return (missing.Count > 0, missing);
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var (required, missing) = await IsSeedingRequired(ct);

        if (!required)
            return;

        await dbContext.Permissions.AddRangeAsync(missing, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seeded {Count} permissions",
            missing.Count);
    }
}
