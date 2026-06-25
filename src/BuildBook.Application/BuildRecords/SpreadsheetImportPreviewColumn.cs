namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetImportPreviewColumn
{
    public string FieldKey { get; init; } = string.Empty;

    public string FieldLabel { get; init; } = string.Empty;

    public string SourceColumnName { get; init; } = string.Empty;

    public bool IsSensitive { get; init; }
}
