namespace BuildBook.Application.Rmas;

public sealed record RmaAuditChange(
    string FieldChanged,
    string? OldValue,
    string? NewValue);
