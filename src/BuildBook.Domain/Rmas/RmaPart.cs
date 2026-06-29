namespace BuildBook.Domain.Rmas;

public sealed class RmaPart
{
    public int Id { get; set; }

    public int RmaRecordId { get; set; }

    public RmaRecord? RmaRecord { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string? PartNumber { get; set; }

    public int Quantity { get; set; } = 1;

    public string? SerialNumber { get; set; }

    public string? Supplier { get; set; }

    public decimal? UnitCost { get; set; }

    public string? Notes { get; set; }
}
