namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetImportValidationResult
{
    public IReadOnlyList<SpreadsheetImportValidationIssue> Issues { get; init; } = [];

    public int RowsRead { get; init; }

    public int ErrorCount { get; init; }

    public int WarningCount { get; init; }
}
