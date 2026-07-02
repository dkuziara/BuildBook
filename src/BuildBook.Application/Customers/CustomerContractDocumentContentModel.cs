namespace BuildBook.Application.Customers;

public sealed record CustomerContractDocumentContentModel(
    string FileName,
    string ContentType,
    byte[] Content);
