namespace BuildBook.Application.Rmas;

public sealed class SaveRmaAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public string AttachmentType { get; set; } = string.Empty;

    public string? Description { get; set; }
}
