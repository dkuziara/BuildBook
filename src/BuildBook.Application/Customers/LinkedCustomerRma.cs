using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Customers;

public sealed record LinkedCustomerRma(
    int Id,
    string RmaNumber,
    RmaStatus Status,
    string ProductName,
    string? SerialNumber,
    string FaultSummary,
    string? SupportTicketNumber,
    DateOnly? DueDate,
    DateTimeOffset LastUpdatedAt,
    bool IsOpen,
    bool IsOverdue);
