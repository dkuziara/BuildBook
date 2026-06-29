namespace BuildBook.Application.Rmas;

public sealed record RmaAttachmentModel(
    int Id,
    string FileName,
    string ContentType,
    string AttachmentType,
    string? Description,
    string UploadedBy,
    DateTimeOffset UploadedAt);
