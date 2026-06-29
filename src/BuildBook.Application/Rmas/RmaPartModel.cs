namespace BuildBook.Application.Rmas;

public sealed record RmaPartModel(
    int Id,
    string PartName,
    string? PartNumber,
    int Quantity,
    string? SerialNumber,
    string? Supplier,
    decimal? UnitCost,
    string? Notes);
