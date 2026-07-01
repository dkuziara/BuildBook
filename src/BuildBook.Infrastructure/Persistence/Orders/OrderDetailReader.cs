using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderDetailReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IOrderDetailReader
{
    public async Task<OrderDetailModel?> GetByIdAsync(
        int orderRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var orderRecord = await dbContext.OrderRecords
            .AsNoTracking()
            .Where(record => record.Id == orderRecordId && record.IsActive)
            .Select(record => new
            {
                record.Id,
                record.OrderNumber,
                record.OrderTitle,
                record.OrderDescription,
                record.CustomerId,
                CustomerName = record.Customer == null ? null : record.Customer.Name,
                record.Status,
                record.Priority,
                record.ImportedPriorityText,
                record.StartDate,
                record.DueDate,
                record.CompletedAt,
                CompletedBy = record.CompletedByUser != null
                    ? record.CompletedByUser.DisplayName
                        ?? record.CompletedByUser.EmailAddress
                        ?? record.CompletedByUser.WindowsUserName
                    : record.ImportedCompletedByText,
                record.CreatedAt,
                CreatedBy = record.CreatedByUser != null
                    ? record.CreatedByUser.DisplayName
                        ?? record.CreatedByUser.EmailAddress
                        ?? record.CreatedByUser.WindowsUserName
                    : record.ImportedCreatedByText,
                record.LastUpdatedAt,
                LastUpdatedBy = record.LastUpdatedByUser != null
                    ? record.LastUpdatedByUser.DisplayName
                        ?? record.LastUpdatedByUser.EmailAddress
                        ?? record.LastUpdatedByUser.WindowsUserName
                    : null,
                record.IsRecurring,
                record.PlannerTaskId,
                record.PlannerPlanId,
                record.PlannerBucketId,
                record.PlannerBucketName,
                record.PlannerSource,
                record.PlannerStatus,
                record.PlannerGoal,
                record.ImportedLateFlag,
                record.CustomerReference,
                record.CustomerPurchaseOrderNumber,
                record.InternalOrderReference,
                record.QuoteNumber,
                SalesAdminOwner = record.SalesAdminOwnerUser != null
                    ? record.SalesAdminOwnerUser.DisplayName
                        ?? record.SalesAdminOwnerUser.EmailAddress
                        ?? record.SalesAdminOwnerUser.WindowsUserName
                    : null,
                ProductionOwner = record.ProductionOwnerUser != null
                    ? record.ProductionOwnerUser.DisplayName
                        ?? record.ProductionOwnerUser.EmailAddress
                        ?? record.ProductionOwnerUser.WindowsUserName
                    : null,
                record.NotesSummary,
                record.SupportTicketNo,
                Assignments = record.Assignments
                    .OrderBy(assignment => assignment.AssignedAt)
                    .Select(assignment => new OrderAssignmentSummary(
                        assignment.ApplicationUser != null
                            ? assignment.ApplicationUser.DisplayName
                                ?? assignment.ApplicationUser.EmailAddress
                                ?? assignment.ApplicationUser.WindowsUserName
                            : assignment.ImportedUserText ?? "Unknown",
                        assignment.AssignmentType == null ? null : assignment.AssignmentType.ToString(),
                        assignment.AssignedAt))
                    .ToArray(),
                ChecklistItems = record.ChecklistItems
                    .OrderBy(checklistItem => checklistItem.DisplayOrder)
                    .Select(checklistItem => new OrderChecklistItemSummary(
                        checklistItem.DisplayOrder,
                        checklistItem.Text,
                        checklistItem.IsCompleted,
                        checklistItem.CompletedByUser != null
                            ? checklistItem.CompletedByUser.DisplayName
                                ?? checklistItem.CompletedByUser.EmailAddress
                                ?? checklistItem.CompletedByUser.WindowsUserName
                            : checklistItem.ImportedCompletedText,
                        checklistItem.CompletedAt,
                        checklistItem.Source))
                    .ToArray(),
                Notes = record.Notes
                    .OrderByDescending(note => note.CreatedAt)
                    .Select(note => new OrderNoteSummary(
                        note.Id,
                        FormatNoteType(note.NoteType),
                        note.NoteText,
                        note.CreatedByUser != null
                            ? note.CreatedByUser.DisplayName
                                ?? note.CreatedByUser.EmailAddress
                                ?? note.CreatedByUser.WindowsUserName
                            : null,
                        note.CreatedAt,
                        note.LastUpdatedAt))
                    .ToArray(),
                Labels = record.Labels
                    .OrderBy(label => label.LabelText)
                    .Select(label => new OrderLabelSummary(
                        label.LabelText,
                        label.Source))
                    .ToArray(),
                LinkedBuildRecords = record.BuildRecordLinks
                    .OrderBy(link => link.LinkedAt)
                    .Select(link => new OrderLinkedBuildRecordSummary(
                        link.BuildRecordId,
                        link.BuildRecord != null ? link.BuildRecord.ProductCode : string.Empty,
                        link.BuildRecord != null ? link.BuildRecord.ProductName : string.Empty,
                        link.BuildRecord != null ? link.BuildRecord.SerialNumber : string.Empty,
                        link.BuildRecord != null ? link.BuildRecord.MachineName : null,
                        link.LinkType,
                        link.LinkedAt))
                    .ToArray(),
                StatusHistory = record.StatusHistoryEntries
                    .OrderByDescending(entry => entry.ChangedAt)
                    .Select(entry => new OrderStatusHistorySummary(
                        entry.OldStatus,
                        entry.NewStatus,
                        entry.ChangedByUser != null
                            ? entry.ChangedByUser.DisplayName
                                ?? entry.ChangedByUser.EmailAddress
                                ?? entry.ChangedByUser.WindowsUserName
                            : null,
                        entry.ChangedAt,
                        entry.Reason))
                    .ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return orderRecord is null
            ? null
            : new OrderDetailModel(
                orderRecord.Id,
                orderRecord.OrderNumber,
                orderRecord.OrderTitle,
                orderRecord.OrderDescription,
                orderRecord.CustomerId,
                orderRecord.CustomerName,
                orderRecord.Status,
                orderRecord.Priority,
                orderRecord.ImportedPriorityText,
                orderRecord.StartDate,
                orderRecord.DueDate,
                orderRecord.CompletedAt,
                orderRecord.CompletedBy,
                orderRecord.CreatedAt,
                orderRecord.CreatedBy,
                orderRecord.LastUpdatedAt,
                orderRecord.LastUpdatedBy,
                orderRecord.IsRecurring,
                orderRecord.PlannerTaskId,
                orderRecord.PlannerPlanId,
                orderRecord.PlannerBucketId,
                orderRecord.PlannerBucketName,
                orderRecord.PlannerSource,
                orderRecord.PlannerStatus,
                orderRecord.PlannerGoal,
                orderRecord.ImportedLateFlag,
                orderRecord.CustomerReference,
                orderRecord.CustomerPurchaseOrderNumber,
                orderRecord.InternalOrderReference,
                orderRecord.QuoteNumber,
                orderRecord.SalesAdminOwner,
                orderRecord.ProductionOwner,
                orderRecord.NotesSummary,
                orderRecord.SupportTicketNo,
                orderRecord.Assignments,
                orderRecord.ChecklistItems,
                orderRecord.Notes,
                orderRecord.Labels,
                orderRecord.LinkedBuildRecords,
                orderRecord.StatusHistory);
    }

    private static string FormatNoteType(OrderNoteType noteType)
    {
        return noteType switch
        {
            OrderNoteType.ProductionNote => "Production Note",
            OrderNoteType.ShippingNote => "Shipping Note",
            OrderNoteType.InvoicingNote => "Invoicing Note",
            OrderNoteType.PlannerImportedNote => "Planner Imported Note",
            _ => "Internal Note"
        };
    }
}
