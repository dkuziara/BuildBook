namespace BuildBook.Application.Rmas;

public sealed class RmaReportFilter
{
    public RmaReportScope Scope { get; init; } = RmaReportScope.All;

    public string? Value { get; init; }
}
