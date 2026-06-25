namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetImportValidationIssue
{
    public int SourceRowNumber { get; init; }

    public SpreadsheetImportValidationSeverity Severity { get; init; }

    public string Message { get; init; } = string.Empty;
}
