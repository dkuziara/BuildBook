using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Customers;

public sealed record CustomerSupportRmaReportRow(
    int RmaId,
    string RmaNumber,
    RmaStatus Status,
    string CustomerName,
    string? SupportContractLevelName,
    string SupportContractStatus,
    RmaPriority? Priority,
    RmaPriority? SuggestedPriority,
    string ProductName,
    string? SerialNumber,
    string FaultSummary,
    string? SupportTicketNumber,
    DateOnly? DueDate,
    DateTimeOffset LastUpdatedAt,
    bool IsOpen,
    bool IsOverdue,
    bool HasPriorityMismatch);
