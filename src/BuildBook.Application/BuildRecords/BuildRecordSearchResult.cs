namespace BuildBook.Application.BuildRecords;

public sealed record BuildRecordSearchResult(
    int Id,
    string ProductCode,
    string ProductName,
    string SerialNumber,
    string? CustomerName,
    string? MachineName,
    string? RadSightVersion,
    string? WindowsVersion,
    DateOnly? DateShipped);
