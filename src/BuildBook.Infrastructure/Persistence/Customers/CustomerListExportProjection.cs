using BuildBook.Application.Customers;

namespace BuildBook.Infrastructure.Persistence.Customers;

internal static class CustomerListExportProjection
{
    public static IReadOnlyList<string> Headers { get; } =
    [
        "Customer name",
        "Primary contact",
        "Email",
        "Phone",
        "Support contract level",
        "Support contract status",
        "Contract end date",
        "Active",
        "Last updated"
    ];

    public static string?[] Project(CustomerListItem row)
    {
        return
        [
            row.Name,
            Display(row.PrimaryContactName),
            Display(row.MainEmail),
            Display(row.MainPhone),
            Display(row.SupportContractLevelName),
            row.SupportContractStatus,
            Display(row.SupportContractEndDate),
            row.IsActive ? "Active" : "Inactive",
            row.LastUpdatedAt.ToLocalTime().ToString("d MMM yyyy HH:mm")
        ];
    }

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;
    }

    private static string Display(DateOnly? value)
    {
        return value?.ToString("d MMM yyyy") ?? "Not recorded";
    }
}
