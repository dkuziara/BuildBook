using System.Text;
using BuildBook.Application.Customers;

namespace BuildBook.Infrastructure.Persistence.Customers;

public sealed class CustomerReportCsvExporter(
    ICustomerReportReader customerReportReader) : ICustomerReportCsvExporter
{
    public async Task<string> ExportAsync(
        CustomerReportFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new StringBuilder();

        if (UsesCustomerRows(filter))
        {
            var rows = await customerReportReader.ListCustomersAsync(filter, cancellationToken);
            AppendRow(builder, CustomerReportExportProjection.CustomerHeaders);

            foreach (var row in rows)
            {
                AppendRow(builder, CustomerReportExportProjection.Project(row));
            }

            return builder.ToString();
        }

        var rmaRows = await customerReportReader.ListRmasAsync(filter, cancellationToken);
        AppendRow(builder, CustomerReportExportProjection.RmaHeaders);

        foreach (var row in rmaRows)
        {
            AppendRow(builder, CustomerReportExportProjection.Project(row));
        }

        return builder.ToString();
    }

    private static bool UsesCustomerRows(CustomerReportFilter? filter)
    {
        return filter is null
            || filter.Scope == CustomerReportScope.AllCustomers
            || filter.Scope == CustomerReportScope.CustomersByContractLevel
            || filter.Scope == CustomerReportScope.CustomersWithNoContract
            || filter.Scope == CustomerReportScope.ExpiredContracts
            || filter.Scope == CustomerReportScope.ContractsExpiringWithinDays;
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
