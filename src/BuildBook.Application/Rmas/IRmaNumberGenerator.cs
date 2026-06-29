namespace BuildBook.Application.Rmas;

public interface IRmaNumberGenerator
{
    Task<string> GenerateNextAsync(CancellationToken cancellationToken = default);
}
