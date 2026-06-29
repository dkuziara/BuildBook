using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed record RmaBoardCardModel(
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
    bool IsOverdue,
    int CompletedChecklistCount,
    int TotalChecklistCount,
    int PreviousRmaCount,
    IReadOnlyList<string> Warnings);
