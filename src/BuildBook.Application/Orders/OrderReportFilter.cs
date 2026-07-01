namespace BuildBook.Application.Orders;

public sealed class OrderReportFilter
{
    public OrderReportScope Scope { get; init; } = OrderReportScope.All;

    public string? Value { get; init; }
}
