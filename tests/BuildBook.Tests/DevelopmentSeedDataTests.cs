using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Application.Settings;
using BuildBook.Infrastructure.Persistence.SeedData;

namespace BuildBook.Tests;

public class DevelopmentSeedDataTests
{
    [Fact]
    public void DevelopmentSeedDataIncludesRealisticBuildRecords()
    {
        var seedData = DevelopmentSeedData.Create();

        Assert.Equal(3, seedData.SupportContractLevels.Count);
        Assert.Equal(3, seedData.Customers.Count);
        Assert.Single(seedData.SystemSettings, setting => setting.Key == SystemSettingKeys.SupportTicketUrlTemplate);
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
    public void DevelopmentSeedDataIncludesEditableSupportContractExamples()
    {
        var seedData = DevelopmentSeedData.Create();

        Assert.Contains(seedData.SupportContractLevels, level => level.Name == "Bronze");
        Assert.Contains(seedData.SupportContractLevels, level => level.Name == "Silver");
        Assert.Contains(seedData.SupportContractLevels, level => level.Name == "Gold");
        Assert.Contains(seedData.Customers, customer => customer.SupportContractStatus == CustomerSupportContractStatuses.NoContract);
        Assert.Contains(seedData.Customers, customer => customer.SupportContractLevel is not null);
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

    [Fact]
    public void DevelopmentSeedDataIncludesLinkedAndUnlinkedRmaRecords()
    {
        var seedData = DevelopmentSeedData.Create();

        Assert.Equal(2, seedData.RmaRecords.Count);
        Assert.Contains(seedData.RmaRecords, rmaRecord => rmaRecord.BuildRecord is not null);
        Assert.Contains(seedData.RmaRecords, rmaRecord => rmaRecord.BuildRecord is null);
        Assert.Contains(seedData.RmaRecords, rmaRecord => rmaRecord.Status == RmaStatus.WorkInProgress);
        Assert.All(seedData.RmaRecords, rmaRecord =>
        {
            Assert.False(string.IsNullOrWhiteSpace(rmaRecord.RmaNumber));
            Assert.False(string.IsNullOrWhiteSpace(rmaRecord.ProductName));
            Assert.False(string.IsNullOrWhiteSpace(rmaRecord.FaultSummary));
        });
    }

    [Fact]
    public void DevelopmentSeedDataIncludesSupportingRmaOperationalData()
    {
        var seedData = DevelopmentSeedData.Create();

        Assert.NotEmpty(seedData.RmaChecklistItems);
        Assert.NotEmpty(seedData.RmaNotes);
        Assert.NotEmpty(seedData.RmaCommunications);
        Assert.NotEmpty(seedData.RmaAttachments);
        Assert.NotEmpty(seedData.RmaParts);
        Assert.NotEmpty(seedData.RmaStatusHistoryEntries);
        Assert.NotEmpty(seedData.RmaAuditEntries);
    }
}
