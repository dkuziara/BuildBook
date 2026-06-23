using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;

namespace BuildBook.Infrastructure.Persistence.SeedData;

public sealed record DevelopmentSeedDataSet(
    IReadOnlyCollection<Customer> Customers,
    IReadOnlyCollection<BuildRecord> BuildRecords,
    ImportBatch ImportBatch,
    IReadOnlyCollection<BuildRecordAudit> AuditEntries);
