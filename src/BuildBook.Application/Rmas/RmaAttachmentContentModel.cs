namespace BuildBook.Application.Rmas;

public sealed record RmaAttachmentContentModel(
    string FileName,
    string ContentType,
    byte[] Content);
