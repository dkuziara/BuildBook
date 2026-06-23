namespace BuildBook.Domain.BuildRecords;

public sealed class BuildRecordSecret
{
    public int Id { get; set; }

    public int BuildRecordId { get; set; }

    public BuildRecord? BuildRecord { get; set; }

    public SecretType SecretType { get; set; }

    public byte[] SecretValueEncrypted { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = string.Empty;
}
