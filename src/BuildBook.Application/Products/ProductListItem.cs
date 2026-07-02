namespace BuildBook.Application.Products;

public sealed record ProductListItem(
    int Id,
    string ProductCode,
    string? Description,
    string? Notes,
    DateTimeOffset LastUpdatedAt);
