using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class MissingDataReportReaderTests
{
    [Fact]
    public async Task ListActiveAsync_ReportsMissingCustomerRecoveryKeyAndDateShipped()
    {
        var databaseName = $"BuildBookMissingDataReader_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var setupContext = new BuildBookDbContext(options);
        await setupContext.Database.MigrateAsync();

        var customer = new Customer { Name = "APVL", IsActive = true };
        setupContext.Customers.Add(customer);

        var completeRecord = new BuildRecord
        {
            ProductCode = "CDM61120",
            ProductName = "Complete Device",
            SerialNumber = "3000001",
            Customer = customer,
            DateShipped = new DateOnly(2026, 6, 26),
            CreatedBy = "tester",
            LastUpdatedBy = "tester"
        };
        var missingRecord = new BuildRecord
        {
            ProductCode = "CDM61121",
            ProductName = "Incomplete Device",
            SerialNumber = "3000002",
            CreatedBy = "tester",
            LastUpdatedBy = "tester"
        };
        var inactiveRecord = new BuildRecord
        {
            ProductCode = "CDM61122",
            ProductName = "Inactive Device",
            SerialNumber = "3000003",
            CreatedBy = "tester",
            LastUpdatedBy = "tester",
            IsActive = false
        };

        setupContext.BuildRecords.AddRange(completeRecord, missingRecord, inactiveRecord);
        await setupContext.SaveChangesAsync();

        setupContext.BuildRecordSecrets.Add(new BuildRecordSecret
        {
            BuildRecordId = completeRecord.Id,
            SecretType = SecretType.BitLockerRecoveryKey,
            SecretValueEncrypted = [1, 2, 3],
            CreatedBy = "tester",
            LastUpdatedBy = "tester"
        });
        await setupContext.SaveChangesAsync();

        var reader = new MissingDataReportReader(new TestDbContextFactory(options));

        var rows = await reader.ListActiveAsync();

        Assert.Equal(2, rows.Count);

        var completeRow = Assert.Single(rows, row => row.SerialNumber == "3000001");
        Assert.False(completeRow.IsMissingCustomer);
        Assert.False(completeRow.IsMissingRecoveryData);
        Assert.False(completeRow.IsMissingDateShipped);

        var missingRow = Assert.Single(rows, row => row.SerialNumber == "3000002");
        Assert.True(missingRow.IsMissingCustomer);
        Assert.True(missingRow.IsMissingRecoveryData);
        Assert.True(missingRow.IsMissingDateShipped);

        Assert.DoesNotContain(rows, row => row.SerialNumber == "3000003");

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
