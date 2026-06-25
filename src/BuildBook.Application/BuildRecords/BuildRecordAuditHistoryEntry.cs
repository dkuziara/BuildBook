using BuildBook.Domain.BuildRecords;

namespace BuildBook.Application.BuildRecords;

public sealed record BuildRecordAuditHistoryEntry(
    int Id,
    DateTimeOffset OccurredAt,
    string User,
    AuditAction Action,
    string? FieldChanged,
    string? OldValue,
    string? NewValue);
