namespace BuildBook.Domain.Rmas;

public sealed class RmaAudit
{
    public int Id { get; set; }

    public int RmaRecordId { get; set; }

    public RmaRecord? RmaRecord { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public string User { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? FieldChanged { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Comment { get; set; }
}
