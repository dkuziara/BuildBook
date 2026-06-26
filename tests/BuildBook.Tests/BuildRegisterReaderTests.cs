using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class BuildRegisterReaderTests
{
    [Fact]
    public async Task ListAsync_FiltersByExactCustomerIdAndSortsByLastUpdatedDescendingByDefault()
    {
        var databaseName = $"BuildBookRegisterReader_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var setupContext = new BuildBookDbContext(options);
        await setupContext.Database.MigrateAsync();

        var customerAlpha = new Customer { Name = "Alpha Site", IsActive = true };
        var customerAlphaLab = new Customer { Name = "Alpha Site Lab", IsActive = true };
        setupContext.Customers.AddRange(customerAlpha, customerAlphaLab);

        setupContext.BuildRecords.AddRange(
            new BuildRecord
            {
                ProductCode = "CDM61100",
                ProductName = "Terminal A",
                SerialNumber = "1000001",
                Customer = customerAlpha,
                RadSightVersion = "1.3.6",
                WindowsVersion = "Windows 10",
                CreatedBy = "tester",
                LastUpdatedBy = "tester",
                LastUpdatedAt = new DateTimeOffset(2026, 6, 26, 8, 0, 0, TimeSpan.Zero)
            },
            new BuildRecord
            {
                ProductCode = "CDM61101",
                ProductName = "Terminal B",
                SerialNumber = "1000002",
                Customer = customerAlpha,
                RadSightVersion = "1.3.7",
                WindowsVersion = "Windows 11",
                CreatedBy = "tester",
                LastUpdatedBy = "tester",
                LastUpdatedAt = new DateTimeOffset(2026, 6, 26, 9, 0, 0, TimeSpan.Zero)
            },
            new BuildRecord
            {
                ProductCode = "CDM61102",
                ProductName = "Terminal C",
                SerialNumber = "1000003",
                Customer = customerAlphaLab,
                RadSightVersion = "1.3.8",
                WindowsVersion = "Windows 11",
                CreatedBy = "tester",
                LastUpdatedBy = "tester",
                LastUpdatedAt = new DateTimeOffset(2026, 6, 26, 10, 0, 0, TimeSpan.Zero)
            });

        await setupContext.SaveChangesAsync();

        var reader = new BuildRegisterReader(new TestDbContextFactory(options));

        var rows = await reader.ListAsync(new BuildRegisterFilter { CustomerId = customerAlpha.Id });

        Assert.Equal(["1000002", "1000001"], rows.Select(row => row.SerialNumber).ToArray());

        await using var cleanupContext = new BuildBookDbContext(options);
        await cleanupContext.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task ListAsync_FiltersByPartialVersionFields()
    {
        var databaseName = $"BuildBookRegisterReader_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var setupContext = new BuildBookDbContext(options);
        await setupContext.Database.MigrateAsync();

        setupContext.BuildRecords.AddRange(
            new BuildRecord
            {
                ProductCode = "CDM61110",
                ProductName = "Device One",
                SerialNumber = "2000001",
                RadSightVersion = "1.3.6.1946",
                WindowsVersion = "Windows 10 IoT",
                CreatedBy = "tester",
                LastUpdatedBy = "tester"
            },
            new BuildRecord
            {
                ProductCode = "CDM61111",
                ProductName = "Device Two",
                SerialNumber = "2000002",
                RadSightVersion = "1.4.0.1000",
                WindowsVersion = "Windows 11 Pro",
                CreatedBy = "tester",
                LastUpdatedBy = "tester"
            });

        await setupContext.SaveChangesAsync();

        var reader = new BuildRegisterReader(new TestDbContextFactory(options));

        var rows = await reader.ListAsync(new BuildRegisterFilter
        {
            RadSightVersion = "1.3.6",
            WindowsVersion = "IoT"
        });

        var row = Assert.Single(rows);
        Assert.Equal("2000001", row.SerialNumber);

        await using var cleanupContext = new BuildBookDbContext(options);
        await cleanupContext.Database.EnsureDeletedAsync();
    }

    private sealed class TestDbContextFactory(DbContextOptions<BuildBookDbContext> options) : IDbContextFactory<BuildBookDbContext>
    {
        public BuildBookDbContext CreateDbContext() => new(options);

        public Task<BuildBookDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new BuildBookDbContext(options));
        }
    }
}
