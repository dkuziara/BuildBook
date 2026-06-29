using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public interface IRmaStatusTransitionService
{
    IReadOnlyList<RmaStatus> GetAllowedNextStatuses(RmaStatus currentStatus);

    ChangeRmaStatusResult Validate(RmaStatus currentStatus, ChangeRmaStatusRequest request);
}
