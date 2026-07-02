namespace BuildBook.Application.BuildRecords;

public sealed record BuildRegisterRow(
    int Id,
    string ProductCode,
    int? LinkedProductId,
    string ProductName,
    string SerialNumber,
    string? CustomerName,
    string? MachineName,
    string? RadSightVersion,
    string? WindowsVersion,
    DateOnly? DateAssembled,
    DateOnly? DateShipped,
    string? CheckedBy,
    DateTimeOffset LastUpdatedAt);
