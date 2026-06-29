namespace BuildBook.Application.Rmas;

public sealed class SaveRmaPartRequest
{
    public int? PartId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string? PartNumber { get; set; }

    public int Quantity { get; set; } = 1;

    public string? SerialNumber { get; set; }

    public string? Supplier { get; set; }

    public decimal? UnitCost { get; set; }

    public string? Notes { get; set; }
}
