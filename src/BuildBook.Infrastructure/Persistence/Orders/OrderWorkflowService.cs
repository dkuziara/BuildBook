using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using BuildBook.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderWorkflowService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IOrderWorkflowService
{
    private static readonly HashSet<string> WorkflowStatusesRequiringReadinessChecks =
    [
        BuildBookOrderStatuses.PreparedForShipping,
        BuildBookOrderStatuses.ReadyForCollection,
        BuildBookOrderStatuses.Shipped,
        BuildBookOrderStatuses.ContractReadyForInvoicing,
        BuildBookOrderStatuses.Invoiced
    ];

    public async Task<OrderOperationResult> UpdateWorkflowAsync(
        int orderRecordId,
        UpdateOrderWorkflowRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (request.StartDate is not null
            && request.DueDate is not null
            && request.DueDate.Value < request.StartDate.Value)
        {
            return OrderOperationResult.Failure("Due date cannot be before the start date.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        if (orderRecord.Priority == request.Priority
            && orderRecord.StartDate == request.StartDate
            && orderRecord.DueDate == request.DueDate)
        {
            return OrderOperationResult.Success();
        }

        orderRecord.Priority = request.Priority;
        orderRecord.StartDate = request.StartDate;
        orderRecord.DueDate = request.DueDate;

        await SaveOrderAsync(dbContext, orderRecord, updatedBy, cancellationToken);
        return OrderOperationResult.Success();
    }

    public async Task<OrderOperationResult> UpdateShippingAsync(
        int orderRecordId,
        UpdateOrderShippingRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        var shippingMethod = NormalizeOptionalValue(request.ShippingMethod);
        var courier = NormalizeOptionalValue(request.Courier);
        var trackingNumber = NormalizeOptionalValue(request.TrackingNumber);
        var shippingNotes = NormalizeOptionalValue(request.ShippingNotes);

        if (orderRecord.ShippingRequired == request.ShippingRequired
            && orderRecord.ShippingMethod == shippingMethod
            && orderRecord.Courier == courier
            && orderRecord.TrackingNumber == trackingNumber
            && orderRecord.CollectionRequired == request.CollectionRequired
            && orderRecord.CollectionDate == request.CollectionDate
            && orderRecord.ShippedDate == request.ShippedDate
            && orderRecord.ShippingNotes == shippingNotes)
        {
            return OrderOperationResult.Success();
        }

        var actor = NormalizeActor(updatedBy);
        var actorUser = await FindApplicationUserAsync(dbContext, actor, cancellationToken);

        orderRecord.ShippingRequired = request.ShippingRequired;
        orderRecord.ShippingMethod = shippingMethod;
        orderRecord.Courier = courier;
        orderRecord.TrackingNumber = trackingNumber;
        orderRecord.CollectionRequired = request.CollectionRequired;
        orderRecord.CollectionDate = request.CollectionDate;
        orderRecord.ShippedDate = request.ShippedDate;
        orderRecord.ShippedByUserId = request.ShippedDate is null ? null : actorUser?.Id;
        orderRecord.ShippingNotes = shippingNotes;

        await SaveOrderAsync(dbContext, orderRecord, actor, cancellationToken, actorUser);
        return OrderOperationResult.Success();
    }

    public async Task<OrderOperationResult> UpdateInvoicingAsync(
        int orderRecordId,
        UpdateOrderInvoicingRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        var invoiceNumber = NormalizeOptionalValue(request.InvoiceNumber);
        var invoicingNotes = NormalizeOptionalValue(request.InvoicingNotes);

        if (orderRecord.ContractReadyForInvoicing == request.ContractReadyForInvoicing
            && orderRecord.ReadyForInvoicingDate == request.ReadyForInvoicingDate
            && orderRecord.InvoiceNumber == invoiceNumber
            && orderRecord.InvoicedDate == request.InvoicedDate
            && orderRecord.InvoicingNotes == invoicingNotes)
        {
            return OrderOperationResult.Success();
        }

        var actor = NormalizeActor(updatedBy);
        var actorUser = await FindApplicationUserAsync(dbContext, actor, cancellationToken);

        orderRecord.ContractReadyForInvoicing = request.ContractReadyForInvoicing;
        orderRecord.ReadyForInvoicingDate = request.ReadyForInvoicingDate;
        orderRecord.InvoiceNumber = invoiceNumber;
        orderRecord.InvoicedDate = request.InvoicedDate;
        orderRecord.InvoicedByUserId = request.InvoicedDate is null ? null : actorUser?.Id;
        orderRecord.InvoicingNotes = invoicingNotes;

        await SaveOrderAsync(dbContext, orderRecord, actor, cancellationToken, actorUser);
        return OrderOperationResult.Success();
    }

    public async Task<ChangeOrderStatusResult> ChangeStatusAsync(
        int orderRecordId,
        ChangeOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var newStatus = NormalizeOptionalValue(request.NewStatus);
        if (newStatus is null)
        {
            return ChangeOrderStatusResult.Failure("Select a new status.");
        }

        if (!BuildBookOrderStatuses.DefaultWorkflow.Contains(newStatus, StringComparer.Ordinal))
        {
            return ChangeOrderStatusResult.Failure("The selected status is not valid.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .Include(record => record.Assignments)
            .Include(record => record.ChecklistItems)
            .Include(record => record.StatusHistoryEntries)
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return ChangeOrderStatusResult.Failure("Order was not found.");
        }

        if (string.Equals(orderRecord.Status, newStatus, StringComparison.Ordinal))
        {
            return ChangeOrderStatusResult.Success();
        }

        if (!request.IgnoreReadinessWarnings)
        {
            var warnings = BuildStatusWarnings(orderRecord, newStatus);
            if (warnings.Count > 0)
            {
                return ChangeOrderStatusResult.WarningConfirmationRequired(warnings);
            }
        }

        var actor = NormalizeActor(updatedBy);
        var actorUser = await FindApplicationUserAsync(dbContext, actor, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var oldStatus = orderRecord.Status;

        orderRecord.Status = newStatus;
        if (string.Equals(newStatus, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal))
        {
            orderRecord.CompletedAt ??= now;
            orderRecord.CompletedByUserId ??= actorUser?.Id;
            orderRecord.ImportedCompletedByText ??= actorUser is null ? actor : null;
        }

        orderRecord.StatusHistoryEntries.Add(new OrderStatusHistory
        {
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedAt = now,
            ChangedByUserId = actorUser?.Id,
            Reason = NormalizeOptionalValue(request.Reason)
        });

        await SaveOrderAsync(dbContext, orderRecord, actor, cancellationToken, actorUser, now);
        return ChangeOrderStatusResult.Success();
    }

    public async Task<OrderOperationResult> SaveAssignmentAsync(
        int orderRecordId,
        SaveOrderAssignmentRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var importedUserText = NormalizeOptionalValue(request.ImportedUserText);
        if (request.ApplicationUserId is null && importedUserText is null)
        {
            return OrderOperationResult.Failure("Select a BuildBook user or enter an imported user name.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .Include(record => record.Assignments)
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        ApplicationUser? assignmentUser = null;
        if (request.ApplicationUserId is not null)
        {
            assignmentUser = await dbContext.ApplicationUsers
                .SingleOrDefaultAsync(user => user.Id == request.ApplicationUserId.Value && user.IsActive, cancellationToken);

            if (assignmentUser is null)
            {
                return OrderOperationResult.Failure("The selected BuildBook user could not be found.");
            }
        }

        var duplicateAssignmentExists = orderRecord.Assignments.Any(assignment =>
            assignment.ApplicationUserId == request.ApplicationUserId
            && string.Equals(assignment.ImportedUserText, importedUserText, StringComparison.OrdinalIgnoreCase)
            && assignment.AssignmentType == request.AssignmentType);

        if (duplicateAssignmentExists)
        {
            return OrderOperationResult.Failure("That assignment already exists on this Order.");
        }

        var actor = NormalizeActor(updatedBy);
        var actorUser = await FindApplicationUserAsync(dbContext, actor, cancellationToken);

        orderRecord.Assignments.Add(new OrderAssignment
        {
            ApplicationUserId = assignmentUser?.Id,
            ImportedUserText = importedUserText,
            AssignmentType = request.AssignmentType,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedByUserId = actorUser?.Id
        });

        await SaveOrderAsync(dbContext, orderRecord, actor, cancellationToken, actorUser);
        return OrderOperationResult.Success();
    }

    public async Task<OrderOperationResult> DeleteAssignmentAsync(
        int orderRecordId,
        int assignmentId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        var assignment = await dbContext.OrderAssignments
            .SingleOrDefaultAsync(item => item.Id == assignmentId && item.OrderRecordId == orderRecordId, cancellationToken);

        if (assignment is null)
        {
            return OrderOperationResult.Failure("Assignment was not found.");
        }

        dbContext.OrderAssignments.Remove(assignment);
        await SaveOrderAsync(dbContext, orderRecord, updatedBy, cancellationToken);
        return OrderOperationResult.Success();
    }

    public async Task<OrderOperationResult> UpdateChecklistItemAsync(
        int orderRecordId,
        UpdateOrderChecklistItemRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        if (request.ChecklistItemId is null)
        {
            var text = NormalizeOptionalValue(request.Text);
            if (text is null)
            {
                return OrderOperationResult.Failure("Checklist item text is required.");
            }

            var maxDisplayOrder = await dbContext.OrderChecklistItems
                .Where(item => item.OrderRecordId == orderRecordId)
                .Select(item => (int?)item.DisplayOrder)
                .MaxAsync(cancellationToken)
                ?? 0;

            dbContext.OrderChecklistItems.Add(new OrderChecklistItem
            {
                OrderRecordId = orderRecordId,
                DisplayOrder = maxDisplayOrder + 1,
                Text = text,
                ShowInBoardView = false,
                Source = "Manual"
            });

            await SaveOrderAsync(dbContext, orderRecord, updatedBy, cancellationToken);
            return OrderOperationResult.Success();
        }

        var checklistItem = await dbContext.OrderChecklistItems
            .SingleOrDefaultAsync(item => item.Id == request.ChecklistItemId.Value && item.OrderRecordId == orderRecordId, cancellationToken);

        if (checklistItem is null)
        {
            return OrderOperationResult.Failure("Checklist item was not found.");
        }

        if (request.IsCompleted is null)
        {
            return OrderOperationResult.Failure("Checklist completion state was not provided.");
        }

        var actor = NormalizeActor(updatedBy);
        var actorUser = await FindApplicationUserAsync(dbContext, actor, cancellationToken);
        checklistItem.IsCompleted = request.IsCompleted.Value;
        checklistItem.CompletedAt = request.IsCompleted.Value ? DateTimeOffset.UtcNow : null;
        checklistItem.CompletedByUserId = request.IsCompleted.Value ? actorUser?.Id : null;
        checklistItem.ImportedCompletedText = request.IsCompleted.Value && actorUser is null ? actor : null;

        await SaveOrderAsync(dbContext, orderRecord, actor, cancellationToken, actorUser);
        return OrderOperationResult.Success();
    }

    private static List<string> BuildStatusWarnings(OrderRecord orderRecord, string newStatus)
    {
        if (!WorkflowStatusesRequiringReadinessChecks.Contains(newStatus))
        {
            return [];
        }

        var warnings = new List<string>();
        if (orderRecord.Assignments.Count == 0)
        {
            warnings.Add("This Order has no assignments yet.");
        }

        if (orderRecord.ChecklistItems.Any(item => !item.IsCompleted))
        {
            warnings.Add("Checklist items are still incomplete.");
        }

        if (string.Equals(newStatus, BuildBookOrderStatuses.Shipped, StringComparison.Ordinal)
            && orderRecord.ShippedDate is null)
        {
            warnings.Add("Status is Shipped but shipped date is missing.");
        }

        if (string.Equals(newStatus, BuildBookOrderStatuses.ContractReadyForInvoicing, StringComparison.Ordinal))
        {
            if (orderRecord.ReadyForInvoicingDate is null)
            {
                warnings.Add("Status is Contract Ready for Invoicing but invoice readiness date is missing.");
            }

            if (orderRecord.ContractReadyForInvoicing != true)
            {
                warnings.Add("Contract ready for invoicing has not been marked.");
            }
        }

        if (string.Equals(newStatus, BuildBookOrderStatuses.Invoiced, StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(orderRecord.InvoiceNumber))
            {
                warnings.Add("Status is Invoiced but invoice number is missing.");
            }

            if (orderRecord.InvoicedDate is null)
            {
                warnings.Add("Status is Invoiced but invoiced date is missing.");
            }
        }

        return warnings;
    }

    private static async Task SaveOrderAsync(
        BuildBookDbContext dbContext,
        OrderRecord orderRecord,
        string updatedBy,
        CancellationToken cancellationToken,
        ApplicationUser? actorUser = null,
        DateTimeOffset? updatedAt = null)
    {
        orderRecord.LastUpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
        orderRecord.LastUpdatedByUserId = actorUser?.Id;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<ApplicationUser?> FindApplicationUserAsync(
        BuildBookDbContext dbContext,
        string actor,
        CancellationToken cancellationToken)
    {
        return await dbContext.ApplicationUsers
            .SingleOrDefaultAsync(
                user => user.IsActive
                    && (user.WindowsUserName == actor
                        || user.EmailAddress == actor
                        || user.DisplayName == actor),
                cancellationToken);
    }

    private static string NormalizeActor(string? actor)
    {
        return string.IsNullOrWhiteSpace(actor) ? "Unknown" : actor.Trim();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
