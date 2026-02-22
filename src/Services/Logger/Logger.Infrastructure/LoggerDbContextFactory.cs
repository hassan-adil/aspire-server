using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Logger.Infrastructure;

public class LoggerDbContextFactory : IDesignTimeDbContextFactory<LoggerDbContext>
{
    public LoggerDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<LoggerDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("logger-db"));

        return new LoggerDbContext(optionsBuilder.Options);
    }
}
