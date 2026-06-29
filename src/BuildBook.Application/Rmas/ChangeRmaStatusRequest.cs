using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class ChangeRmaStatusRequest
{
    public RmaStatus NewStatus { get; set; }

    public string? Reason { get; set; }

    public string? OnHoldReason { get; set; }

    public RmaOutcome? Outcome { get; set; }

    public string? ClosureNotes { get; set; }
}
