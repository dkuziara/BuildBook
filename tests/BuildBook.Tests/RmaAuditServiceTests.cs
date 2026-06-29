using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Rmas;

namespace BuildBook.Tests;

public class RmaAuditServiceTests
{
    [Fact]
    public void CreateRecordCreatedEntry_DoesNotPresetForeignKeyForUnsavedRma()
    {
        var service = new RmaAuditService();
        var rmaRecord = new RmaRecord();
        var before = DateTimeOffset.UtcNow;

        var entry = service.CreateRecordCreatedEntry(rmaRecord, "DOMAIN\\alice");
        var after = DateTimeOffset.UtcNow;

        Assert.Same(rmaRecord, entry.RmaRecord);
        Assert.Equal(0, entry.RmaRecordId);
        Assert.Equal("Created", entry.Action);
        Assert.Equal("DOMAIN\\alice", entry.User);
        Assert.Equal("RMA Record created.", entry.NewValue);
        Assert.InRange(entry.OccurredAt, before, after);
    }

    [Fact]
    public void CreateRecordUpdatedEntries_ReturnsChangedFieldHistory()
    {
        var service = new RmaAuditService();
        var rmaRecord = new RmaRecord { Id = 7 };
        var before = DateTimeOffset.UtcNow;

        var entries = service.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("ProductCode", "OLD-1", "NEW-1"),
                new RmaAuditChange("FaultSummary", "Same", "Same")
            ],
            "editor");
        var after = DateTimeOffset.UtcNow;

        var entry = Assert.Single(entries);
        Assert.Same(rmaRecord, entry.RmaRecord);
        Assert.Equal(0, entry.RmaRecordId);
        Assert.Equal("Updated", entry.Action);
        Assert.Equal("editor", entry.User);
        Assert.Equal("ProductCode", entry.FieldChanged);
        Assert.Equal("OLD-1", entry.OldValue);
        Assert.Equal("NEW-1", entry.NewValue);
        Assert.InRange(entry.OccurredAt, before, after);
    }
}
