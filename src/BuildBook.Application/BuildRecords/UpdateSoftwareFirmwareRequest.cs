namespace BuildBook.Application.BuildRecords;

public sealed class UpdateSoftwareFirmwareRequest
{
    public string? DiskImageVersion { get; set; }

    public string? RadSightVersion { get; set; }

    public string? WindowsVersion { get; set; }

    public string? WindowsLatestPatch { get; set; }

    public string? BleuvioFirmwareVersion { get; set; }

    public string? CharthouseIrdaFirmwareVersion { get; set; }

    public string? RadioFirmware { get; set; }
}
