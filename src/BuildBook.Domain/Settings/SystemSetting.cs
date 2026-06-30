namespace BuildBook.Domain.Settings;

public sealed class SystemSetting
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = string.Empty;
}
