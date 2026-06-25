namespace BuildBook.Application.BuildRecords;

public sealed class SpreadsheetImportPreview
{
    public IReadOnlyList<SpreadsheetImportPreviewColumn> Columns { get; init; } = [];

    public IReadOnlyList<SpreadsheetImportPreviewRow> Rows { get; init; } = [];

    public IReadOnlyList<string> Notices { get; init; } = [];

    public int RowsRead { get; init; }

    public int RowsShown { get; init; }
}
