namespace BuildBook.Domain.Rmas;

public sealed class RmaChecklistItem
{
    public int Id { get; set; }

    public int RmaRecordId { get; set; }

    public RmaRecord? RmaRecord { get; set; }

    public int DisplayOrder { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public string? CompletedBy { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public bool ShowInBoardView { get; set; }
}
