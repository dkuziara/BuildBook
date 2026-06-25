using BuildBook.Domain.BuildRecords;

namespace BuildBook.Application.BuildRecords;

public interface IBuildRecordAuditService
{
    BuildRecordAudit CreateRecordCreatedEntry(BuildRecord buildRecord, string userName);

    IReadOnlyList<BuildRecordAudit> CreateRecordUpdatedEntries(
        BuildRecord buildRecord,
        IReadOnlyCollection<BuildRecordAuditChange> changes,
        string userName);

    BuildRecordAudit CreateSensitiveValueViewedEntry(
        BuildRecord buildRecord,
        string fieldChanged,
        string userName);

    BuildRecordAudit CreateSensitiveValueChangedEntry(
        BuildRecord buildRecord,
        string fieldChanged,
        string userName);
}
