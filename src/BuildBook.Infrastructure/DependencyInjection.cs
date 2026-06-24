using BuildBook.Application.BuildRecords;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using BuildBook.Infrastructure.Persistence.SeedData;
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
        services.AddScoped<IBuildRecordCreator, BuildRecordCreator>();
        services.AddScoped<IBuildRegisterReader, BuildRegisterReader>();
        services.AddScoped<IBuildRecordDetailReader, BuildRecordDetailReader>();
        services.AddScoped<IProductDetailsUpdater, ProductDetailsUpdater>();
        services.AddScoped<IBuildDetailsUpdater, BuildDetailsUpdater>();
        services.AddScoped<ICustomerOptionsReader, CustomerOptionsReader>();
        services.AddScoped<ICustomerShippingUpdater, CustomerShippingUpdater>();
        services.AddScoped<IHardwareDetailsUpdater, HardwareDetailsUpdater>();
        services.AddScoped<ISoftwareFirmwareUpdater, SoftwareFirmwareUpdater>();
        services.AddScoped<INetworkNotesUpdater, NetworkNotesUpdater>();

        return services;
    }

    private static bool IsDetailedErrorsEnabled(IConfiguration configuration)
    {
        return bool.TryParse(configuration["BuildBook:EnableDetailedErrors"], out var enabled)
            && enabled;
    }
}
