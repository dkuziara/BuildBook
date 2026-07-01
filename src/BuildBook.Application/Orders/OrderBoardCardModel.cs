using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed record OrderBoardCardModel(
    int Id,
    string OrderNumber,
    string OrderTitle,
    string Status,
    string? CustomerName,
    OrderPriority? Priority,
    string AssignedTo,
    DateOnly? DueDate,
    int CompletedChecklistItems,
    int TotalChecklistItems,
    bool HasLinkedBuildRecord,
    bool IsOverdue,
    IReadOnlyList<string> Warnings);
