namespace BuildBook.Application.BuildRecords;

public sealed class UpdateBuildDetailsRequest
{
    public string? AssembledIn { get; set; }

    public string? AssembledBy { get; set; }

    public DateOnly? DateAssembled { get; set; }

    public string? HardwareManufacturer { get; set; }

    public string? ManufacturerPartNumber { get; set; }

    public string? ManufacturerRevision { get; set; }

    public string? ManufacturerSerialNumber { get; set; }

    public string? PackingList { get; set; }

    public string? CheckedBy { get; set; }
}
