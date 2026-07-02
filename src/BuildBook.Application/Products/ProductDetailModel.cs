namespace BuildBook.Application.Products;

public sealed record ProductDetailModel(
    int Id,
    string ProductCode,
    string? Description,
    string? Notes,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset LastUpdatedAt,
    string LastUpdatedBy);
