using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderRegisterReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IOrderRegisterReader
{
    public async Task<IReadOnlyList<OrderRegisterRow>> ListAsync(
        OrderRegisterFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        filter ??= new OrderRegisterFilter();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.OrderRecords
            .AsNoTracking()
            .Where(orderRecord => orderRecord.IsActive);

        query = ApplyFilters(query, filter);

        var rows = await query
            .Select(orderRecord => new RegisterProjection
            {
                Id = orderRecord.Id,
                OrderNumber = orderRecord.OrderNumber,
                OrderTitle = orderRecord.OrderTitle,
                ProductCode = orderRecord.ProductCode,
                LinkedProductId = orderRecord.ProductCode == null
                    ? null
                    : dbContext.Products
                        .Where(product => product.ProductCode == orderRecord.ProductCode)
                        .Select(product => (int?)product.Id)
                        .FirstOrDefault(),
                Status = orderRecord.Status,
                CustomerName = orderRecord.Customer == null ? null : orderRecord.Customer.Name,
                Priority = orderRecord.Priority,
                AssignmentNames = orderRecord.Assignments
                    .Select(assignment => assignment.ApplicationUser != null
                        ? assignment.ApplicationUser.DisplayName
                            ?? assignment.ApplicationUser.EmailAddress
                            ?? assignment.ApplicationUser.WindowsUserName
                        : assignment.ImportedUserText ?? string.Empty)
                    .ToArray(),
                StartDate = orderRecord.StartDate,
                DueDate = orderRecord.DueDate,
                CompletedChecklistItems = orderRecord.ChecklistItems.Count(checklistItem => checklistItem.IsCompleted),
                TotalChecklistItems = orderRecord.ChecklistItems.Count(),
                LinkedBuildRecords = orderRecord.BuildRecordLinks.Count(),
                LastUpdatedAt = orderRecord.LastUpdatedAt
            })
            .ToListAsync(cancellationToken);

        var projectedRows = rows
            .Select(row => new OrderRegisterRow(
                row.Id,
                row.OrderNumber,
                row.OrderTitle,
                row.ProductCode,
                row.LinkedProductId,
                row.Status,
                row.CustomerName,
                row.Priority,
                SummarizeAssignments(row.AssignmentNames),
                row.StartDate,
                row.DueDate,
                row.CompletedChecklistItems,
                row.TotalChecklistItems,
                row.LinkedBuildRecords,
                row.LastUpdatedAt))
            .ToList();

        return ApplySort(projectedRows, filter);
    }

    private static IQueryable<OrderRecord> ApplyFilters(
        IQueryable<OrderRecord> query,
        OrderRegisterFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(orderRecord =>
                orderRecord.OrderNumber.Contains(search)
                || orderRecord.OrderTitle.Contains(search)
                || (orderRecord.ProductCode != null && orderRecord.ProductCode.Contains(search))
                || (orderRecord.Customer != null && orderRecord.Customer.Name.Contains(search))
                || (orderRecord.CustomerReference != null && orderRecord.CustomerReference.Contains(search))
                || (orderRecord.CustomerPurchaseOrderNumber != null && orderRecord.CustomerPurchaseOrderNumber.Contains(search))
                || (orderRecord.InternalOrderReference != null && orderRecord.InternalOrderReference.Contains(search))
                || (orderRecord.QuoteNumber != null && orderRecord.QuoteNumber.Contains(search))
                || (orderRecord.SupportTicketNo != null && orderRecord.SupportTicketNo.Contains(search))
                || (orderRecord.PlannerTaskId != null && orderRecord.PlannerTaskId.Contains(search))
                || orderRecord.Assignments.Any(assignment =>
                    (assignment.ImportedUserText != null && assignment.ImportedUserText.Contains(search))
                    || (assignment.ApplicationUser != null
                        && ((assignment.ApplicationUser.DisplayName != null && assignment.ApplicationUser.DisplayName.Contains(search))
                            || (assignment.ApplicationUser.EmailAddress != null && assignment.ApplicationUser.EmailAddress.Contains(search))
                            || assignment.ApplicationUser.WindowsUserName.Contains(search))))
                || orderRecord.Notes.Any(note => note.NoteText.Contains(search))
                || orderRecord.ChecklistItems.Any(checklistItem => checklistItem.Text.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Customer))
        {
            var customer = filter.Customer.Trim();
            query = query.Where(orderRecord => orderRecord.Customer != null && orderRecord.Customer.Name.Contains(customer));
        }

        if (!string.IsNullOrWhiteSpace(filter.AssignedTo))
        {
            var assignedTo = filter.AssignedTo.Trim();
            query = query.Where(orderRecord => orderRecord.Assignments.Any(assignment =>
                (assignment.ImportedUserText != null && assignment.ImportedUserText.Contains(assignedTo))
                || (assignment.ApplicationUser != null
                    && ((assignment.ApplicationUser.DisplayName != null && assignment.ApplicationUser.DisplayName.Contains(assignedTo))
                        || (assignment.ApplicationUser.EmailAddress != null && assignment.ApplicationUser.EmailAddress.Contains(assignedTo))
                        || assignment.ApplicationUser.WindowsUserName.Contains(assignedTo)))));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(orderRecord => orderRecord.Status == status);
        }

        if (filter.Priority is not null)
        {
            query = query.Where(orderRecord => orderRecord.Priority == filter.Priority);
        }

        if (filter.DueDate is not null)
        {
            query = query.Where(orderRecord => orderRecord.DueDate == filter.DueDate);
        }

        if (filter.IsOverdue is not null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = filter.IsOverdue.Value
                ? query.Where(orderRecord => orderRecord.DueDate != null && orderRecord.DueDate < today && orderRecord.CompletedAt == null)
                : query.Where(orderRecord => orderRecord.DueDate == null || orderRecord.DueDate >= today || orderRecord.CompletedAt != null);
        }

        if (filter.IsCompleted is not null)
        {
            query = filter.IsCompleted.Value
                ? query.Where(orderRecord => orderRecord.CompletedAt != null)
                : query.Where(orderRecord => orderRecord.CompletedAt == null);
        }

        if (filter.HasLinkedBuildRecord is not null)
        {
            query = filter.HasLinkedBuildRecord.Value
                ? query.Where(orderRecord => orderRecord.BuildRecordLinks.Any())
                : query.Where(orderRecord => !orderRecord.BuildRecordLinks.Any());
        }

        return query;
    }

    private static IReadOnlyList<OrderRegisterRow> ApplySort(
        IReadOnlyList<OrderRegisterRow> rows,
        OrderRegisterFilter filter)
    {
        Func<OrderRegisterRow, object?> keySelector = filter.SortBy switch
        {
            OrderRegisterSortColumn.Order => row => row.OrderNumber,
            OrderRegisterSortColumn.Status => row => row.Status,
            OrderRegisterSortColumn.Customer => row => row.CustomerName,
            OrderRegisterSortColumn.Priority => row => row.Priority,
            OrderRegisterSortColumn.AssignedTo => row => row.AssignedTo,
            OrderRegisterSortColumn.StartDate => row => row.StartDate,
            OrderRegisterSortColumn.DueDate => row => row.DueDate,
            OrderRegisterSortColumn.ChecklistProgress => row => row.TotalChecklistItems == 0
                ? -1
                : (decimal)row.CompletedChecklistItems / row.TotalChecklistItems,
            OrderRegisterSortColumn.LinkedBuilds => row => row.LinkedBuildRecords,
            _ => row => row.LastUpdatedAt
        };

        return filter.SortDescending
            ? rows.OrderByDescending(keySelector).ThenBy(row => row.OrderNumber, StringComparer.OrdinalIgnoreCase).ToArray()
            : rows.OrderBy(keySelector).ThenBy(row => row.OrderNumber, StringComparer.OrdinalIgnoreCase).ToArray();
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

    private sealed class RegisterProjection
    {
        public int Id { get; init; }

        public string OrderNumber { get; init; } = string.Empty;

        public string OrderTitle { get; init; } = string.Empty;

        public string? ProductCode { get; init; }

        public int? LinkedProductId { get; init; }

        public string Status { get; init; } = string.Empty;

        public string? CustomerName { get; init; }

        public OrderPriority? Priority { get; init; }

        public string[] AssignmentNames { get; init; } = [];

        public DateOnly? StartDate { get; init; }

        public DateOnly? DueDate { get; init; }

        public int CompletedChecklistItems { get; init; }

        public int TotalChecklistItems { get; init; }

        public int LinkedBuildRecords { get; init; }

        public DateTimeOffset LastUpdatedAt { get; init; }
    }
}
