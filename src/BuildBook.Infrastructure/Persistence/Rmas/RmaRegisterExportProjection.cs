using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;

namespace BuildBook.Infrastructure.Persistence.Rmas;

internal static class RmaRegisterExportProjection
{
    public static IReadOnlyList<string> Headers { get; } =
    [
        "RMA number",
        "Status",
        "Customer",
        "Product",
        "Serial",
        "Fault summary",
        "Priority",
        "Assigned to",
        "Due date",
        "Build Record",
        "Last updated"
    ];

    public static string?[] Project(RmaRegisterRow row)
    {
        return
        [
            row.RmaNumber,
            FormatStatus(row.Status),
            Display(row.CustomerName),
            row.ProductName,
            Display(row.SerialNumber),
            row.FaultSummary,
            DisplayPriority(row.Priority),
            Display(row.AssignedTo),
            Display(row.DueDate),
            row.HasLinkedBuildRecord ? "Linked" : "Unlinked",
            row.LastUpdatedAt.ToLocalTime().ToString("d MMM yyyy HH:mm")
        ];
    }

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;
    }

    private static string Display(DateOnly? value)
    {
        return value?.ToString("d MMM yyyy") ?? "Not recorded";
    }

    private static string DisplayPriority(RmaPriority? value)
    {
        return value?.ToString() ?? "Not set";
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
}
