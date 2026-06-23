using BuildBook.Domain.BuildRecords;
using BuildBook.Infrastructure.Persistence.SeedData;

namespace BuildBook.Tests;

public class DevelopmentSeedDataTests
{
    [Fact]
    public void DevelopmentSeedDataIncludesRealisticBuildRecords()
    {
        var seedData = DevelopmentSeedData.Create();

        Assert.Equal(3, seedData.Customers.Count);
        Assert.Equal(3, seedData.BuildRecords.Count);
        Assert.All(seedData.BuildRecords, buildRecord =>
        {
            Assert.False(string.IsNullOrWhiteSpace(buildRecord.ProductCode));
            Assert.False(string.IsNullOrWhiteSpace(buildRecord.ProductName));
            Assert.False(string.IsNullOrWhiteSpace(buildRecord.SerialNumber));
            Assert.NotNull(buildRecord.Customer);
            Assert.False(string.IsNullOrWhiteSpace(buildRecord.MachineName));
            Assert.False(string.IsNullOrWhiteSpace(buildRecord.RadSightVersion));
            Assert.False(string.IsNullOrWhiteSpace(buildRecord.WindowsVersion));
        });
    }

    [Fact]
    public void DevelopmentSeedDataDoesNotSeedSecrets()
    {
        var seedData = DevelopmentSeedData.Create();

        Assert.All(seedData.BuildRecords, buildRecord => Assert.Empty(buildRecord.Secrets));
    }

    [Fact]
    public void DevelopmentSeedDataIncludesImportAuditEntries()
    {
        var seedData = DevelopmentSeedData.Create();

        Assert.Equal(seedData.BuildRecords.Count, seedData.AuditEntries.Count);
        Assert.All(seedData.AuditEntries, auditEntry =>
        {
            Assert.Equal(AuditAction.ImportPerformed, auditEntry.Action);
            Assert.NotNull(auditEntry.BuildRecord);
            Assert.Same(seedData.ImportBatch, auditEntry.ImportBatch);
        });
    }
}
