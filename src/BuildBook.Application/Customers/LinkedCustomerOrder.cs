using BuildBook.Domain.Orders;

namespace BuildBook.Application.Customers;

public sealed record LinkedCustomerOrder(
    int Id,
    string OrderNumber,
    string OrderTitle,
    string Status,
    OrderPriority? Priority,
    DateOnly? DueDate,
    string? SupportTicketNumber,
    DateTimeOffset LastUpdatedAt);
