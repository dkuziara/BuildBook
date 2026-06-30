using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Domain.Settings;

namespace BuildBook.Infrastructure.Persistence.SeedData;

public sealed record DevelopmentSeedDataSet(
    IReadOnlyCollection<SupportContractLevel> SupportContractLevels,
    IReadOnlyCollection<Customer> Customers,
    IReadOnlyCollection<SystemSetting> SystemSettings,
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
