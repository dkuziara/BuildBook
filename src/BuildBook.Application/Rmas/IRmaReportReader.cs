namespace BuildBook.Application.Rmas;

public interface IRmaReportReader
{
    Task<IReadOnlyList<RmaReportRow>> ListAsync(
        RmaReportFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task<RmaDurationMetrics?> GetMetricsAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default);
}
