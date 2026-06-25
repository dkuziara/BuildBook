namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetImportExecutionResult
{
    public int? ImportBatchId { get; init; }

    public int RowsRead { get; init; }

    public int RecordsCreated { get; init; }

    public int RecordsSkipped { get; init; }

    public int WarningCount { get; init; }

    public int ErrorCount { get; init; }

    public string Summary { get; init; } = string.Empty;
}
