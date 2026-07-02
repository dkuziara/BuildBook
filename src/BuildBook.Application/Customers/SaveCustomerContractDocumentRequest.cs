namespace BuildBook.Application.Customers;

public sealed class SaveCustomerContractDocumentRequest
{
    public string FileName { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string? Description { get; set; }
}
