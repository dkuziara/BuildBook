using System.Text;
using BuildBook.Application.BuildRecords;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRegisterCsvExporter(
    IBuildRegisterReader buildRegisterReader) : IBuildRegisterCsvExporter
{
    public async Task<string> ExportAsync(
        BuildRegisterFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await buildRegisterReader.ListAsync(filter, cancellationToken);
        var builder = new StringBuilder();

        AppendRow(builder, BuildRegisterExportColumns.Headers);

        foreach (var row in rows)
        {
            AppendRow(
                builder,
                row.ProductCode,
                row.ProductName,
                row.SerialNumber,
                row.CustomerName,
                row.MachineName,
                row.RadSightVersion,
                row.WindowsVersion,
                FormatDate(row.DateAssembled),
                FormatDate(row.DateShipped),
                row.CheckedBy,
                row.LastUpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
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

    private static string? FormatDate(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd");
    }
}
