using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;

namespace BuildBook.Domain.Rmas;

public sealed class RmaRecord
{
    public int Id { get; set; }

    public string RmaNumber { get; set; } = string.Empty;

    public int? BuildRecordId { get; set; }

    public BuildRecord? BuildRecord { get; set; }

    public RmaStatus Status { get; set; } = RmaStatus.BookedIn;

    public RmaPriority? Priority { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = string.Empty;

    public DateTimeOffset? ClosedAt { get; set; }

    public string? ClosedBy { get; set; }

    public string? ProductCode { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? SerialNumber { get; set; }

    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerReference { get; set; }

    public string? SupportTicketNumber { get; set; }

    public string? SupportTicketUrl { get; set; }

    public string? OriginalOrderNumber { get; set; }

    public DateOnly? OriginalOrderDate { get; set; }

    public string? OriginalInvoiceNumber { get; set; }

    public string FaultSummary { get; set; } = string.Empty;

    public string InitialFaultDescription { get; set; } = string.Empty;

    public string? FaultDescription { get; set; }

    public RmaFaultCategory? FaultCategory { get; set; }

    public string? FaultSubcategory { get; set; }

    public string? ReportedSymptoms { get; set; }

    public bool? IntermittentFault { get; set; }

    public bool? SafetyConcern { get; set; }

    public bool? DataLossConcern { get; set; }

    public RmaCustomerImpact? CustomerImpact { get; set; }

    public RmaYesNoUnknown? Reproducible { get; set; }

    public string? InitialDiagnosis { get; set; }

    public string? DiagnosisNotes { get; set; }

    public string? RootCause { get; set; }

    public RmaRootCauseCategory? RootCauseCategory { get; set; }

    public string? RepairActionTaken { get; set; }

    public RmaWarrantyStatus? WarrantyStatus { get; set; }

    public DateOnly? WarrantyExpiryDate { get; set; }

    public bool? ChargeableRepair { get; set; }

    public bool? CustomerApprovalRequired { get; set; }

    public bool? CustomerApprovalReceived { get; set; }

    public DateOnly? CustomerApprovalDate { get; set; }

    public string? QuoteNumber { get; set; }

    public string? PurchaseOrderNumber { get; set; }

    public string? RepairInvoiceNumber { get; set; }

    public decimal? EstimatedRepairCost { get; set; }

    public decimal? ActualRepairCost { get; set; }

    public DateOnly? DateItemReceived { get; set; }

    public string? ReceivedBy { get; set; }

    public string? AssignedTo { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateOnly? TargetCompletionDate { get; set; }

    public string? OnHoldReason { get; set; }

    public bool? EscalationRequired { get; set; }

    public string? EscalatedTo { get; set; }

    public string? EscalationNotes { get; set; }

    public DateOnly? RepairCompletedDate { get; set; }

    public string? RepairCompletedBy { get; set; }

    public bool? TestRequired { get; set; }

    public string? TestPlanUsed { get; set; }

    public RmaTestResult? TestResult { get; set; }

    public string? TestedBy { get; set; }

    public DateOnly? TestDate { get; set; }

    public string? TestNotes { get; set; }

    public bool? QaRequired { get; set; }

    public RmaQaResult? QaResult { get; set; }

    public string? QaCheckedBy { get; set; }

    public DateOnly? QaDate { get; set; }

    public bool? ReleaseApproved { get; set; }

    public string? ReleaseApprovedBy { get; set; }

    public DateTimeOffset? ReleaseApprovedAt { get; set; }

    public string? ReturnMethod { get; set; }

    public string? Courier { get; set; }

    public string? TrackingNumber { get; set; }

    public bool? CollectionArranged { get; set; }

    public DateOnly? CollectionDate { get; set; }

    public DateOnly? ShippedDate { get; set; }

    public string? ShippedBy { get; set; }

    public string? ReturnAddress { get; set; }

    public string? ShippingNotes { get; set; }

    public bool? ProofOfDeliveryReceived { get; set; }

    public DateOnly? ProofOfDeliveryDate { get; set; }

    public RmaOutcome? Outcome { get; set; }

    public string? ClosureNotes { get; set; }

    public string? CustomerFacingSummary { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RmaChecklistItem> ChecklistItems { get; } = new List<RmaChecklistItem>();

    public ICollection<RmaNote> Notes { get; } = new List<RmaNote>();

    public ICollection<RmaCommunication> Communications { get; } = new List<RmaCommunication>();

    public ICollection<RmaAttachment> Attachments { get; } = new List<RmaAttachment>();

    public ICollection<RmaPart> Parts { get; } = new List<RmaPart>();

    public ICollection<RmaStatusHistory> StatusHistoryEntries { get; } = new List<RmaStatusHistory>();

    public ICollection<RmaAudit> AuditEntries { get; } = new List<RmaAudit>();
}
