using System.Text;
using BuildBook.Application.Orders;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderReportCsvExporter(
    IOrderReportReader orderReportReader) : IOrderReportCsvExporter
{
    public async Task<string> ExportAsync(
        OrderReportFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await orderReportReader.ListAsync(filter, cancellationToken);
        var builder = new StringBuilder();

        AppendRow(builder, [.. OrderReportExportProjection.Headers]);

        foreach (var row in rows)
        {
            AppendRow(builder, OrderReportExportProjection.Project(row));
        }

        return builder.ToString();
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
