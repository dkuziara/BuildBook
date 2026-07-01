using BuildBook.Application.BuildRecords;
using BuildBook.Application.Customers;
using BuildBook.Application.Orders;
using BuildBook.Application.Rmas;
using BuildBook.Application.Security;
using BuildBook.Application.Settings;
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
        services.AddSingleton(configuration);

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
        var importHistoryReader = provider.GetRequiredService<IImportHistoryReader>();
        var buildRegisterCsvExporter = provider.GetRequiredService<IBuildRegisterCsvExporter>();
        var buildRegisterExcelExporter = provider.GetRequiredService<IBuildRegisterExcelExporter>();
        var registerReader = provider.GetRequiredService<IBuildRegisterReader>();
        var missingDataReportReader = provider.GetRequiredService<IMissingDataReportReader>();
        var searchService = provider.GetRequiredService<IBuildRecordSearchService>();
        var detailReader = provider.GetRequiredService<IBuildRecordDetailReader>();
        var orderPlannerImportService = provider.GetRequiredService<IOrderPlannerImportService>();
        var orderRegisterReader = provider.GetRequiredService<IOrderRegisterReader>();
        var orderRecordCreator = provider.GetRequiredService<IOrderRecordCreator>();
        var orderDetailReader = provider.GetRequiredService<IOrderDetailReader>();
        var productDetailsUpdater = provider.GetRequiredService<IProductDetailsUpdater>();
        var buildDetailsUpdater = provider.GetRequiredService<IBuildDetailsUpdater>();
        var customerOptionsReader = provider.GetRequiredService<ICustomerOptionsReader>();
        var customerService = provider.GetRequiredService<ICustomerService>();
        var customerShippingUpdater = provider.GetRequiredService<ICustomerShippingUpdater>();
        var supportContractLevelService = provider.GetRequiredService<ISupportContractLevelService>();
        var systemSettingsService = provider.GetRequiredService<ISystemSettingsService>();
        var hardwareDetailsUpdater = provider.GetRequiredService<IHardwareDetailsUpdater>();
        var softwareFirmwareUpdater = provider.GetRequiredService<ISoftwareFirmwareUpdater>();
        var networkNotesUpdater = provider.GetRequiredService<INetworkNotesUpdater>();
        var applicationUserManagementService = provider.GetRequiredService<IApplicationUserManagementService>();
        var buildBookRoleResolver = provider.GetRequiredService<IBuildBookRoleResolver>();
        var rmaReportReader = provider.GetRequiredService<IRmaReportReader>();
        var rmaReportCsvExporter = provider.GetRequiredService<IRmaReportCsvExporter>();
        var rmaReportExcelExporter = provider.GetRequiredService<IRmaReportExcelExporter>();
        var recentlyViewedTracker = provider.GetRequiredService<IRecentlyViewedBuildRecordTracker>();

        Assert.NotNull(factory);
        Assert.NotNull(databaseInitializer);
        Assert.NotNull(auditService);
        Assert.NotNull(auditHistoryReader);
        Assert.NotNull(secretStore);
        Assert.NotNull(secretService);
        Assert.NotNull(creator);
        Assert.NotNull(homePageReader);
        Assert.NotNull(importHistoryReader);
        Assert.NotNull(buildRegisterCsvExporter);
        Assert.NotNull(buildRegisterExcelExporter);
        Assert.NotNull(registerReader);
        Assert.NotNull(missingDataReportReader);
        Assert.NotNull(searchService);
        Assert.NotNull(detailReader);
        Assert.NotNull(orderPlannerImportService);
        Assert.NotNull(orderRegisterReader);
        Assert.NotNull(orderRecordCreator);
        Assert.NotNull(orderDetailReader);
        Assert.NotNull(productDetailsUpdater);
        Assert.NotNull(buildDetailsUpdater);
        Assert.NotNull(customerOptionsReader);
        Assert.NotNull(customerService);
        Assert.NotNull(customerShippingUpdater);
        Assert.NotNull(supportContractLevelService);
        Assert.NotNull(systemSettingsService);
        Assert.NotNull(hardwareDetailsUpdater);
        Assert.NotNull(softwareFirmwareUpdater);
        Assert.NotNull(networkNotesUpdater);
        Assert.NotNull(applicationUserManagementService);
        Assert.NotNull(buildBookRoleResolver);
        Assert.NotNull(rmaReportReader);
        Assert.NotNull(rmaReportCsvExporter);
        Assert.NotNull(rmaReportExcelExporter);
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
        Assert.Contains(migrations, migration => migration.EndsWith("_AddOrdersModuleDataModel", StringComparison.Ordinal));
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

    [Fact]
    public void DatabaseInitializerSeedsSupportContractLevelsAndSystemSettings()
    {
        var initializerPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "Persistence",
            "BuildBookDatabaseInitializer.cs");
        var initializerContent = File.ReadAllText(initializerPath);

        Assert.Contains("EnsureSupportContractLevelsAsync", initializerContent);
        Assert.Contains("EnsureSystemSettingsAsync", initializerContent);
        Assert.Contains("\"Bronze\"", initializerContent);
        Assert.Contains("\"Silver\"", initializerContent);
        Assert.Contains("\"Gold\"", initializerContent);
        Assert.Contains(nameof(BuildBookOrderStatuses.SerializeDefaultWorkflow), initializerContent);
        Assert.Contains(SystemSettingKeys.OrderWorkflowStatuses, initializerContent);
        Assert.Contains(SystemSettingKeys.SupportTicketUrlTemplate, initializerContent);
        Assert.Contains(SystemSettingKeys.SupportTicketLabel, initializerContent);
    }

    [Fact]
    public void OrdersMigrationCreatesCoreOrdersTables()
    {
        var migrationPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "Persistence",
            "Migrations",
            "20260701140000_AddOrdersModuleDataModel.cs");
        var migrationContent = File.ReadAllText(migrationPath);

        Assert.Contains("CreateTable(", migrationContent);
        Assert.Contains("\"OrderRecords\"", migrationContent);
        Assert.Contains("\"OrderAssignments\"", migrationContent);
        Assert.Contains("\"OrderChecklistItems\"", migrationContent);
        Assert.Contains("\"OrderNotes\"", migrationContent);
        Assert.Contains("\"OrderLabels\"", migrationContent);
        Assert.Contains("\"OrderBuildRecordLinks\"", migrationContent);
        Assert.Contains("\"OrderStatusHistory\"", migrationContent);
        Assert.Contains("\"OrderImportBatches\"", migrationContent);
        Assert.Contains("\"OrderImportWarnings\"", migrationContent);
        Assert.Contains("IX_OrderRecords_PlannerTaskId", migrationContent);
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
