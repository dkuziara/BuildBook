namespace BuildBook.Domain.Rmas;

public sealed class RmaNote
{
    public int Id { get; set; }

    public int RmaRecordId { get; set; }

    public RmaRecord? RmaRecord { get; set; }

    public RmaNoteType NoteType { get; set; }

    public string NoteText { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? LastUpdatedBy { get; set; }

    public DateTimeOffset? LastUpdatedAt { get; set; }
}
