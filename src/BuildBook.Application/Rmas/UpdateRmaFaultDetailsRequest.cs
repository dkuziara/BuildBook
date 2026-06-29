using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaFaultDetailsRequest
{
    public string FaultSummary { get; set; } = string.Empty;

    public string? FaultDescription { get; set; }

    public string? ReportedSymptoms { get; set; }

    public RmaFaultCategory? FaultCategory { get; set; }

    public string? FaultSubcategory { get; set; }

    public bool? IntermittentFault { get; set; }

    public bool? SafetyConcern { get; set; }

    public bool? DataLossConcern { get; set; }

    public RmaCustomerImpact? CustomerImpact { get; set; }

    public RmaYesNoUnknown? Reproducible { get; set; }

    public string? InitialDiagnosis { get; set; }
}
