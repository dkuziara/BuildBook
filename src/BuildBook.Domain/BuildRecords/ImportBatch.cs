namespace BuildBook.Domain.BuildRecords;

public sealed class ImportBatch
{
    public int Id { get; set; }

    public string SourceFileName { get; set; } = string.Empty;

    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;

    public string ImportedBy { get; set; } = string.Empty;

    public ImportStatus Status { get; set; } = ImportStatus.Pending;

    public int RowsRead { get; set; }

    public int RecordsCreated { get; set; }

    public int RecordsSkipped { get; set; }

    public int WarningCount { get; set; }

    public int ErrorCount { get; set; }

    public string? Summary { get; set; }

    public ICollection<BuildRecordAudit> AuditEntries { get; } = new List<BuildRecordAudit>();
}
