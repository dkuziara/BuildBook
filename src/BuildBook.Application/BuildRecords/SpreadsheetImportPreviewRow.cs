namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetImportPreviewRow
{
    public int SourceRowNumber { get; init; }

    public IReadOnlyDictionary<string, string> Values { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
