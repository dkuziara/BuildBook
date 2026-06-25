using BuildBook.Domain.BuildRecords;

namespace BuildBook.Application.BuildRecords;

public sealed record ImportHistoryEntry(
    int Id,
    string SourceFileName,
    DateTimeOffset ImportedAt,
    string ImportedBy,
    ImportStatus Status,
    int RowsRead,
    int RecordsCreated,
    int RecordsSkipped,
    int WarningCount,
    int ErrorCount,
    string Summary);
