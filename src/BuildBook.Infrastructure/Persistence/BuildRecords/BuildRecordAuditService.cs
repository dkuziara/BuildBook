using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRecordAuditService : IBuildRecordAuditService
{
    public BuildRecordAudit CreateRecordCreatedEntry(BuildRecord buildRecord, string userName)
    {
        return CreateAuditEntry(
            buildRecord,
            NormalizeUserName(userName),
            AuditAction.Created,
            fieldChanged: null,
            oldValue: null,
            newValue: "Build Record created.");
    }

    public IReadOnlyList<BuildRecordAudit> CreateRecordUpdatedEntries(
        BuildRecord buildRecord,
        IReadOnlyCollection<BuildRecordAuditChange> changes,
        string userName)
    {
        var normalizedUserName = NormalizeUserName(userName);

        return changes
            .Where(change => !string.Equals(change.OldValue, change.NewValue, StringComparison.Ordinal))
            .Select(change => CreateAuditEntry(
                buildRecord,
                normalizedUserName,
                AuditAction.Updated,
                change.FieldChanged,
                change.OldValue,
                change.NewValue))
            .ToArray();
    }

    public BuildRecordAudit CreateSensitiveValueViewedEntry(
        BuildRecord buildRecord,
        string fieldChanged,
        string userName)
    {
        return CreateAuditEntry(
            buildRecord,
            NormalizeUserName(userName),
            AuditAction.SensitiveValueViewed,
            fieldChanged,
            oldValue: null,
            newValue: null);
    }

    public BuildRecordAudit CreateSensitiveValueChangedEntry(
        BuildRecord buildRecord,
        string fieldChanged,
        string userName)
    {
        return CreateAuditEntry(
            buildRecord,
            NormalizeUserName(userName),
            AuditAction.SensitiveValueChanged,
            fieldChanged,
            oldValue: null,
            newValue: null);
    }

    private static BuildRecordAudit CreateAuditEntry(
        BuildRecord buildRecord,
        string userName,
        AuditAction action,
        string? fieldChanged,
        string? oldValue,
        string? newValue)
    {
        return new BuildRecordAudit
        {
            BuildRecord = buildRecord,
            BuildRecordId = buildRecord.Id,
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
