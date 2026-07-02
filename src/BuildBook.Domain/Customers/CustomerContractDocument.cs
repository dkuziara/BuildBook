namespace BuildBook.Domain.Customers;

public sealed class CustomerContractDocument
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredFilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string UploadedBy { get; set; } = string.Empty;

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
