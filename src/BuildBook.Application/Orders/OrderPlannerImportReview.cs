namespace BuildBook.Application.Orders;

public sealed class OrderPlannerImportReview
{
    public string? PreferredWorksheetName { get; init; }

    public string? PlanId { get; init; }

    public string? PlanName { get; init; }

    public DateTimeOffset? ExportDate { get; init; }

    public int RowsRead { get; init; }

    public int RowsShown { get; init; }

    public IReadOnlyList<string> WorksheetNames { get; init; } = [];

    public IReadOnlyList<string> Notices { get; init; } = [];

    public IReadOnlyList<OrderPlannerImportPreviewColumn> Columns { get; init; } = [];

    public IReadOnlyList<OrderPlannerImportPreviewRow> Rows { get; init; } = [];
}
