using BuildBook.Application.Customers;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Customers;

public sealed class CustomerReportReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : ICustomerReportReader
{
    public async Task<IReadOnlyList<CustomerContractReportRow>> ListCustomersAsync(
        CustomerReportFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rows = await dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.IsActive)
            .OrderBy(customer => customer.Name)
            .Select(customer => new RawCustomerRow(
                customer.Id,
                customer.Name,
                customer.AccountCode,
                customer.PrimaryContactName,
                customer.MainEmail,
                customer.MainPhone,
                customer.SupportContractLevel != null ? customer.SupportContractLevel.Name : null,
                customer.SupportContractStatus,
                customer.SupportContractEndDate,
                customer.IsActive,
                customer.BuildRecords.Count(buildRecord => buildRecord.IsActive),
                customer.RmaRecords.Count(rmaRecord => rmaRecord.IsActive),
                customer.RmaRecords.Count(rmaRecord =>
                    rmaRecord.IsActive
                    && rmaRecord.Status != RmaStatus.Closed
                    && rmaRecord.Status != RmaStatus.Shipped
                    && rmaRecord.Status != RmaStatus.CancelledNoReply
                    && rmaRecord.Status != RmaStatus.CustomerFixed),
                customer.RmaRecords.Count(rmaRecord =>
                    rmaRecord.IsActive
                    && rmaRecord.Status != RmaStatus.Closed
                    && rmaRecord.Status != RmaStatus.Shipped
                    && rmaRecord.Status != RmaStatus.CancelledNoReply
                    && rmaRecord.Status != RmaStatus.CustomerFixed
                    && rmaRecord.DueDate != null
                    && rmaRecord.DueDate < today),
                customer.LastUpdatedAt))
            .ToListAsync(cancellationToken);

