using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed record OrderReportRow(
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
    int LinkedBuildRecords,
    bool? ContractReadyForInvoicing,
    DateOnly? ReadyForInvoicingDate,
    string? InvoiceNumber,
    DateOnly? InvoicedDate,
    DateOnly? ShippedDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUpdatedAt,
    bool IsOverdue);
