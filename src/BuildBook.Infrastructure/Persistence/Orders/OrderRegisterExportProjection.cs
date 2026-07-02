using BuildBook.Application.Orders;

namespace BuildBook.Infrastructure.Persistence.Orders;

internal static class OrderRegisterExportProjection
{
    public static readonly string[] Headers =
    [
        "Order Number",
        "Order Title",
        "Product Code",
        "Status",
        "Customer",
        "Priority",
        "Assigned To",
        "Start Date",
        "Due Date",
        "Checklist Progress",
        "Linked Build Records",
        "Last Updated"
    ];

    public static string[] Project(OrderRegisterRow row)
    {
        return
        [
            row.OrderNumber,
            row.OrderTitle,
            row.ProductCode ?? string.Empty,
            row.Status,
            row.CustomerName ?? string.Empty,
            row.Priority?.ToString() ?? string.Empty,
            row.AssignedTo,
            FormatDate(row.StartDate),
            FormatDate(row.DueDate),
            row.TotalChecklistItems == 0 ? "No checklist" : $"{row.CompletedChecklistItems} / {row.TotalChecklistItems}",
            row.LinkedBuildRecords.ToString(),
            row.LastUpdatedAt.ToLocalTime().ToString("d MMM yyyy HH:mm")
        ];
    }

    private static string FormatDate(DateOnly? value) => value?.ToString("d MMM yyyy") ?? string.Empty;
}
