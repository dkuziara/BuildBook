using BuildBook.Application.Rmas;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public static class RmaReportExportProjection
{
    private static readonly string[] ExportHeaders =
    [
        "RMA number",
        "Status",
        "Customer",
        "Product code",
        "Product name",
        "Serial number",
        "Fault summary",
        "Fault category",
        "Root cause category",
        "Warranty status",
        "Chargeable repair",
        "Approval required",
        "Approval received",
        "Estimated repair cost",
        "Actual repair cost",
        "Assigned to",
        "Due date",
        "On hold reason",
        "Date received",
        "Repair completed date",
        "Shipped date",
        "Created at",
        "Last updated",
        "Previous RMA count",
        "Days open",
        "Days in current status",
        "Days on hold",
        "Repair days",
        "Ready-to-ship to shipped days"
    ];

    private static readonly string[] SensitiveTerms =
    [
        "Password",
        "BitLocker",
        "RecoveryKey",
        "Secret"
    ];

    public static IReadOnlyList<string> Headers => ExportHeaders;

    public static string?[] Project(RmaReportRow row)
    {
        var values =
            new string?[]
            {
                row.RmaNumber,
                row.Status.ToString(),
                row.CustomerName,
                row.ProductCode,
                row.ProductName,
                row.SerialNumber,
                row.FaultSummary,
                row.FaultCategory?.ToString(),
                row.RootCauseCategory?.ToString(),
                row.WarrantyStatus?.ToString(),
                FormatBool(row.ChargeableRepair),
                FormatBool(row.CustomerApprovalRequired),
                FormatBool(row.CustomerApprovalReceived),
                FormatCurrency(row.EstimatedRepairCost),
                FormatCurrency(row.ActualRepairCost),
                row.AssignedTo,
                FormatDate(row.DueDate),
                row.OnHoldReason,
                FormatDate(row.DateItemReceived),
                FormatDate(row.RepairCompletedDate),
                FormatDate(row.ShippedDate),
                row.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                row.LastUpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                row.PreviousRmaCount.ToString(),
                row.DaysOpen.ToString(),
                row.DaysInCurrentStatus.ToString(),
                row.DaysOnHold.ToString(),
                row.RepairDays?.ToString(),
                row.ReadyToShipToShippedDays?.ToString()
            };

        Validate(values);
        return values;
    }

    public static void Validate(string?[] values)
    {
        if (values.Length != ExportHeaders.Length)
        {
            throw new InvalidOperationException("RMA report export rows must match the non-sensitive export column list.");
        }

        foreach (var header in ExportHeaders)
        {
            foreach (var sensitiveTerm in SensitiveTerms)
            {
                if (header.Contains(sensitiveTerm, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("RMA report exports cannot include sensitive column names.");
                }
            }
        }
    }

    private static string? FormatDate(DateOnly? value) => value?.ToString("yyyy-MM-dd");
    private static string? FormatBool(bool? value) => value?.ToString();
    private static string? FormatCurrency(decimal? value) => value?.ToString("0.00");
}
