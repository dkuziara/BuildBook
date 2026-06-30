using BuildBook.Application.Customers;
using BuildBook.Domain.Rmas;

namespace BuildBook.Infrastructure.Persistence.Customers;

internal static class CustomerReportExportProjection
{
    public static readonly string[] CustomerHeaders =
    [
        "Customer",
        "Account code",
        "Primary contact",
        "Email",
        "Phone",
        "Support contract level",
        "Support contract status",
        "Contract end date",
        "Build Records",
        "Linked RMAs",
        "Open RMAs",
        "Overdue RMAs",
        "Last updated"
    ];

    public static readonly string[] RmaHeaders =
    [
        "RMA number",
        "Status",
        "Customer",
        "Support contract level",
        "Support contract status",
        "Selected priority",
        "Suggested priority",
        "Product",
        "Serial number",
        "Fault summary",
        "Support Ticket No.",
        "Due date",
        "Last updated"
    ];

    public static string?[] Project(CustomerContractReportRow row)
    {
        return
        [
            row.CustomerName,
            row.AccountCode,
            row.PrimaryContactName,
            row.MainEmail,
            row.MainPhone,
            row.SupportContractLevelName,
            row.SupportContractStatus,
            FormatDate(row.SupportContractEndDate),
            row.BuildRecordCount.ToString(),
            row.LinkedRmaCount.ToString(),
            row.OpenRmaCount.ToString(),
            row.OverdueRmaCount.ToString(),
            FormatDateTime(row.LastUpdatedAt)
        ];
    }

    public static string?[] Project(CustomerSupportRmaReportRow row)
    {
        return
        [
            row.RmaNumber,
            FormatStatus(row.Status),
            row.CustomerName,
            row.SupportContractLevelName,
            row.SupportContractStatus,
            row.Priority?.ToString(),
            row.SuggestedPriority?.ToString(),
            row.ProductName,
            row.SerialNumber,
            row.FaultSummary,
            row.SupportTicketNumber,
            FormatDate(row.DueDate),
            FormatDateTime(row.LastUpdatedAt)
        ];
    }

    private static string FormatStatus(RmaStatus status)
    {
        return status switch
        {
            RmaStatus.BookedIn => "Booked In",
            RmaStatus.WorkInProgress => "Work In Progress",
            RmaStatus.ReadyToShip => "Ready To Ship",
            RmaStatus.CancelledNoReply => "Cancelled / No Reply",
            RmaStatus.CustomerFixed => "Customer Fixed",
            _ => status.ToString()
        };
    }

    private static string? FormatDate(DateOnly? value) => value?.ToString("yyyy-MM-dd");

    private static string FormatDateTime(DateTimeOffset value) => value.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
}
