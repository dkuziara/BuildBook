using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public sealed class RmaAuditService : IRmaAuditService
{
    public RmaAudit CreateRecordCreatedEntry(RmaRecord rmaRecord, string userName)
    {
        return CreateAuditEntry(
            rmaRecord,
            NormalizeUserName(userName),
            "Created",
            fieldChanged: null,
            oldValue: null,
            newValue: "RMA Record created.");
    }

    public IReadOnlyList<RmaAudit> CreateRecordUpdatedEntries(
        RmaRecord rmaRecord,
        IReadOnlyCollection<RmaAuditChange> changes,
        string userName)
    {
        var normalizedUserName = NormalizeUserName(userName);

        return changes
            .Where(change => !string.Equals(change.OldValue, change.NewValue, StringComparison.Ordinal))
            .Select(change => CreateAuditEntry(
                rmaRecord,
                normalizedUserName,
                "Updated",
                change.FieldChanged,
                change.OldValue,
                change.NewValue))
            .ToArray();
    }

    private static RmaAudit CreateAuditEntry(
        RmaRecord rmaRecord,
        string userName,
        string action,
        string? fieldChanged,
        string? oldValue,
        string? newValue)
    {
        return new RmaAudit
        {
            RmaRecord = rmaRecord,
            OccurredAt = DateTimeOffset.UtcNow,
            User = userName,
            Action = action,
            FieldChanged = fieldChanged,
            OldValue = oldValue,
            NewValue = newValue
        };
    }

    private static string NormalizeUserName(string userName)
    {
        return string.IsNullOrWhiteSpace(userName) ? "Unknown" : userName.Trim();
    }
}
