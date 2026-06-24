namespace BuildBook.Application.BuildRecords;

public sealed class UpdateHardwareDetailsRequest
{
    public string? PanelDeviceModel { get; set; }

    public string? PanelDeviceSerial { get; set; }

    public string? PanelFirmwareVersion { get; set; }

    public string? MachineName { get; set; }

    public string? RadioSerialNumber { get; set; }

    public string? HardwareNotes { get; set; }
}
