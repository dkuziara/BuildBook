namespace BuildBook.Application.BuildRecords;

public sealed record HomePageRecordSummary(
    int Id,
    string ProductCode,
    string ProductName,
    string SerialNumber,
    string? CustomerName,
    string? MachineName,
    DateTimeOffset ActivityAt);
