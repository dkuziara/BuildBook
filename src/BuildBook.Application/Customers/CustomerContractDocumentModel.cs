namespace BuildBook.Application.Customers;

public sealed record CustomerContractDocumentModel(
    int Id,
    string FileName,
    string ContentType,
    string DocumentType,
    string? Description,
    string UploadedBy,
    DateTimeOffset UploadedAt);
