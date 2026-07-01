namespace BuildBook.Application.Rmas;

public interface IRmaRegisterReader
{
    Task<IReadOnlyList<RmaRegisterRow>> ListAsync(
        RmaRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
