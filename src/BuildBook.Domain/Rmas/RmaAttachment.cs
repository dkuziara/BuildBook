namespace BuildBook.Domain.Rmas;

public sealed class RmaAttachment
{
    public int Id { get; set; }

    public int RmaRecordId { get; set; }

    public RmaRecord? RmaRecord { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredFilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string AttachmentType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string UploadedBy { get; set; } = string.Empty;

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
