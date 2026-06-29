using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public interface IRmaAuditService
{
    RmaAudit CreateRecordCreatedEntry(RmaRecord rmaRecord, string userName);

    IReadOnlyList<RmaAudit> CreateRecordUpdatedEntries(
        RmaRecord rmaRecord,
        IReadOnlyCollection<RmaAuditChange> changes,
        string userName);
}
