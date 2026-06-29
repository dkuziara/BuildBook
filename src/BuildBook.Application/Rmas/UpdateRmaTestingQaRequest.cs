using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaTestingQaRequest
{
    public bool? TestRequired { get; set; }

    public string? TestPlanUsed { get; set; }

    public RmaTestResult? TestResult { get; set; }

    public string? TestedBy { get; set; }

    public DateOnly? TestDate { get; set; }

    public string? TestNotes { get; set; }

    public bool? QaRequired { get; set; }

    public RmaQaResult? QaResult { get; set; }

    public string? QaCheckedBy { get; set; }

    public DateOnly? QaDate { get; set; }

    public bool? ReleaseApproved { get; set; }

    public string? ReleaseApprovedBy { get; set; }
}
