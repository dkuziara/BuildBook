using System.ComponentModel.DataAnnotations;

namespace BuildBook.Application.Rmas;

public sealed class CreateRmaRequest
{
    [Required(ErrorMessage = "Customer is required.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product name is required.")]
    public string ProductName { get; set; } = string.Empty;

    public string? ProductCode { get; set; }

    public string? SerialNumber { get; set; }

    [Required(ErrorMessage = "Fault summary is required.")]
    public string FaultSummary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fault description is required.")]
    public string InitialFaultDescription { get; set; } = string.Empty;

    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? SupportTicketNumber { get; set; }

    public string? OriginalOrderNumber { get; set; }

    public string? OriginalInvoiceNumber { get; set; }

    public string? MigrationSource { get; set; }

    public string? OriginalPlannerTaskTitle { get; set; }

    public string? OriginalPlannerNotes { get; set; }

    public int? LinkedBuildRecordId { get; set; }
}
