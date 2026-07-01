using BuildBook.Domain.Security;

namespace BuildBook.Domain.Orders;

public sealed class OrderImportBatch
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? PlanId { get; set; }

    public string? PlanName { get; set; }

    public DateTimeOffset? ExportDate { get; set; }

    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? ImportedByUserId { get; set; }

    public ApplicationUser? ImportedByUser { get; set; }

    public int RowsRead { get; set; }

    public int OrdersCreated { get; set; }

    public int OrdersUpdated { get; set; }

    public int OrdersSkipped { get; set; }

    public int Warnings { get; set; }

    public int Errors { get; set; }

    public ICollection<OrderImportWarning> WarningEntries { get; } = new List<OrderImportWarning>();
}
