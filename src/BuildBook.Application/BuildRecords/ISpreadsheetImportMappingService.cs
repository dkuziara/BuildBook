namespace BuildBook.Application.BuildRecords;

public interface ISpreadsheetImportMappingService
{
    Task<SpreadsheetColumnMappingReview> BuildReviewAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);
}
