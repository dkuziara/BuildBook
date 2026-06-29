namespace BuildBook.Application.Rmas;

public sealed record RmaCreatePrefillModel(
    int BuildRecordId,
    string ProductCode,
    string ProductName,
    string SerialNumber,
    string? CustomerName);
