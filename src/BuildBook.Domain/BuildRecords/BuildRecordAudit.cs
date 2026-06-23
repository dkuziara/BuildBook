namespace BuildBook.Domain.BuildRecords;

public sealed class BuildRecordAudit
{
    public int Id { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public string User { get; set; } = string.Empty;

    public int? BuildRecordId { get; set; }

    public BuildRecord? BuildRecord { get; set; }

    public AuditAction Action { get; set; }

    public string? FieldChanged { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ImportBatchId { get; set; }

    public ImportBatch? ImportBatch { get; set; }
}
