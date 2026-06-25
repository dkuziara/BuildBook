namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetImportColumnMapping
{
    public string SourceColumnName { get; init; } = string.Empty;

    public string? SuggestedFieldKey { get; init; }
}
