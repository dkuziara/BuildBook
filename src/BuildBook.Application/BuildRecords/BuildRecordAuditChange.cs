namespace BuildBook.Application.BuildRecords;

public sealed record BuildRecordAuditChange(
    string FieldChanged,
    string? OldValue,
    string? NewValue);
