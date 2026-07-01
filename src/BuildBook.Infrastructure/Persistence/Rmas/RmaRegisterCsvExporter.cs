using System.Text;
using BuildBook.Application.Rmas;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public sealed class RmaRegisterCsvExporter(
    IRmaRegisterReader rmaRegisterReader) : IRmaRegisterCsvExporter
{
    public async Task<string> ExportAsync(
        RmaRegisterFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await rmaRegisterReader.ListAsync(filter, cancellationToken);
        var builder = new StringBuilder();

        AppendRow(builder, [.. RmaRegisterExportProjection.Headers]);

        foreach (var row in rows)
        {
            AppendRow(builder, RmaRegisterExportProjection.Project(row));
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
