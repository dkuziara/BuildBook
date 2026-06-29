using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Rmas;

namespace BuildBook.Tests;

public class RmaStatusTransitionServiceTests
{
    [Fact]
    public void GetAllowedNextStatuses_ForWorkInProgress_ReturnsExpectedWorkflow()
    {
        var service = new RmaStatusTransitionService();

        var statuses = service.GetAllowedNextStatuses(RmaStatus.WorkInProgress);

        Assert.Equal(
        [
            RmaStatus.OnHold,
            RmaStatus.ReadyToShip,
            RmaStatus.CancelledNoReply,
            RmaStatus.CustomerFixed
        ], statuses);
    }

    [Fact]
    public void Validate_RequiresReasonWhenPuttingRmaOnHold()
    {
        var service = new RmaStatusTransitionService();

        var result = service.Validate(
            RmaStatus.WorkInProgress,
            new ChangeRmaStatusRequest
            {
                NewStatus = RmaStatus.OnHold,
                OnHoldReason = RmaOnHoldReasons.WaitingForParts
            });

        Assert.False(result.Succeeded);
        Assert.Contains("Add on-hold notes", result.Errors.Single());
    }

    [Fact]
    public void Validate_RequiresOutcomeAndClosureNotesWhenClosing()
    {
        var service = new RmaStatusTransitionService();

        var missingOutcome = service.Validate(
            RmaStatus.Shipped,
            new ChangeRmaStatusRequest
            {
                NewStatus = RmaStatus.Closed,
                ClosureNotes = "Returned to customer."
            });

        var missingNotes = service.Validate(
            RmaStatus.Shipped,
            new ChangeRmaStatusRequest
            {
                NewStatus = RmaStatus.Closed,
                Outcome = RmaOutcome.RepairedAndReturned
            });

        Assert.False(missingOutcome.Succeeded);
        Assert.False(missingNotes.Succeeded);
    }

    [Fact]
    public void Validate_RejectsInvalidTransition()
    {
        var service = new RmaStatusTransitionService();

        var result = service.Validate(
            RmaStatus.BookedIn,
            new ChangeRmaStatusRequest
            {
                NewStatus = RmaStatus.Closed,
                Outcome = RmaOutcome.Cancelled,
                ClosureNotes = "Skipped ahead."
            });

        Assert.False(result.Succeeded);
        Assert.Contains("cannot change", result.Errors.Single(), StringComparison.OrdinalIgnoreCase);
    }
}
