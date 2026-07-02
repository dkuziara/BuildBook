namespace BuildBook.Application.Products;

public sealed record ProductDetailModel(
    int Id,
    string ProductCode,
    string? Description,
    string? Notes,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset LastUpdatedAt,
    string LastUpdatedBy,
    IReadOnlyList<ProductLinkedBuildRecord> LinkedBuildRecords,
    IReadOnlyList<ProductLinkedOrder> LinkedOrders,
    IReadOnlyList<ProductLinkedRma> LinkedRmas);

public sealed record ProductLinkedBuildRecord(
    int Id,
    string SerialNumber,
    string ProductName,
    string? CustomerName,
    DateOnly? DateShipped,
    DateTimeOffset LastUpdatedAt);

public sealed record ProductLinkedOrder(
    int Id,
    string OrderTitle,
    string Status,
    string? CustomerName,
    DateOnly? DueDate,
    DateTimeOffset LastUpdatedAt);

public sealed record ProductLinkedRma(
    int Id,
    string RmaNumber,
    string Status,
    string ProductName,
    string? SerialNumber,
    string FaultSummary,
    DateOnly? DueDate,
    DateTimeOffset LastUpdatedAt);
