namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetImportFieldOption
{
    public string Key { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public bool IsRequired { get; init; }

    public bool IsSensitive { get; init; }
}
