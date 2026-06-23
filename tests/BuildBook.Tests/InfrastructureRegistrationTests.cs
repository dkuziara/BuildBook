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

        Assert.NotNull(factory);
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
