namespace BuildBook.Application.Rmas;

public sealed record RmaRepeatReturnRequest(
    int? CurrentRmaRecordId,
    int? LinkedBuildRecordId,
    string? SerialNumber);
