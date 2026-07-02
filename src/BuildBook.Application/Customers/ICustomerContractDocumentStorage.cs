namespace BuildBook.Application.Customers;

public interface ICustomerContractDocumentStorage
{
    Task<string> SaveAsync(
        int customerId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<byte[]?> ReadAsync(
        string storedFilePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storedFilePath,
        CancellationToken cancellationToken = default);
}
