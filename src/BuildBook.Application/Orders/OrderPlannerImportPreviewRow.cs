namespace BuildBook.Application.Orders;

public sealed class OrderPlannerImportPreviewRow
{
    public int SourceRowNumber { get; init; }

    public IReadOnlyDictionary<string, string> Values { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
