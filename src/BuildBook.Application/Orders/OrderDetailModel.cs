using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public sealed record OrderDetailModel(
    int Id,
    string OrderNumber,
    string OrderTitle,
    string? OrderDescription,
    int? CustomerId,
    string? CustomerName,
    string Status,
    OrderPriority? Priority,
    string? ImportedPriorityText,
    DateOnly? StartDate,
    DateOnly? DueDate,
    DateTimeOffset? CompletedAt,
    string? CompletedBy,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset LastUpdatedAt,
    string? LastUpdatedBy,
    bool IsRecurring,
    string? PlannerTaskId,
    string? PlannerPlanId,
    string? PlannerBucketId,
    string? PlannerBucketName,
    string? PlannerSource,
    string? PlannerStatus,
    string? PlannerGoal,
    bool? ImportedLateFlag,
    string? CustomerReference,
    string? CustomerPurchaseOrderNumber,
    string? InternalOrderReference,
    string? QuoteNumber,
    string? SalesAdminOwner,
    string? ProductionOwner,
    string? NotesSummary,
    string? SupportTicketNo,
    IReadOnlyList<OrderAssignmentSummary> Assignments,
    IReadOnlyList<OrderChecklistItemSummary> ChecklistItems,
    IReadOnlyList<OrderNoteSummary> Notes,
    IReadOnlyList<OrderLabelSummary> Labels,
    IReadOnlyList<OrderLinkedBuildRecordSummary> LinkedBuildRecords,
    IReadOnlyList<OrderStatusHistorySummary> StatusHistory);

public sealed record OrderAssignmentSummary(
    int Id,
    string DisplayName,
    string? AssignmentType,
    DateTimeOffset AssignedAt);

public sealed record OrderChecklistItemSummary(
    int Id,
    int DisplayOrder,
    string Text,
    bool IsCompleted,
    string? CompletedBy,
    DateTimeOffset? CompletedAt,
    string? Source);

public sealed record OrderNoteSummary(
    int Id,
    string NoteType,
    string NoteText,
    string? CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUpdatedAt);

public sealed record OrderLabelSummary(
    string LabelText,
    string? Source);

public sealed record OrderLinkedBuildRecordSummary(
    int BuildRecordId,
    string ProductCode,
    string ProductName,
    string SerialNumber,
    string? MachineName,
    string? LinkType,
    DateTimeOffset LinkedAt);

public sealed record OrderStatusHistorySummary(
    string? OldStatus,
    string NewStatus,
    string? ChangedBy,
    DateTimeOffset ChangedAt,
    string? Reason);
