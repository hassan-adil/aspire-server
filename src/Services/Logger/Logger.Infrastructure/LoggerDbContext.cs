using Logger.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Abstractions.MultiTenancy;
using System.Text.Json;

namespace Logger.Infrastructure;

public class LoggerDbContext : DbContext
{
    private readonly ITenantResolver? tenantResolver;

    public LoggerDbContext(DbContextOptions<LoggerDbContext> options, ITenantResolver _tenantResolver)
        : base(options)
    {
        tenantResolver = _tenantResolver;
    }

    public LoggerDbContext(DbContextOptions<LoggerDbContext> options) : base(options)
    {
    }

    public DbSet<RequestLog> RequestLogs { get; set; } = default!;
    public DbSet<ExceptionLog> ExceptionLogs { get; set; } = default!;
    public DbSet<DatabaseOperationLog> DatabaseOperationLogs { get; set; } = default!;

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

        var jsonDict = new ValueComparer<Dictionary<string, object>>(
            (a, b) => JsonSerializer.Serialize(a, JsonSerializerOptions.Default) == JsonSerializer.Serialize(b, JsonSerializerOptions.Default),
            v => v == null ? 0 : JsonSerializer.Serialize(v, JsonSerializerOptions.Default).GetHashCode(),
            v => v == null ? new() : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(v, JsonSerializerOptions.Default), JsonSerializerOptions.Default)!
        );

        modelBuilder.Entity<DatabaseOperationLog>()
          .Property(p => p.OldSnapshot).HasColumnType("jsonb").Metadata.SetValueComparer(jsonDict);

        modelBuilder.Entity<DatabaseOperationLog>()
          .Property(p => p.NewSnapshot).HasColumnType("jsonb").Metadata.SetValueComparer(jsonDict);

        modelBuilder.Entity<DatabaseOperationLog>()
            .HasIndex(x => x.OldSnapshot)
            .HasMethod("gin");

        modelBuilder.Entity<DatabaseOperationLog>()
            .HasIndex(x => x.NewSnapshot)
            .HasMethod("gin");
    }
}
