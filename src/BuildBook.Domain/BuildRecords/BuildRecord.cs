using BuildBook.Domain.Customers;

namespace BuildBook.Domain.BuildRecords;

public sealed class BuildRecord
{
    public int Id { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string? ProductClassification { get; set; }

    public string SerialNumber { get; set; } = string.Empty;

    public InternalStatus? InternalStatus { get; set; }

    public string? AssembledIn { get; set; }

    public string? AssembledBy { get; set; }

    public DateOnly? DateAssembled { get; set; }

    public string? HardwareManufacturer { get; set; }

    public string? ManufacturerPartNumber { get; set; }

    public string? ManufacturerRevision { get; set; }

    public string? ManufacturerSerialNumber { get; set; }

    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string? CustomerOrder { get; set; }

    public string? OANumber { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateOnly? DateShipped { get; set; }

    public string? ShippingNotes { get; set; }

    public string? PanelDeviceModel { get; set; }

    public string? PanelDeviceSerial { get; set; }

    public string? PanelFirmwareVersion { get; set; }

    public string? MachineName { get; set; }

    public string? RadioSerialNumber { get; set; }

    public string? RouterUsed { get; set; }

    public string? HardwareNotes { get; set; }

    public string? DiskImageVersion { get; set; }

    public string? RadSightVersion { get; set; }

    public string? WindowsVersion { get; set; }

    public string? WindowsLatestPatch { get; set; }

    public string? BleuvioFirmwareVersion { get; set; }

    public string? CharthouseIrdaFirmwareVersion { get; set; }

    public string? RadioFirmware { get; set; }

    public string? RadSightUserLogin { get; set; }

    public string? KioskUser { get; set; }

    public string? WindowsAdminUser { get; set; }

    public string? WifiSsid { get; set; }

    public string? PackingList { get; set; }

    public string? CheckedBy { get; set; }

    public string? Note { get; set; }

    public int? OriginalSpreadsheetRowNumber { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<BuildRecordSecret> Secrets { get; } = new List<BuildRecordSecret>();

    public ICollection<BuildRecordAudit> AuditEntries { get; } = new List<BuildRecordAudit>();
}
