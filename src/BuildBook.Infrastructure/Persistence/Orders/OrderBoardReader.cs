using BuildBook.Application.Orders;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderBoardReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IOrderBoardReader
{
    public async Task<IReadOnlyList<OrderBoardCardModel>> GetBoardAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rows = await dbContext.OrderRecords
            .AsNoTracking()
            .Where(orderRecord => orderRecord.IsActive)
            .OrderBy(orderRecord => orderRecord.DueDate)
            .ThenBy(orderRecord => orderRecord.OrderNumber)
            .Select(orderRecord => new
            {
                orderRecord.Id,
                orderRecord.OrderNumber,
                orderRecord.OrderTitle,
                orderRecord.Status,
                CustomerName = orderRecord.Customer == null ? null : orderRecord.Customer.Name,
                orderRecord.Priority,
                AssignmentNames = orderRecord.Assignments
                    .Select(assignment => assignment.ApplicationUser != null
                        ? assignment.ApplicationUser.DisplayName
                            ?? assignment.ApplicationUser.EmailAddress
                            ?? assignment.ApplicationUser.WindowsUserName
                        : assignment.ImportedUserText ?? string.Empty)
                    .ToArray(),
                orderRecord.DueDate,
                CompletedChecklistItems = orderRecord.ChecklistItems.Count(checklistItem => checklistItem.IsCompleted),
                TotalChecklistItems = orderRecord.ChecklistItems.Count(),
                LinkedBuildRecords = orderRecord.BuildRecordLinks.Count(),
                orderRecord.ContractReadyForInvoicing,
                orderRecord.ReadyForInvoicingDate,
                orderRecord.InvoiceNumber,
                orderRecord.InvoicedDate,
                orderRecord.ShippedDate
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new OrderBoardCardModel(
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
                row.LinkedBuildRecords > 0,
                row.DueDate is not null && row.DueDate < today && !string.Equals(row.Status, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal),
                BuildWarnings(row.Status, row.AssignmentNames, row.CompletedChecklistItems, row.TotalChecklistItems, row.LinkedBuildRecords, row.ContractReadyForInvoicing, row.ReadyForInvoicingDate, row.InvoiceNumber, row.InvoicedDate, row.ShippedDate)))
            .ToList();
    }

    private static IReadOnlyList<string> BuildWarnings(
        string status,
        IReadOnlyCollection<string> assignmentNames,
        int completedChecklistItems,
        int totalChecklistItems,
        int linkedBuildRecords,
        bool? contractReadyForInvoicing,
        DateOnly? readyForInvoicingDate,
        string? invoiceNumber,
        DateOnly? invoicedDate,
        DateOnly? shippedDate)
    {
        var warnings = new List<string>();
        var requiresReadinessChecks =
            string.Equals(status, BuildBookOrderStatuses.PreparedForShipping, StringComparison.Ordinal)
            || string.Equals(status, BuildBookOrderStatuses.ReadyForCollection, StringComparison.Ordinal)
            || string.Equals(status, BuildBookOrderStatuses.Shipped, StringComparison.Ordinal)
            || string.Equals(status, BuildBookOrderStatuses.ContractReadyForInvoicing, StringComparison.Ordinal)
            || string.Equals(status, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal);

        if (requiresReadinessChecks && assignmentNames.All(string.IsNullOrWhiteSpace))
        {
            warnings.Add("No assignments");
        }

        if (requiresReadinessChecks && totalChecklistItems > 0 && completedChecklistItems < totalChecklistItems)
        {
            warnings.Add("Checklist incomplete");
        }

        if (linkedBuildRecords == 0
            && (string.Equals(status, BuildBookOrderStatuses.Built, StringComparison.Ordinal)
                || string.Equals(status, BuildBookOrderStatuses.PreparedForShipping, StringComparison.Ordinal)
                || string.Equals(status, BuildBookOrderStatuses.ReadyForCollection, StringComparison.Ordinal)
                || string.Equals(status, BuildBookOrderStatuses.Shipped, StringComparison.Ordinal)))
        {
            warnings.Add("No linked Build Record");
        }

        if (string.Equals(status, BuildBookOrderStatuses.Shipped, StringComparison.Ordinal) && shippedDate is null)
        {
            warnings.Add("Shipped date missing");
        }

        if (string.Equals(status, BuildBookOrderStatuses.ContractReadyForInvoicing, StringComparison.Ordinal))
        {
            if (contractReadyForInvoicing != true)
            {
                warnings.Add("Invoice readiness not marked");
            }

            if (readyForInvoicingDate is null)
            {
                warnings.Add("Invoice readiness date missing");
            }
        }

        if (string.Equals(status, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                warnings.Add("Invoice number missing");
            }

            if (invoicedDate is null)
            {
                warnings.Add("Invoiced date missing");
            }
        }

        return warnings;
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
}
