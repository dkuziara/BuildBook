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

        AppendRow(builder, BuildRegisterExportProjection.Headers.ToArray());

        foreach (var row in rows)
        {
            AppendRow(builder, BuildRegisterExportProjection.Project(row));
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
