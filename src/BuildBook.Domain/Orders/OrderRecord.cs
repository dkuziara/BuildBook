using BuildBook.Domain.Customers;
using BuildBook.Domain.Security;

namespace BuildBook.Domain.Orders;

public sealed class OrderRecord
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string OrderTitle { get; set; } = string.Empty;

    public string? OrderDescription { get; set; }

    public string? ProductCode { get; set; }

    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string Status { get; set; } = "Order Received";

    public OrderPriority? Priority { get; set; }

    public string? ImportedPriorityText { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public int? CompletedByUserId { get; set; }

    public ApplicationUser? CompletedByUser { get; set; }

    public string? ImportedCompletedByText { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? CreatedByUserId { get; set; }

    public ApplicationUser? CreatedByUser { get; set; }

    public string? ImportedCreatedByText { get; set; }

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? LastUpdatedByUserId { get; set; }

    public ApplicationUser? LastUpdatedByUser { get; set; }

    public bool IsRecurring { get; set; }

    public string? PlannerTaskId { get; set; }

    public string? PlannerPlanId { get; set; }

    public string? PlannerBucketId { get; set; }

    public string? PlannerBucketName { get; set; }

    public string? PlannerSource { get; set; }

    public string? PlannerStatus { get; set; }

    public string? PlannerGoal { get; set; }

    public bool? ImportedLateFlag { get; set; }

    public string? CustomerReference { get; set; }

    public string? CustomerPurchaseOrderNumber { get; set; }

    public string? InternalOrderReference { get; set; }

    public string? QuoteNumber { get; set; }

    public int? SalesAdminOwnerUserId { get; set; }

    public ApplicationUser? SalesAdminOwnerUser { get; set; }

    public int? ProductionOwnerUserId { get; set; }

    public ApplicationUser? ProductionOwnerUser { get; set; }

    public string? NotesSummary { get; set; }

    public string? SupportTicketNo { get; set; }

    public bool? ShippingRequired { get; set; }

    public string? ShippingMethod { get; set; }

    public string? Courier { get; set; }

    public string? TrackingNumber { get; set; }

    public bool? CollectionRequired { get; set; }

    public DateOnly? CollectionDate { get; set; }

    public DateOnly? ShippedDate { get; set; }

    public int? ShippedByUserId { get; set; }

    public ApplicationUser? ShippedByUser { get; set; }

    public string? ShippingNotes { get; set; }

    public bool? ContractReadyForInvoicing { get; set; }

    public DateOnly? ReadyForInvoicingDate { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateOnly? InvoicedDate { get; set; }

    public int? InvoicedByUserId { get; set; }

    public ApplicationUser? InvoicedByUser { get; set; }

    public string? InvoicingNotes { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<OrderAssignment> Assignments { get; } = new List<OrderAssignment>();

    public ICollection<OrderChecklistItem> ChecklistItems { get; } = new List<OrderChecklistItem>();

    public ICollection<OrderNote> Notes { get; } = new List<OrderNote>();

    public ICollection<OrderLabel> Labels { get; } = new List<OrderLabel>();

    public ICollection<OrderBuildRecordLink> BuildRecordLinks { get; } = new List<OrderBuildRecordLink>();

    public ICollection<OrderStatusHistory> StatusHistoryEntries { get; } = new List<OrderStatusHistory>();
}
