namespace BuildBook.Application.BuildRecords;

public interface ISpreadsheetImportMappingService
{
    Task<SpreadsheetColumnMappingReview> BuildReviewAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    Task<SpreadsheetImportPreview> BuildPreviewAsync(
        string fileName,
        Stream fileStream,
        IReadOnlyDictionary<string, string> selectedMappings,
        CancellationToken cancellationToken = default);

    Task<SpreadsheetImportValidationResult> BuildValidationAsync(
        string fileName,
        Stream fileStream,
        IReadOnlyDictionary<string, string> selectedMappings,
        CancellationToken cancellationToken = default);
}
