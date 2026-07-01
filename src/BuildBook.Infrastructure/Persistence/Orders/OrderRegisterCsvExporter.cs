using System.Text;
using BuildBook.Application.Orders;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderRegisterCsvExporter(
    IOrderRegisterReader orderRegisterReader) : IOrderRegisterCsvExporter
{
    public async Task<string> ExportAsync(
        OrderRegisterFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await orderRegisterReader.ListAsync(filter, cancellationToken);
        var builder = new StringBuilder();

        AppendRow(builder, [.. OrderRegisterExportProjection.Headers]);

        foreach (var row in rows)
        {
            AppendRow(builder, OrderRegisterExportProjection.Project(row));
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
