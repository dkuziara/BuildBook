using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaRepairDetailsRequest
{
    public string? DiagnosisNotes { get; set; }

    public string? RootCause { get; set; }

    public RmaRootCauseCategory? RootCauseCategory { get; set; }

    public string? RepairActionTaken { get; set; }

    public DateOnly? RepairCompletedDate { get; set; }

    public string? RepairCompletedBy { get; set; }
}
