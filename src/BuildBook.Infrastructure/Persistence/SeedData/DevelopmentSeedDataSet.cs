using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;

namespace BuildBook.Infrastructure.Persistence.SeedData;

public sealed record DevelopmentSeedDataSet(
    IReadOnlyCollection<Customer> Customers,
    IReadOnlyCollection<BuildRecord> BuildRecords,
    ImportBatch ImportBatch,
    IReadOnlyCollection<BuildRecordAudit> AuditEntries,
    IReadOnlyCollection<RmaRecord> RmaRecords,
    IReadOnlyCollection<RmaChecklistItem> RmaChecklistItems,
    IReadOnlyCollection<RmaNote> RmaNotes,
    IReadOnlyCollection<RmaCommunication> RmaCommunications,
    IReadOnlyCollection<RmaAttachment> RmaAttachments,
    IReadOnlyCollection<RmaPart> RmaParts,
    IReadOnlyCollection<RmaStatusHistory> RmaStatusHistoryEntries,
    IReadOnlyCollection<RmaAudit> RmaAuditEntries);
