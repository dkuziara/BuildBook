namespace BuildBook.Application.BuildRecords;

public sealed record MissingDataReportRow(
    int Id,
    string ProductCode,
    string ProductName,
    string SerialNumber,
    string? CustomerName,
    string? MachineName,
    string? RadSightVersion,
    string? WindowsVersion,
    DateOnly? DateShipped,
    DateTimeOffset LastUpdatedAt,
    bool IsMissingCustomer,
    bool IsMissingRecoveryData,
    bool IsMissingDateShipped);
