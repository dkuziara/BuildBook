using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaWarrantyCommercialRequest
{
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
}
