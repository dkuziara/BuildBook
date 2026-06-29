using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public sealed class RmaStatusTransitionService : IRmaStatusTransitionService
{
    public IReadOnlyList<RmaStatus> GetAllowedNextStatuses(RmaStatus currentStatus)
    {
        return currentStatus switch
        {
            RmaStatus.BookedIn =>
            [
                RmaStatus.WorkInProgress,
                RmaStatus.OnHold,
                RmaStatus.CancelledNoReply,
                RmaStatus.CustomerFixed
            ],
            RmaStatus.WorkInProgress =>
            [
                RmaStatus.OnHold,
                RmaStatus.ReadyToShip,
                RmaStatus.CancelledNoReply,
                RmaStatus.CustomerFixed
            ],
            RmaStatus.ReadyToShip =>
            [
                RmaStatus.Shipped,
                RmaStatus.OnHold,
                RmaStatus.WorkInProgress,
                RmaStatus.CancelledNoReply,
                RmaStatus.CustomerFixed
            ],
            RmaStatus.OnHold =>
            [
                RmaStatus.WorkInProgress,
                RmaStatus.ReadyToShip,
                RmaStatus.CancelledNoReply,
                RmaStatus.CustomerFixed
            ],
            RmaStatus.Shipped => [RmaStatus.Closed],
            RmaStatus.CustomerFixed => [RmaStatus.Closed],
            RmaStatus.CancelledNoReply => [RmaStatus.Closed],
            _ => []
        };
    }

    public ChangeRmaStatusResult Validate(RmaStatus currentStatus, ChangeRmaStatusRequest request)
    {
        if (request.NewStatus == currentStatus)
        {
            return ChangeRmaStatusResult.Failure("Select a different status to apply a status change.");
        }

        if (!GetAllowedNextStatuses(currentStatus).Contains(request.NewStatus))
        {
            return ChangeRmaStatusResult.Failure($"The status cannot change from {FormatStatus(currentStatus)} to {FormatStatus(request.NewStatus)}.");
        }

        if (request.NewStatus == RmaStatus.OnHold)
        {
            if (string.IsNullOrWhiteSpace(request.OnHoldReason))
            {
                return ChangeRmaStatusResult.Failure("Select an on-hold reason.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return ChangeRmaStatusResult.Failure("Add on-hold notes so the blocker is clear.");
            }
        }

        if ((request.NewStatus == RmaStatus.CancelledNoReply || request.NewStatus == RmaStatus.CustomerFixed)
            && string.IsNullOrWhiteSpace(request.Reason))
        {
            return ChangeRmaStatusResult.Failure("Add a note explaining this status change.");
        }

        if (request.NewStatus == RmaStatus.Closed)
        {
            if (request.Outcome is null)
            {
                return ChangeRmaStatusResult.Failure("Select an outcome before closing this RMA.");
            }

            if (string.IsNullOrWhiteSpace(request.ClosureNotes))
            {
                return ChangeRmaStatusResult.Failure("Add closure notes before closing this RMA.");
            }
        }

        return ChangeRmaStatusResult.Success();
    }

    private static string FormatStatus(RmaStatus status)
    {
        return status switch
        {
            RmaStatus.BookedIn => "Booked In",
            RmaStatus.WorkInProgress => "Work In Progress",
            RmaStatus.ReadyToShip => "Ready To Ship",
            RmaStatus.CancelledNoReply => "Cancelled / No Reply",
            RmaStatus.CustomerFixed => "Customer Fixed",
            _ => status.ToString()
        };
    }
}
