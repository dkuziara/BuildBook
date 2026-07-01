using System.ComponentModel.DataAnnotations;

namespace BuildBook.Application.BuildRecords;

public sealed class CreateBuildRecordRequest
{
    public int? CustomerId { get; set; }

    public string? CustomerOrder { get; set; }

    public string? InvoiceNumber { get; set; }

    [Required(ErrorMessage = "Product code is required.")]
    public string ProductCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product name is required.")]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Serial number is required.")]
    public string SerialNumber { get; set; } = string.Empty;
}
