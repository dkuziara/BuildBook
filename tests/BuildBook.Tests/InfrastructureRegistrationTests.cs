using BuildBook.Application.BuildRecords;
using BuildBook.Infrastructure;
using BuildBook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildBook.Tests;

public class InfrastructureRegistrationTests
{
    [Fact]
    public void AddBuildBookInfrastructureRegistersDbContextFactory()
    {
        var configuration = CreateConfiguration(
            "Server=(localdb)\\MSSQLLocalDB;Database=BuildBookTest;Trusted_Connection=True;TrustServerCertificate=True");
        var services = new ServiceCollection();

        services.AddBuildBookInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<BuildBookDbContext>>();
        var creator = provider.GetRequiredService<IBuildRecordCreator>();
        var detailReader = provider.GetRequiredService<IBuildRecordDetailReader>();
        var productDetailsUpdater = provider.GetRequiredService<IProductDetailsUpdater>();
        var buildDetailsUpdater = provider.GetRequiredService<IBuildDetailsUpdater>();

        Assert.NotNull(factory);
        Assert.NotNull(creator);
        Assert.NotNull(detailReader);
        Assert.NotNull(productDetailsUpdater);
        Assert.NotNull(buildDetailsUpdater);
    }

    [Fact]
    public void AddBuildBookInfrastructureRequiresConnectionString()
    {
        var configuration = CreateConfiguration(null);
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddBuildBookInfrastructure(configuration));

        Assert.Contains(DependencyInjection.BuildBookDatabaseConnectionName, exception.Message);
    }

    [Fact]
    public void InitialMigrationIsAvailable()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookMigrationTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);
        var migrations = context.Database.GetMigrations();

        Assert.Contains(migrations, migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
    }

    private static IConfiguration CreateConfiguration(string? connectionString)
    {
        var values = new Dictionary<string, string?>
        {
            ["BuildBook:EnableDetailedErrors"] = "false"
        };

        if (connectionString is not null)
        {
            values[$"ConnectionStrings:{DependencyInjection.BuildBookDatabaseConnectionName}"] = connectionString;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
