using BuildBook.Application.Orders;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderReportReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IOrderReportReader
{
    public async Task<IReadOnlyList<OrderReportRow>> ListAsync(
        OrderReportFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var rows = await dbContext.OrderRecords
            .AsNoTracking()
            .Where(orderRecord => orderRecord.IsActive)
            .OrderByDescending(orderRecord => orderRecord.LastUpdatedAt)
            .ThenBy(orderRecord => orderRecord.OrderNumber)
            .Select(orderRecord => new RawOrderReportRow(
                orderRecord.Id,
                orderRecord.OrderNumber,
                orderRecord.OrderTitle,
                orderRecord.Status,
                orderRecord.Customer == null ? null : orderRecord.Customer.Name,
                orderRecord.Priority,
                orderRecord.Assignments
                    .Select(assignment => assignment.ApplicationUser != null
                        ? assignment.ApplicationUser.DisplayName
                            ?? assignment.ApplicationUser.EmailAddress
                            ?? assignment.ApplicationUser.WindowsUserName
                        : assignment.ImportedUserText ?? string.Empty)
                    .ToArray(),
                orderRecord.DueDate,
                orderRecord.ChecklistItems.Count(checklistItem => checklistItem.IsCompleted),
                orderRecord.ChecklistItems.Count(),
                orderRecord.BuildRecordLinks.Count(),
                orderRecord.ContractReadyForInvoicing,
                orderRecord.ReadyForInvoicingDate,
                orderRecord.InvoiceNumber,
                orderRecord.InvoicedDate,
                orderRecord.ShippedDate,
                orderRecord.CreatedAt,
                orderRecord.LastUpdatedAt,
                orderRecord.DueDate != null && orderRecord.DueDate < today && orderRecord.CompletedAt == null))
            .ToListAsync(cancellationToken);

        var projectedRows = rows
            .Select(row => new OrderReportRow(
                row.Id,
                row.OrderNumber,
                row.OrderTitle,
                row.Status,
                row.CustomerName,
                row.Priority,
                SummarizeAssignments(row.AssignmentNames),
                row.DueDate,
                row.CompletedChecklistItems,
                row.TotalChecklistItems,
                row.LinkedBuildRecords,
                row.ContractReadyForInvoicing,
                row.ReadyForInvoicingDate,
                row.InvoiceNumber,
                row.InvoicedDate,
                row.ShippedDate,
                row.CreatedAt,
                row.LastUpdatedAt,
                row.IsOverdue))
            .ToList();

        return ApplyFilter(projectedRows, filter, today, monthStart, monthEnd);
    }

    private static IReadOnlyList<OrderReportRow> ApplyFilter(
        IReadOnlyList<OrderReportRow> rows,
        OrderReportFilter? filter,
        DateOnly today,
        DateOnly monthStart,
        DateOnly monthEnd)
    {
        if (filter is null)
        {
            return rows;
        }

        var value = Normalize(filter.Value);

        return filter.Scope switch
        {
            OrderReportScope.OperationalOpen => rows.Where(row => !string.Equals(row.Status, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal)).ToList(),
            OrderReportScope.OperationalOverdue => rows.Where(row => row.IsOverdue).ToList(),
            OrderReportScope.OperationalDueThisWeek => rows.Where(row =>
                row.DueDate is not null
                && row.DueDate >= today
                && row.DueDate <= today.AddDays(7)
                && !string.Equals(row.Status, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal)).ToList(),
            OrderReportScope.Status when value is not null => rows.Where(row =>
                string.Equals(row.Status, value, StringComparison.OrdinalIgnoreCase)).ToList(),
            OrderReportScope.AssignedUser when value is not null => rows.Where(row =>
                ContainsValue(row.AssignedTo, value)).ToList(),
            OrderReportScope.WaitingForPartsStock => rows.Where(row =>
                string.Equals(row.Status, BuildBookOrderStatuses.PartsOrderedOrStockAllocated, StringComparison.Ordinal)).ToList(),
            OrderReportScope.BuiltNotPreparedForShipping => rows.Where(row =>
                string.Equals(row.Status, BuildBookOrderStatuses.Built, StringComparison.Ordinal)).ToList(),
            OrderReportScope.ReadyForCollectionNotShipped => rows.Where(row =>
                string.Equals(row.Status, BuildBookOrderStatuses.ReadyForCollection, StringComparison.Ordinal)
                || (string.Equals(row.Status, BuildBookOrderStatuses.Shipped, StringComparison.Ordinal) && row.ShippedDate is null)).ToList(),
            OrderReportScope.ShippedNotReadyForInvoicing => rows.Where(row =>
                row.ShippedDate is not null
                && (row.ContractReadyForInvoicing != true || row.ReadyForInvoicingDate is null)
                && row.InvoicedDate is null).ToList(),
            OrderReportScope.ReadyForInvoicingNotInvoiced => rows.Where(row =>
                row.ContractReadyForInvoicing == true
                && row.InvoicedDate is null).ToList(),
            OrderReportScope.Customer when value is not null => rows.Where(row =>
                string.Equals(Normalize(row.CustomerName), value, StringComparison.OrdinalIgnoreCase)).ToList(),
            OrderReportScope.OrdersWithoutLinkedCustomer => rows.Where(row =>
                string.IsNullOrWhiteSpace(row.CustomerName)).ToList(),
            OrderReportScope.NoLinkedBuildRecord => rows.Where(row => row.LinkedBuildRecords == 0).ToList(),
            OrderReportScope.MultipleLinkedBuildRecords => rows.Where(row => row.LinkedBuildRecords > 1).ToList(),
            OrderReportScope.IncompleteChecklist => rows.Where(row =>
                row.TotalChecklistItems > 0 && row.CompletedChecklistItems < row.TotalChecklistItems).ToList(),
            OrderReportScope.ReadyBasedOnChecklist => rows.Where(row =>
                row.TotalChecklistItems > 0
                && row.CompletedChecklistItems == row.TotalChecklistItems
                && !string.Equals(row.Status, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal)).ToList(),
            OrderReportScope.OrdersReadyForInvoicing => rows.Where(row =>
                row.ContractReadyForInvoicing == true
                && row.ReadyForInvoicingDate is not null
                && row.InvoicedDate is null).ToList(),
            OrderReportScope.OrdersInvoicedThisMonth => rows.Where(row =>
                row.InvoicedDate is not null
                && row.InvoicedDate >= monthStart
                && row.InvoicedDate <= monthEnd).ToList(),
            OrderReportScope.OrdersShippedNotInvoiced => rows.Where(row =>
                row.ShippedDate is not null
                && row.InvoicedDate is null).ToList(),
            OrderReportScope.MissingInvoiceNumber => rows.Where(row =>
                string.Equals(row.Status, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal)
                && string.IsNullOrWhiteSpace(row.InvoiceNumber)).ToList(),
            _ => rows
        };
    }

    private static bool ContainsValue(string? source, string value)
    {
        return Normalize(source)?.Contains(value, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string SummarizeAssignments(IEnumerable<string> assignmentNames)
    {
        var values = assignmentNames
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return values.Length == 0
            ? "Unassigned"
            : string.Join(", ", values);
    }

    private sealed record RawOrderReportRow(
        int Id,
        string OrderNumber,
        string OrderTitle,
        string Status,
        string? CustomerName,
        BuildBook.Domain.Orders.OrderPriority? Priority,
        string[] AssignmentNames,
        DateOnly? DueDate,
        int CompletedChecklistItems,
        int TotalChecklistItems,
        int LinkedBuildRecords,
        bool? ContractReadyForInvoicing,
        DateOnly? ReadyForInvoicingDate,
        string? InvoiceNumber,
        DateOnly? InvoicedDate,
        DateOnly? ShippedDate,
        DateTimeOffset CreatedAt,
        DateTimeOffset LastUpdatedAt,
        bool IsOverdue);
}
