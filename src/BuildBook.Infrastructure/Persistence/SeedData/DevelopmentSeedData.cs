using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;

namespace BuildBook.Infrastructure.Persistence.SeedData;

public static class DevelopmentSeedData
{
    private static readonly DateTimeOffset SeededAt = new(2026, 6, 23, 9, 0, 0, TimeSpan.Zero);

    public static DevelopmentSeedDataSet Create()
    {
        var customers = new List<Customer>
        {
            CreateCustomer("Demo Site Alpha"),
            CreateCustomer("Demo Site Bravo"),
            CreateCustomer("Internal Validation Lab")
        };

        var importBatch = new ImportBatch
        {
            SourceFileName = "development-seed-data.xlsx",
            ImportedAt = SeededAt,
            ImportedBy = "BuildBook development seed",
            Status = ImportStatus.Completed,
            RowsRead = 3,
            RecordsCreated = 3,
            RecordsSkipped = 0,
            WarningCount = 0,
            ErrorCount = 0,
            Summary = "Development-only seed data for local BuildBook testing."
        };

        var records = new List<BuildRecord>
        {
            CreateRadSightAccessTerminal(
                customers[0],
                serialNumber: "DEMO-1000",
                machineName: "DEMO-RAD-1000",
                customerOrder: "PO-DEMO-ALPHA-001",
                oaNumber: "OA-DEMO-001",
                invoiceNumber: "INV-DEMO-001",
                radSightVersion: "1.3.6-demo",
                windowsVersion: "Windows 10",
                dateAssembled: new DateOnly(2019, 10, 3),
                dateShipped: new DateOnly(2019, 10, 17),
                checkedBy: "QA Team"),
            CreateRadSightAccessTerminal(
                customers[1],
                serialNumber: "DEMO-1001",
                machineName: "DEMO-RAD-1001",
                customerOrder: "PO-DEMO-BRAVO-002",
                oaNumber: "OA-DEMO-002",
                invoiceNumber: "INV-DEMO-002",
                radSightVersion: "1.3.7-demo",
                windowsVersion: "Windows 10",
                dateAssembled: new DateOnly(2020, 2, 11),
                dateShipped: new DateOnly(2020, 2, 24),
                checkedBy: "QA Team"),
            CreateRadSightAccessTerminal(
                customers[2],
                serialNumber: "DEMO-DEV-0001",
                machineName: "DEMO-LAB-01",
                customerOrder: "INTERNAL-DEMO-001",
                oaNumber: "OA-DEMO-LAB",
                invoiceNumber: "INV-DEMO-LAB",
                radSightVersion: "2.0.0-dev",
                windowsVersion: "Windows 11",
                dateAssembled: new DateOnly(2026, 6, 20),
                dateShipped: null,
                checkedBy: "Development")
        };

        var auditEntries = records
            .Select(record => new BuildRecordAudit
            {
                BuildRecord = record,
                ImportBatch = importBatch,
                OccurredAt = SeededAt,
                User = "BuildBook development seed",
                Action = AuditAction.ImportPerformed,
                FieldChanged = null,
                OldValue = null,
                NewValue = "Development seed record created."
            })
            .ToList();

        return new DevelopmentSeedDataSet(customers, records, importBatch, auditEntries);
    }

    private static Customer CreateCustomer(string name)
    {
        return new Customer
        {
            Name = name,
            CreatedAt = SeededAt,
            CreatedBy = "BuildBook development seed",
            LastUpdatedAt = SeededAt,
            LastUpdatedBy = "BuildBook development seed"
        };
    }

    private static BuildRecord CreateRadSightAccessTerminal(
        Customer customer,
        string serialNumber,
        string machineName,
        string customerOrder,
        string oaNumber,
        string invoiceNumber,
        string radSightVersion,
        string windowsVersion,
        DateOnly dateAssembled,
        DateOnly? dateShipped,
        string checkedBy)
    {
        return new BuildRecord
        {
            ProductCode = "CDM61100",
            ProductName = "RadSight Access Terminal",
            ProductClassification = "RadSight",
            SerialNumber = serialNumber,
            InternalStatus = dateShipped.HasValue ? InternalStatus.Shipped : InternalStatus.Checked,
            AssembledIn = "Production",
            AssembledBy = "Assembly Team",
            DateAssembled = dateAssembled,
            HardwareManufacturer = "Charthouse",
            ManufacturerPartNumber = "CDM61100",
            ManufacturerRevision = "Rev A",
            ManufacturerSerialNumber = $"MFG-{serialNumber}",
            Customer = customer,
            CustomerOrder = customerOrder,
            OANumber = oaNumber,
            InvoiceNumber = invoiceNumber,
            DateShipped = dateShipped,
            ShippingNotes = dateShipped.HasValue ? "Seeded shipped device." : "Seeded internal development device.",
            PanelDeviceModel = "D10 Panel PC",
            PanelDeviceSerial = $"D10A00{serialNumber}",
            PanelFirmwareVersion = "1.0.0",
            MachineName = machineName,
            RadioSerialNumber = $"RAD-{serialNumber}",
            RouterUsed = "Seed Router",
            HardwareNotes = "Development seed hardware record.",
            DiskImageVersion = "BuildBook-Seed-Image",
            RadSightVersion = radSightVersion,
            WindowsVersion = windowsVersion,
            WindowsLatestPatch = "To Be Confirmed",
            BleuvioFirmwareVersion = "4.1.0",
            CharthouseIrdaFirmwareVersion = "2.3.0",
            RadioFirmware = "1.8.2",
            RadSightUserLogin = "demo-operator",
            KioskUser = "demo-kiosk",
            WindowsAdminUser = "demo-admin",
            WifiSsid = "BuildBook-Demo",
            PackingList = $"PL-{serialNumber}",
            CheckedBy = checkedBy,
            Note = "Synthetic development seed data for local testing. Contains no real customer information or credentials.",
            OriginalSpreadsheetRowNumber = null,
            CreatedAt = SeededAt,
            CreatedBy = "BuildBook development seed",
            LastUpdatedAt = SeededAt,
            LastUpdatedBy = "BuildBook development seed",
            IsActive = true
        };
    }
}
