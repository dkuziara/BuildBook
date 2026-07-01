namespace BuildBook.Domain.Orders;

public sealed class OrderImportWarning
{
    public int Id { get; set; }

    public int OrderImportBatchId { get; set; }

    public OrderImportBatch? OrderImportBatch { get; set; }

    public int? RowNumber { get; set; }

    public string? PlannerTaskId { get; set; }

    public string WarningType { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public OrderImportWarningSeverity Severity { get; set; } = OrderImportWarningSeverity.Warning;
}
