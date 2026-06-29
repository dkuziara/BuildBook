using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed record RmaRegisterRow(
    int Id,
    string RmaNumber,
    RmaStatus Status,
    string? CustomerName,
    string ProductName,
    string? SerialNumber,
    string FaultSummary,
    RmaPriority? Priority,
    string? AssignedTo,
    DateOnly? DueDate,
    bool HasLinkedBuildRecord,
    int? LinkedBuildRecordId,
    DateTimeOffset LastUpdatedAt);
