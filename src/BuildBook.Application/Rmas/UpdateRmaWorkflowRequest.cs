using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaWorkflowRequest
{
    public string? AssignedTo { get; set; }

    public RmaPriority? Priority { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateOnly? TargetCompletionDate { get; set; }
}
