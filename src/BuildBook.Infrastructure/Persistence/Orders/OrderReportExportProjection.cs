using BuildBook.Application.Orders;

namespace BuildBook.Infrastructure.Persistence.Orders;

internal static class OrderReportExportProjection
{
    public static readonly string[] Headers =
    [
        "Order Number",
        "Order Title",
        "Status",
        "Customer",
        "Priority",
        "Assigned To",
        "Due Date",
        "Checklist Progress",
        "Linked Build Records",
        "Shipped Date",
        "Ready For Invoicing Date",
        "Invoice Number",
        "Invoiced Date",
        "Last Updated"
    ];

    public static string[] Project(OrderReportRow row)
    {
        return
        [
            row.OrderNumber,
            row.OrderTitle,
            row.Status,
            row.CustomerName ?? string.Empty,
            row.Priority?.ToString() ?? string.Empty,
            row.AssignedTo,
            FormatDate(row.DueDate),
            row.TotalChecklistItems == 0 ? "No checklist" : $"{row.CompletedChecklistItems} / {row.TotalChecklistItems}",
            row.LinkedBuildRecords.ToString(),
            FormatDate(row.ShippedDate),
            FormatDate(row.ReadyForInvoicingDate),
            row.InvoiceNumber ?? string.Empty,
            FormatDate(row.InvoicedDate),
            row.LastUpdatedAt.ToLocalTime().ToString("d MMM yyyy HH:mm")
        ];
    }

    private static string FormatDate(DateOnly? value) => value?.ToString("d MMM yyyy") ?? string.Empty;
}
