using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Security;

namespace BuildBook.Domain.Orders;

public sealed class OrderBuildRecordLink
{
    public int Id { get; set; }

    public int OrderRecordId { get; set; }

    public OrderRecord? OrderRecord { get; set; }

    public int BuildRecordId { get; set; }

    public BuildRecord? BuildRecord { get; set; }

    public string? LinkType { get; set; }

    public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? LinkedByUserId { get; set; }

    public ApplicationUser? LinkedByUser { get; set; }
}
