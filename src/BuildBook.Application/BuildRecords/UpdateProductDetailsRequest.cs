using BuildBook.Domain.BuildRecords;

namespace BuildBook.Application.BuildRecords;

public sealed class UpdateProductDetailsRequest
{
    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string? ProductClassification { get; set; }

    public string SerialNumber { get; set; } = string.Empty;

    public InternalStatus? InternalStatus { get; set; }
}
