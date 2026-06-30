using System.Text;
using BuildBook.Application.BuildRecords;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class MissingDataReportCsvExporter(
    IMissingDataReportReader missingDataReportReader) : IMissingDataReportCsvExporter
{
    public async Task<string> ExportAsync(
        MissingDataReportType reportType,
        CancellationToken cancellationToken = default)
    {
        var rows = FilterRows(
            await missingDataReportReader.ListActiveAsync(cancellationToken),
            reportType);
        var builder = new StringBuilder();

        AppendRow(builder, [.. MissingDataReportExportProjection.Headers]);

        foreach (var row in rows)
        {
            AppendRow(builder, MissingDataReportExportProjection.Project(row));
        }

        return builder.ToString();
    }

    private static IReadOnlyList<MissingDataReportRow> FilterRows(
        IReadOnlyList<MissingDataReportRow> rows,
        MissingDataReportType reportType)
    {
        return reportType switch
        {
            MissingDataReportType.Customer => rows.Where(row => row.IsMissingCustomer).ToList(),
            MissingDataReportType.RecoveryData => rows.Where(row => row.IsMissingRecoveryData).ToList(),
            MissingDataReportType.DateShipped => rows.Where(row => row.IsMissingDateShipped).ToList(),
            _ => []
        };
    }

    private static void AppendRow(StringBuilder builder, params string?[] values)
    {
        builder.AppendJoin(",", values.Select(Escape));
        builder.AppendLine();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var normalized = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return normalized.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{normalized}\""
            : normalized;
    }
}
