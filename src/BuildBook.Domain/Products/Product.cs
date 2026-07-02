namespace BuildBook.Domain.Products;

public sealed class Product
{
    public int Id { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = "Unknown";

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = "Unknown";
}
