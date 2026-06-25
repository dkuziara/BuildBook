using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using BuildBook.Infrastructure.Persistence.BuildRecords;

namespace BuildBook.Tests;

public class BuildRecordAuditServiceTests
{
    [Fact]
    public void CreateRecordCreatedEntry_UsesCreatedAction()
    {
        var service = new BuildRecordAuditService();
        var buildRecord = new BuildRecord { Id = 42 };

        var entry = service.CreateRecordCreatedEntry(buildRecord, "DOMAIN\\alice");

        Assert.Equal(AuditAction.Created, entry.Action);
        Assert.Equal(42, entry.BuildRecordId);
        Assert.Equal("DOMAIN\\alice", entry.User);
        Assert.Null(entry.FieldChanged);
        Assert.Equal("Build Record created.", entry.NewValue);
    }

    [Fact]
    public void CreateRecordUpdatedEntries_ReturnsOnlyChangedFields()
    {
        var service = new BuildRecordAuditService();
        var buildRecord = new BuildRecord { Id = 7 };

        var entries = service.CreateRecordUpdatedEntries(
            buildRecord,
            [
                new BuildRecordAuditChange("ProductCode", "OLD-1", "NEW-1"),
                new BuildRecordAuditChange("ProductName", "Same", "Same")
            ],
            "editor");

        var entry = Assert.Single(entries);
        Assert.Equal(AuditAction.Updated, entry.Action);
        Assert.Equal("ProductCode", entry.FieldChanged);
        Assert.Equal("OLD-1", entry.OldValue);
        Assert.Equal("NEW-1", entry.NewValue);
    }

    [Fact]
    public void SensitiveAuditEntries_DoNotStoreSecretValues()
    {
        var service = new BuildRecordAuditService();
        var buildRecord = new BuildRecord { Id = 9 };

        var viewedEntry = service.CreateSensitiveValueViewedEntry(buildRecord, "RouterPassword", "viewer");
        var changedEntry = service.CreateSensitiveValueChangedEntry(buildRecord, "RouterPassword", "editor");

        Assert.Equal(AuditAction.SensitiveValueViewed, viewedEntry.Action);
        Assert.Equal(AuditAction.SensitiveValueChanged, changedEntry.Action);
        Assert.Equal("RouterPassword", viewedEntry.FieldChanged);
        Assert.Equal("RouterPassword", changedEntry.FieldChanged);
        Assert.Null(viewedEntry.OldValue);
        Assert.Null(viewedEntry.NewValue);
        Assert.Null(changedEntry.OldValue);
        Assert.Null(changedEntry.NewValue);
    }
}
