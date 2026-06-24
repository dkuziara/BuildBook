namespace BuildBook.Application.BuildRecords;

public sealed record BuildRecordDetailModel(
    int Id,
    string ProductCode,
    string ProductName,
    string? ProductClassification,
    string SerialNumber,
    string? InternalStatus,
    string? CustomerName,
    string? MachineName,
    string? RadSightVersion,
    string? WindowsVersion,
    DateOnly? DateShipped,
    DateTimeOffset LastUpdatedAt,
    string LastUpdatedBy);
