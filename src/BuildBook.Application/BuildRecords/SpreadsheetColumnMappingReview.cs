namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetColumnMappingReview
{
    public IReadOnlyList<SpreadsheetImportColumnMapping> ColumnMappings { get; init; } = [];

    public IReadOnlyList<SpreadsheetImportFieldOption> AvailableFields { get; init; } = [];

    public IReadOnlyList<string> Notices { get; init; } = [];
}
