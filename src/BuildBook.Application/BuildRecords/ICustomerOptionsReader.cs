namespace BuildBook.Application.BuildRecords;

public interface ICustomerOptionsReader
{
    Task<IReadOnlyList<CustomerOption>> ListActiveAsync(
        CancellationToken cancellationToken = default);
}
