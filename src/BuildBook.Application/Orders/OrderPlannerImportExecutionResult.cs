namespace BuildBook.Application.Orders;

public sealed class OrderPlannerImportExecutionResult
{
    public int? ImportBatchId { get; init; }

    public int RowsRead { get; init; }

    public int OrdersCreated { get; init; }

    public int OrdersUpdated { get; init; }

    public int OrdersSkipped { get; init; }

    public int WarningCount { get; init; }

    public int ErrorCount { get; init; }

    public string Summary { get; init; } = string.Empty;
}
