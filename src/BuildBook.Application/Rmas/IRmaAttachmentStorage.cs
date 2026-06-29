namespace BuildBook.Application.Rmas;

public interface IRmaAttachmentStorage
{
    Task<string> SaveAsync(
        int rmaRecordId,
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
