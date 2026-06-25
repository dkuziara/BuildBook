using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;

namespace BuildBook.Tests;

public class BuildRecordAuditHistoryTests
{
    [Fact]
    public void AuditHistoryEntryContainsExpectedFields()
    {
        var entry = new BuildRecordAuditHistoryEntry(
            12,
            new DateTimeOffset(2026, 6, 25, 12, 30, 0, TimeSpan.Zero),
            "DOMAIN\\alice",
            AuditAction.Updated,
            "RadSightVersion",
            "1.3.6.1946",
            "1.3.7.2001");

        Assert.Equal(12, entry.Id);
        Assert.Equal("DOMAIN\\alice", entry.User);
        Assert.Equal(AuditAction.Updated, entry.Action);
        Assert.Equal("RadSightVersion", entry.FieldChanged);
        Assert.Equal("1.3.6.1946", entry.OldValue);
        Assert.Equal("1.3.7.2001", entry.NewValue);
    }
}
