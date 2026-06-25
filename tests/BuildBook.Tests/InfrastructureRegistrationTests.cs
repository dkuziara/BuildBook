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
        services.AddLogging();

        services.AddBuildBookInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<BuildBookDbContext>>();
        var databaseInitializer = provider.GetRequiredService<BuildBookDatabaseInitializer>();
        var auditService = provider.GetRequiredService<IBuildRecordAuditService>();
        var auditHistoryReader = provider.GetRequiredService<IBuildRecordAuditHistoryReader>();
        var secretStore = provider.GetRequiredService<IBuildRecordSecretStore>();
        var secretService = provider.GetRequiredService<IBuildRecordSecretService>();
        var creator = provider.GetRequiredService<IBuildRecordCreator>();
        var homePageReader = provider.GetRequiredService<IHomePageReader>();
        var registerReader = provider.GetRequiredService<IBuildRegisterReader>();
        var searchService = provider.GetRequiredService<IBuildRecordSearchService>();
        var detailReader = provider.GetRequiredService<IBuildRecordDetailReader>();
        var productDetailsUpdater = provider.GetRequiredService<IProductDetailsUpdater>();
        var buildDetailsUpdater = provider.GetRequiredService<IBuildDetailsUpdater>();
        var customerOptionsReader = provider.GetRequiredService<ICustomerOptionsReader>();
        var customerShippingUpdater = provider.GetRequiredService<ICustomerShippingUpdater>();
        var hardwareDetailsUpdater = provider.GetRequiredService<IHardwareDetailsUpdater>();
        var softwareFirmwareUpdater = provider.GetRequiredService<ISoftwareFirmwareUpdater>();
        var networkNotesUpdater = provider.GetRequiredService<INetworkNotesUpdater>();
        var recentlyViewedTracker = provider.GetRequiredService<IRecentlyViewedBuildRecordTracker>();

        Assert.NotNull(factory);
        Assert.NotNull(databaseInitializer);
        Assert.NotNull(auditService);
        Assert.NotNull(auditHistoryReader);
        Assert.NotNull(secretStore);
        Assert.NotNull(secretService);
        Assert.NotNull(creator);
        Assert.NotNull(homePageReader);
        Assert.NotNull(registerReader);
        Assert.NotNull(searchService);
        Assert.NotNull(detailReader);
        Assert.NotNull(productDetailsUpdater);
        Assert.NotNull(buildDetailsUpdater);
        Assert.NotNull(customerOptionsReader);
        Assert.NotNull(customerShippingUpdater);
        Assert.NotNull(hardwareDetailsUpdater);
        Assert.NotNull(softwareFirmwareUpdater);
        Assert.NotNull(networkNotesUpdater);
        Assert.NotNull(recentlyViewedTracker);
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

    [Fact]
    public void WebProgramInitializesDatabaseBeforeServingRequests()
    {
        var programPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Web",
            "Program.cs");
        var programContent = File.ReadAllText(programPath);
        var initializerPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "Persistence",
            "BuildBookDatabaseInitializer.cs");
        var initializerContent = File.ReadAllText(initializerPath);

        Assert.Contains("await InitializeDatabaseAsync(app, logger);", programContent);
        Assert.Contains("BuildBook database initialization failed. The application will not start.", programContent);
        Assert.Contains("BuildBook database does not exist yet. Creating database and applying", initializerContent);
        Assert.Contains("BuildBook database was created and initialized.", initializerContent);
        Assert.Contains("BuildBook database migration completed.", initializerContent);
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

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
