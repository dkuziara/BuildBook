using BuildBook.Application.Rmas;
using Microsoft.Extensions.Configuration;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public sealed class LocalRmaAttachmentStorage : IRmaAttachmentStorage
{
    private readonly string rootDirectory;

    public LocalRmaAttachmentStorage(IConfiguration configuration)
    {
        rootDirectory = configuration["BuildBook:RmaAttachmentStorageDirectory"]
            ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "RmaAttachments");
    }

    public async Task<string> SaveAsync(
        int rmaRecordId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var safeFileName = SanitizeFileName(fileName);
        var recordDirectoryName = $"rma-{rmaRecordId:D6}";
        var relativePath = Path.Combine(recordDirectoryName, $"{Guid.NewGuid():N}-{safeFileName}");
        var fullPath = ResolveFullPath(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(fileStream, cancellationToken);
        return relativePath.Replace('\\', '/');
    }

    public async Task<byte[]?> ReadAsync(
        string storedFilePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(storedFilePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task DeleteAsync(
        string storedFilePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(storedFilePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveFullPath(string storedFilePath)
    {
        var rootFullPath = Path.GetFullPath(rootDirectory);
        var candidatePath = Path.GetFullPath(Path.Combine(rootFullPath, storedFilePath.Replace('/', Path.DirectorySeparatorChar)));

        if (!candidatePath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Attachment path was outside the configured storage root.");
        }

        return candidatePath;
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalidCharacter, '_');
        }

        return string.IsNullOrWhiteSpace(name) ? "attachment.bin" : name;
    }
}
