using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed record OrderRegisterRow(
    int Id,
    string OrderNumber,
    string OrderTitle,
    string? ProductCode,
    int? LinkedProductId,
    string Status,
    string? CustomerName,
    OrderPriority? Priority,
    string AssignedTo,
    DateOnly? StartDate,
    DateOnly? DueDate,
    int CompletedChecklistItems,
    int TotalChecklistItems,
    int LinkedBuildRecords,
    DateTimeOffset LastUpdatedAt);
