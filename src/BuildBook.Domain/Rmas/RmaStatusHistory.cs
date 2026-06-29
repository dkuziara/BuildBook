namespace BuildBook.Domain.Rmas;

public sealed class RmaStatusHistory
{
    public int Id { get; set; }

    public int RmaRecordId { get; set; }

    public RmaRecord? RmaRecord { get; set; }

    public RmaStatus? OldStatus { get; set; }

    public RmaStatus NewStatus { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? Reason { get; set; }
}
