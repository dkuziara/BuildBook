using System.Text;
using BuildBook.Application.Customers;

namespace BuildBook.Infrastructure.Persistence.Customers;

public sealed class CustomerListCsvExporter(
    ICustomerListReader customerListReader) : ICustomerListCsvExporter
{
    public async Task<string> ExportAsync(
        CustomerListFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = await customerListReader.ListAsync(filter, cancellationToken);
        var builder = new StringBuilder();

        AppendRow(builder, [.. CustomerListExportProjection.Headers]);

        foreach (var row in rows)
        {
            AppendRow(builder, CustomerListExportProjection.Project(row));
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
