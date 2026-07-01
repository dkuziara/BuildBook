using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed class OrderPlannerImportValidationIssue
{
    public int? SourceRowNumber { get; init; }

    public string? PlannerTaskId { get; init; }

    public OrderImportWarningSeverity Severity { get; init; }

    public string Message { get; init; } = string.Empty;
}
