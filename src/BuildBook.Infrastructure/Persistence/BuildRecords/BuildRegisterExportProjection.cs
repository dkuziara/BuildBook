using BuildBook.Application.BuildRecords;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public static class BuildRegisterExportProjection
{
    private static readonly string[] ExportHeaders =
    [
        "Product code",
        "Product name",
        "Serial number",
        "Customer",
        "Machine name",
        "RadSight version",
        "Windows version",
        "Date assembled",
        "Date shipped",
        "Checked by",
        "Last updated"
    ];

    private static readonly string[] SensitiveTerms =
    [
        "Password",
        "BitLocker",
        "RecoveryKey"
    ];

    public static IReadOnlyList<string> Headers => ExportHeaders;

    public static string?[] Project(BuildRegisterRow row)
    {
        var values =
            new string?[]
            {
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
                row.LastUpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            };

        Validate(values);
        return values;
    }

    public static void Validate(string?[] values)
    {
        if (values.Length != ExportHeaders.Length)
        {
            throw new InvalidOperationException(
                "Build Register export rows must match the non-sensitive export column list.");
        }

        foreach (var header in ExportHeaders)
        {
            foreach (var sensitiveTerm in SensitiveTerms)
            {
                if (header.Contains(sensitiveTerm, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Build Register exports cannot include sensitive column names.");
                }
            }
        }
    }

    private static string? FormatDate(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd");
    }
}
