namespace BuildBook.Application.Rmas;

public interface IRmaRecordService
{
    Task<CreateRmaResult> CreateAsync(
        CreateRmaRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<RmaDetailModel?> GetByIdAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default);

    Task<RmaDashboardSummary> GetDashboardSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RmaStatusHistoryEntry>> GetStatusHistoryAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RmaChecklistItemModel>> GetChecklistAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RmaPartModel>> GetPartsAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RmaRegisterRow>> SearchAsync(
        RmaRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RmaBuildRecordMatchSuggestion>> SuggestBuildRecordMatchesAsync(
        RmaBuildRecordMatchRequest request,
        CancellationToken cancellationToken = default);

    Task<UpdateRmaIntakeResult> UpdateIntakeAsync(
        int rmaRecordId,
        UpdateRmaIntakeRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<UpdateRmaFaultDetailsResult> UpdateFaultDetailsAsync(
        int rmaRecordId,
        UpdateRmaFaultDetailsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<UpdateRmaRepairDetailsResult> UpdateRepairDetailsAsync(
        int rmaRecordId,
        UpdateRmaRepairDetailsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<UpdateRmaWorkflowResult> UpdateWorkflowAsync(
        int rmaRecordId,
        UpdateRmaWorkflowRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<UpdateRmaTestingQaResult> UpdateTestingQaAsync(
        int rmaRecordId,
        UpdateRmaTestingQaRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<SaveRmaPartResult> SavePartAsync(
        int rmaRecordId,
        SaveRmaPartRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<SaveRmaPartResult> DeletePartAsync(
        int rmaRecordId,
        int partId,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<UpdateRmaChecklistResult> UpdateChecklistItemAsync(
        int rmaRecordId,
        UpdateRmaChecklistItemRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<ChangeRmaStatusResult> ChangeStatusAsync(
        int rmaRecordId,
        ChangeRmaStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<RmaLinkResult> LinkBuildRecordAsync(
        int rmaRecordId,
        int buildRecordId,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<RmaLinkResult> UnlinkBuildRecordAsync(
        int rmaRecordId,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
