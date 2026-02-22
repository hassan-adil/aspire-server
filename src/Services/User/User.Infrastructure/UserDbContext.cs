using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.MultiTenancy;
using User.Core.Domain.Entities;

namespace User.Infrastructure;

public class UserDbContext : DbContext
{
    private readonly ITenantResolver? tenantResolver;

    public UserDbContext(DbContextOptions<UserDbContext> options, ITenantResolver _tenantResolver)
        : base(options)
    {
        tenantResolver = _tenantResolver;
    }

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<Permission> Permissions { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                // IsGlobal => false by default
                builder
                    .Property(nameof(IMustHaveTenant.IsGlobal))
                    .HasDefaultValue(false);

                // Tenants => empty uuid array by default
                builder
                    .Property(nameof(IMustHaveTenant.Tenants))
                    .HasColumnType("uuid[]")
                    .HasDefaultValueSql("'{}'::uuid[]");

                builder
                    .HasIndex(nameof(IMustHaveTenant.Tenants))
                    .HasMethod("gin");
            }
        }

        modelBuilder.AddInboxStateEntity(cfg =>
        {
            cfg.ToTable("InboxState", "messaging");
        });

        modelBuilder.AddOutboxMessageEntity(cfg =>
        {
            cfg.ToTable("OutboxMessage", "messaging");
        });

        modelBuilder.AddOutboxStateEntity(cfg =>
        {
            cfg.ToTable("OutboxState", "messaging");
        });
    }
}