        return ApplyCustomerFilter(
            rows.Select(row => new CustomerContractReportRow(
                row.CustomerId,
                row.CustomerName,
                row.AccountCode,
                row.PrimaryContactName,
                row.MainEmail,
                row.MainPhone,
                row.SupportContractLevelName,
                row.SupportContractStatus,
                row.SupportContractEndDate,
                row.IsActive,
                row.BuildRecordCount,
                row.LinkedRmaCount,
                row.OpenRmaCount,
                row.OverdueRmaCount,
                row.LastUpdatedAt)).ToList(),
            filter,
            today);
    }

    public async Task<IReadOnlyList<CustomerSupportRmaReportRow>> ListRmasAsync(
        CustomerReportFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rows = await dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord => rmaRecord.IsActive && rmaRecord.CustomerId != null)
            .OrderByDescending(rmaRecord => rmaRecord.LastUpdatedAt)
            .ThenBy(rmaRecord => rmaRecord.RmaNumber)
            .Select(rmaRecord => new RawCustomerRmaRow(
                rmaRecord.Id,
                rmaRecord.RmaNumber,
                rmaRecord.Status,
                rmaRecord.Customer != null ? rmaRecord.Customer.Name : "Not recorded",
                rmaRecord.Customer != null && rmaRecord.Customer.SupportContractLevel != null
                    ? rmaRecord.Customer.SupportContractLevel.Name
                    : null,
                rmaRecord.Customer != null
                    ? rmaRecord.Customer.SupportContractStatus
                    : CustomerSupportContractStatuses.Unknown,
                rmaRecord.Priority,
                rmaRecord.Customer != null && rmaRecord.Customer.SupportContractStatus == CustomerSupportContractStatuses.Active
                    ? rmaRecord.Customer.SupportContractLevel != null
                        ? rmaRecord.Customer.SupportContractLevel.DefaultRmaPriority
                        : null
                    : null,
                rmaRecord.ProductName,
                rmaRecord.SerialNumber,
                rmaRecord.FaultSummary,
                rmaRecord.SupportTicketNumber,
                rmaRecord.DueDate,
                rmaRecord.LastUpdatedAt))
            .ToListAsync(cancellationToken);

        return ApplyRmaFilter(
            rows.Select(row =>
            {
                var isOpen = IsOperationallyOpen(row.Status);
                return new CustomerSupportRmaReportRow(
                    row.RmaId,
                    row.RmaNumber,
                    row.Status,
                    row.CustomerName,
                    row.SupportContractLevelName,
                    row.SupportContractStatus,
                    row.Priority,
                    row.SuggestedPriority,
                    row.ProductName,
                    row.SerialNumber,
                    row.FaultSummary,
                    row.SupportTicketNumber,
                    row.DueDate,
                    row.LastUpdatedAt,
                    isOpen,
                    isOpen && row.DueDate != null && row.DueDate < today,
                    HasPriorityMismatch(
                        row.SupportContractStatus,
                        row.SuggestedPriority,
                        row.Priority,
                        row.Status));
            }).ToList(),
            filter);
    }

    private static IReadOnlyList<CustomerContractReportRow> ApplyCustomerFilter(
        IReadOnlyList<CustomerContractReportRow> rows,
        CustomerReportFilter? filter,
        DateOnly today)
    {
        if (filter is null)
        {
            return rows;
        }

        var value = Normalize(filter.Value);

        return filter.Scope switch
        {
            CustomerReportScope.CustomersByContractLevel when value is not null => rows.Where(row =>
                string.Equals(Normalize(row.SupportContractLevelName), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            CustomerReportScope.CustomersWithNoContract => rows.Where(row =>
                string.Equals(row.SupportContractStatus, CustomerSupportContractStatuses.NoContract, StringComparison.OrdinalIgnoreCase)).ToList(),
            CustomerReportScope.ExpiredContracts => rows.Where(row => IsExpired(row, today)).ToList(),
            CustomerReportScope.ContractsExpiringWithinDays when int.TryParse(value, out var days) => rows.Where(row =>
                row.SupportContractEndDate != null
                && row.SupportContractEndDate >= today
                && row.SupportContractEndDate <= today.AddDays(days)
                && !string.Equals(row.SupportContractStatus, CustomerSupportContractStatuses.NoContract, StringComparison.OrdinalIgnoreCase)).ToList(),
            _ => rows
        };
    }

    private static IReadOnlyList<CustomerSupportRmaReportRow> ApplyRmaFilter(
        IReadOnlyList<CustomerSupportRmaReportRow> rows,
        CustomerReportFilter? filter)
    {
        if (filter is null)
        {
            return rows;
        }

        var value = Normalize(filter.Value);

        return filter.Scope switch
        {
            CustomerReportScope.OpenRmasByContractLevel when value is not null => rows.Where(row =>
                row.IsOpen
                && string.Equals(Normalize(row.SupportContractLevelName), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            CustomerReportScope.OverdueRmasByContractLevel when value is not null => rows.Where(row =>
                row.IsOverdue
                && string.Equals(Normalize(row.SupportContractLevelName), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            CustomerReportScope.PriorityMismatch => rows.Where(row => row.HasPriorityMismatch).ToList(),
            CustomerReportScope.MissingSupportTicketNumber => rows.Where(row =>
                string.IsNullOrWhiteSpace(row.SupportTicketNumber)).ToList(),
            _ => rows
        };
    }

    private static bool IsOperationallyOpen(RmaStatus status)
    {
        return status != RmaStatus.Closed
            && status != RmaStatus.Shipped
            && status != RmaStatus.CancelledNoReply
            && status != RmaStatus.CustomerFixed;
    }

    private static bool IsExpired(CustomerContractReportRow row, DateOnly today)
    {
        return IsExpired(row.SupportContractStatus, row.SupportContractEndDate, today);
    }

    private static bool IsExpired(string? contractStatus, DateOnly? contractEndDate, DateOnly today)
    {
        return string.Equals(contractStatus, CustomerSupportContractStatuses.Expired, StringComparison.OrdinalIgnoreCase)
            || (contractEndDate != null
                && contractEndDate < today
                && !string.Equals(contractStatus, CustomerSupportContractStatuses.NoContract, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasPriorityMismatch(
        string? supportContractStatus,
        RmaPriority? suggestedPriority,
        RmaPriority? selectedPriority,
        RmaStatus status)
    {
        if (!IsOperationallyOpen(status))
        {
            return false;
        }

        if (!string.Equals(supportContractStatus, CustomerSupportContractStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return suggestedPriority is not null
            && (selectedPriority is null || selectedPriority.Value < suggestedPriority.Value);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record RawCustomerRow(
        int CustomerId,
        string CustomerName,
        string? AccountCode,
        string? PrimaryContactName,
        string? MainEmail,
        string? MainPhone,
        string? SupportContractLevelName,
        string SupportContractStatus,
        DateOnly? SupportContractEndDate,
        bool IsActive,
        int BuildRecordCount,
        int LinkedRmaCount,
        int OpenRmaCount,
        int OverdueRmaCount,
        DateTimeOffset LastUpdatedAt);

    private sealed record RawCustomerRmaRow(
        int RmaId,
        string RmaNumber,
        RmaStatus Status,
        string CustomerName,
        string? SupportContractLevelName,
        string SupportContractStatus,
        RmaPriority? Priority,
        RmaPriority? SuggestedPriority,
        string ProductName,
        string? SerialNumber,
        string FaultSummary,
        string? SupportTicketNumber,
        DateOnly? DueDate,
        DateTimeOffset LastUpdatedAt);
}
