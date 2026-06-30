using BuildBook.Application.BuildRecords;
using BuildBook.Application.Rmas;
using BuildBook.Application.Security;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using BuildBook.Infrastructure.Persistence.Rmas;
using BuildBook.Infrastructure.Persistence.Security;
using BuildBook.Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildBook.Infrastructure;

public static class DependencyInjection
{
    public const string BuildBookDatabaseConnectionName = "BuildBookDatabase";

    public static IServiceCollection AddBuildBookInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(BuildBookDatabaseConnectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{BuildBookDatabaseConnectionName}' is not configured.");
        }

        services.AddDbContextFactory<BuildBookDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(BuildBookDbContext).Assembly.FullName));

            options.EnableDetailedErrors(IsDetailedErrorsEnabled(configuration));
            options.EnableSensitiveDataLogging(false);
        });

        services.AddScoped<DevelopmentDataSeeder>();
        services.AddScoped<BuildBookDatabaseInitializer>();
        var dataProtectionBuilder = services.AddDataProtection()
            .SetApplicationName("BuildBook");
        var dataProtectionKeyDirectory = configuration["BuildBook:DataProtectionKeyDirectory"];
        if (!string.IsNullOrWhiteSpace(dataProtectionKeyDirectory))
        {
            dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyDirectory));
        }

        services.AddScoped<IBuildRecordAuditService, BuildRecordAuditService>();
        services.AddScoped<IBuildRecordAuditHistoryReader, BuildRecordAuditHistoryReader>();
        services.AddScoped<IBuildRecordSecretStore, BuildRecordSecretStore>();
        services.AddScoped<IBuildRecordSecretService, BuildRecordSecretService>();
        services.AddScoped<IBuildRecordCreator, BuildRecordCreator>();
        services.AddScoped<IHomePageReader, HomePageReader>();
        services.AddScoped<IImportHistoryReader, ImportHistoryReader>();
        services.AddScoped<IBuildRegisterCsvExporter, BuildRegisterCsvExporter>();
        services.AddScoped<IBuildRegisterExcelExporter, BuildRegisterExcelExporter>();
        services.AddScoped<IBuildRegisterReader, BuildRegisterReader>();
        services.AddScoped<IMissingDataReportReader, MissingDataReportReader>();
        services.AddScoped<IBuildRecordSearchService, BuildRecordSearchService>();
        services.AddScoped<IBuildRecordDetailReader, BuildRecordDetailReader>();
        services.AddScoped<ISpreadsheetImportMappingService, SpreadsheetImportMappingService>();
        services.AddScoped<IProductDetailsUpdater, ProductDetailsUpdater>();
        services.AddScoped<IBuildDetailsUpdater, BuildDetailsUpdater>();
        services.AddScoped<ICustomerOptionsReader, CustomerOptionsReader>();
        services.AddScoped<ICustomerShippingUpdater, CustomerShippingUpdater>();
        services.AddScoped<IHardwareDetailsUpdater, HardwareDetailsUpdater>();
        services.AddScoped<ISoftwareFirmwareUpdater, SoftwareFirmwareUpdater>();
        services.AddScoped<INetworkNotesUpdater, NetworkNotesUpdater>();
        services.AddScoped<IApplicationUserManagementService, ApplicationUserManagementService>();
        services.AddScoped<IBuildBookRoleResolver, ApplicationUserManagementService>();
        services.AddScoped<IRmaAuditService, RmaAuditService>();
        services.AddSingleton<IRmaAttachmentStorage, LocalRmaAttachmentStorage>();
        services.AddScoped<IRmaNumberGenerator, RmaNumberGenerator>();
        services.AddScoped<IRmaStatusTransitionService, RmaStatusTransitionService>();
        services.AddScoped<IRmaRecordService, RmaRecordService>();
        services.AddScoped<IRmaReportReader, RmaReportReader>();
        services.AddScoped<IRmaReportCsvExporter, RmaReportCsvExporter>();
        services.AddScoped<IRmaReportExcelExporter, RmaReportExcelExporter>();
        services.AddSingleton<IRecentlyViewedBuildRecordTracker, RecentlyViewedBuildRecordTracker>();

        return services;
    }

    private static bool IsDetailedErrorsEnabled(IConfiguration configuration)
    {
        return bool.TryParse(configuration["BuildBook:EnableDetailedErrors"], out var enabled)
            && enabled;
    }
}
