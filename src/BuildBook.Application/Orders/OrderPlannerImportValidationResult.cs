namespace BuildBook.Application.Orders;

public sealed class OrderPlannerImportValidationResult
{
    public int RowsRead { get; init; }

    public int ErrorCount { get; init; }

    public int WarningCount { get; init; }

    public IReadOnlyList<OrderPlannerImportValidationIssue> Issues { get; init; } = [];
}
