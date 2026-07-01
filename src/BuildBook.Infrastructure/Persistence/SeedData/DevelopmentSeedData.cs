using BuildBook.Application.Orders;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Domain.Settings;
using BuildBook.Application.Settings;

namespace BuildBook.Infrastructure.Persistence.SeedData;

public static class DevelopmentSeedData
{
    private static readonly DateTimeOffset SeededAt = new(2026, 6, 23, 9, 0, 0, TimeSpan.Zero);

    public static DevelopmentSeedDataSet Create()
    {
        var supportContractLevels = CreateSupportContractLevels();
        var customers = new List<Customer>
        {
            CreateCustomer(
                "Demo Site Alpha",
                supportContractLevels[2],
                CustomerSupportContractStatuses.Active,
                "Alpha-001",
                "Alex Carter"),
            CreateCustomer(
                "Demo Site Bravo",
                supportContractLevels[1],
                CustomerSupportContractStatuses.PendingRenewal,
                "Bravo-002",
                "Morgan Reed"),
            CreateCustomer(
                "Internal Validation Lab",
                null,
                CustomerSupportContractStatuses.NoContract,
                "Lab-Internal",
                "Development Team")
        };

        var systemSettings = new List<SystemSetting>
        {
            new()
            {
                Key = SystemSettingKeys.OrderWorkflowStatuses,
                Value = BuildBookOrderStatuses.SerializeDefaultWorkflow(),
                Description = "Default seeded workflow statuses used by the Orders module foundation.",
                LastUpdatedAt = SeededAt,
                LastUpdatedBy = "BuildBook development seed"
            },
            new()
            {
                Key = SystemSettingKeys.SupportTicketUrlTemplate,
                Value = "https://charthousedatamanagement.freshdesk.com/a/tickets/{1}",
                Description = "Template used to open the configured support site for a support ticket number.",
                LastUpdatedAt = SeededAt,
                LastUpdatedBy = "BuildBook development seed"
            }
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

        var rmaRecords = CreateRmaRecords(customers, records);
        var rmaChecklistItems = CreateChecklistItems(rmaRecords);
        var rmaNotes = CreateRmaNotes(rmaRecords);
        var rmaCommunications = CreateRmaCommunications(rmaRecords);
        var rmaAttachments = CreateRmaAttachments(rmaRecords);
        var rmaParts = CreateRmaParts(rmaRecords);
        var rmaStatusHistoryEntries = CreateRmaStatusHistory(rmaRecords);
        var rmaAuditEntries = CreateRmaAuditEntries(rmaRecords);

        return new DevelopmentSeedDataSet(
            supportContractLevels,
            customers,
            systemSettings,
            records,
            importBatch,
            auditEntries,
            rmaRecords,
            rmaChecklistItems,
            rmaNotes,
            rmaCommunications,
            rmaAttachments,
            rmaParts,
            rmaStatusHistoryEntries,
            rmaAuditEntries);
    }

    private static List<SupportContractLevel> CreateSupportContractLevels()
    {
        return
        [
            CreateSupportContractLevel(
                "Bronze",
                "Basic support",
                2,
                SupportResponseTimeUnit.WorkingDays,
                RmaPriority.Medium,
                10,
                1),
            CreateSupportContractLevel(
                "Silver",
                "Standard support",
                1,
                SupportResponseTimeUnit.WorkingDays,
                RmaPriority.Medium,
                20,
                2),
            CreateSupportContractLevel(
                "Gold",
                "Priority support",
                4,
                SupportResponseTimeUnit.WorkingHours,
                RmaPriority.High,
                30,
                3)
        ];
    }

    private static SupportContractLevel CreateSupportContractLevel(
        string name,
        string description,
        int responseTimeValue,
        SupportResponseTimeUnit responseTimeUnit,
        RmaPriority defaultRmaPriority,
        int priorityWeight,
        int displayOrder)
    {
        return new SupportContractLevel
        {
            Name = name,
            Description = description,
            TargetResponseTimeValue = responseTimeValue,
            TargetResponseTimeUnit = responseTimeUnit,
            DefaultRmaPriority = defaultRmaPriority,
            RmaPriorityWeight = priorityWeight,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = SeededAt,
            CreatedBy = "BuildBook development seed",
            LastUpdatedAt = SeededAt,
            LastUpdatedBy = "BuildBook development seed"
        };
    }

    private static Customer CreateCustomer(
        string name,
        SupportContractLevel? supportContractLevel,
        string supportContractStatus,
        string accountCode,
        string primaryContactName)
    {
        return new Customer
        {
            Name = name,
            AccountCode = accountCode,
            AddressLine1 = $"{name} Campus",
            TownCity = "Inverness",
            CountyRegion = "Highland",
            Postcode = "IV1 1AA",
            Country = "United Kingdom",
            MainPhone = "01463 000000",
            MainEmail = $"support@{name.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant()}.example.test",
            Website = $"https://{name.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant()}.example.test",
            PrimaryContactName = primaryContactName,
            PrimaryContactEmail = $"{primaryContactName.Replace(" ", ".", StringComparison.Ordinal).ToLowerInvariant()}@example.test",
            PrimaryContactPhone = "01463 555555",
            SupportContractLevel = supportContractLevel,
            SupportContractStatus = supportContractStatus,
            SupportContractStartDate = supportContractLevel is null ? null : new DateOnly(2026, 1, 1),
            SupportContractEndDate = supportContractLevel is null ? null : new DateOnly(2026, 12, 31),
            SupportNotes = supportContractLevel is null
                ? "Internal development customer with no external support contract."
                : $"{supportContractLevel.Name} support arrangement seeded for local testing.",
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

    private static List<RmaRecord> CreateRmaRecords(
        IReadOnlyList<Customer> customers,
        IReadOnlyList<BuildRecord> buildRecords)
    {
        return
        [
            new RmaRecord
            {
                RmaNumber = "RMA-0001",
                BuildRecord = buildRecords[0],
                Status = RmaStatus.WorkInProgress,
                Priority = RmaPriority.High,
                CreatedAt = SeededAt.AddDays(-8),
                CreatedBy = "Support Team",
                LastUpdatedAt = SeededAt.AddDays(-1),
                LastUpdatedBy = "Repair Team",
                ProductCode = buildRecords[0].ProductCode,
                ProductName = buildRecords[0].ProductName,
                SerialNumber = buildRecords[0].SerialNumber,
                Customer = customers[0],
                ContactName = "Alex Carter",
                ContactEmail = "alex.carter@example.test",
                ContactPhone = "01234 567890",
                CustomerAddress = "Demo Site Alpha, Support Office",
                CustomerReference = "RMA-ALPHA-17",
                SupportTicketNumber = "FD-10017",
                SupportTicketUrl = "https://support.example.test/tickets/FD-10017",
                OriginalOrderNumber = buildRecords[0].CustomerOrder,
                OriginalOrderDate = new DateOnly(2019, 10, 10),
                OriginalInvoiceNumber = buildRecords[0].InvoiceNumber,
                FaultSummary = "Intermittent boot failure",
                InitialFaultDescription = "Device fails to boot after power cycle and sometimes drops into BIOS storage warnings.",
                FaultDescription = "Customer reported multiple restart attempts before booting successfully.",
                FaultCategory = RmaFaultCategory.DiskStorageIssue,
                FaultSubcategory = "SATA SSD not detected",
                ReportedSymptoms = "Boot loops, BIOS warnings, slow restart recovery.",
                InitialDiagnosis = "Likely storage failure after repeated SMART alerts.",
                RootCause = "Primary SSD intermittently failing SMART health checks.",
                RootCauseCategory = RmaRootCauseCategory.DiskStorageFailure,
                RepairActionTaken = "Replaced SSD, reimaged unit, restored configuration and retested.",
                WarrantyStatus = RmaWarrantyStatus.OutOfWarranty,
                ChargeableRepair = true,
                CustomerApprovalRequired = true,
                CustomerApprovalReceived = true,
                CustomerApprovalDate = ToDateOnly(SeededAt.AddDays(-7)),
                QuoteNumber = "QT-2026-017",
                PurchaseOrderNumber = "PO-RMA-017",
                RepairInvoiceNumber = "RMA-INV-017",
                EstimatedRepairCost = 245.00m,
                ActualRepairCost = 238.50m,
                DateItemReceived = ToDateOnly(SeededAt.AddDays(-8)),
                ReceivedBy = "Support Team",
                AssignedTo = "Giles",
                DueDate = ToDateOnly(SeededAt.AddDays(2)),
                TargetCompletionDate = ToDateOnly(SeededAt.AddDays(1)),
                RepairCompletedDate = ToDateOnly(SeededAt.AddDays(-2)),
                RepairCompletedBy = "Giles",
                TestRequired = true,
                TestPlanUsed = "Radsight boot and connectivity validation",
                TestResult = RmaTestResult.Pass,
                TestedBy = "QA Team",
                TestDate = ToDateOnly(SeededAt.AddDays(-1)),
                QaRequired = true,
                QaResult = RmaQaResult.Pass,
                QaCheckedBy = "QA Team",
                QaDate = ToDateOnly(SeededAt.AddDays(-1)),
                ReleaseApproved = true,
                ReleaseApprovedBy = "Support Lead",
                ReleaseApprovedAt = SeededAt.AddDays(-1),
                ReturnMethod = "Courier",
                Courier = "DHL",
                TrackingNumber = "DHL-ALPHA-00017",
                CollectionArranged = true,
                CollectionDate = ToDateOnly(SeededAt),
                ReturnAddress = "Demo Site Alpha, Support Office",
                ShippingNotes = "Awaiting final dispatch slot.",
                IsActive = true
            },
            new RmaRecord
            {
                RmaNumber = "RMA-0002",
                Status = RmaStatus.BookedIn,
                Priority = RmaPriority.Medium,
                CreatedAt = SeededAt.AddDays(-3),
                CreatedBy = "Support Team",
                LastUpdatedAt = SeededAt.AddDays(-3),
                LastUpdatedBy = "Support Team",
                ProductCode = "LEGACY-200",
                ProductName = "Legacy Radiography Terminal",
                SerialNumber = "LEGACY-7781",
                Customer = customers[1],
                ContactName = "Morgan Reed",
                ContactEmail = "morgan.reed@example.test",
                ContactPhone = "01999 555123",
                CustomerAddress = "Demo Site Bravo, Gatehouse",
                CustomerReference = "BRAVO-RMA-04",
                SupportTicketNumber = "FD-10044",
                OriginalOrderNumber = "LEGACY-PO-7781",
                OriginalInvoiceNumber = "LEGACY-INV-7781",
                FaultSummary = "No power on arrival",
                InitialFaultDescription = "Older unit returned with no power and no matching Build Record available yet.",
                FaultDescription = "Legacy device returned from stores with no clear repair history.",
                FaultCategory = RmaFaultCategory.PowerIssue,
                WarrantyStatus = RmaWarrantyStatus.WarrantyUnknown,
                ChargeableRepair = null,
                CustomerApprovalRequired = false,
                CustomerApprovalReceived = false,
                DateItemReceived = ToDateOnly(SeededAt.AddDays(-2)),
                ReceivedBy = "Support Team",
                AssignedTo = "Unassigned",
                DueDate = ToDateOnly(SeededAt.AddDays(5)),
                IsActive = true
            }
        ];
    }

    private static List<RmaChecklistItem> CreateChecklistItems(IReadOnlyList<RmaRecord> rmaRecords)
    {
        var checklistItems = new List<RmaChecklistItem>();

        foreach (var rmaRecord in rmaRecords)
        {
            for (var index = 0; index < 4; index++)
            {
                checklistItems.Add(new RmaChecklistItem
                {
                    RmaRecord = rmaRecord,
                    DisplayOrder = index + 1,
                    Text = RmaChecklistTemplate.DefaultItems[index],
                    IsCompleted = rmaRecord.RmaNumber == "RMA-0001" && index < 3,
                    CompletedBy = rmaRecord.RmaNumber == "RMA-0001" && index < 3 ? "Giles" : null,
                    CompletedAt = rmaRecord.RmaNumber == "RMA-0001" && index < 3 ? SeededAt.AddDays(-2 + index) : null,
                    ShowInBoardView = index < 2
                });
            }
        }

        return checklistItems;
    }

    private static List<RmaNote> CreateRmaNotes(IReadOnlyList<RmaRecord> rmaRecords)
    {
        return
        [
            new RmaNote
            {
                RmaRecord = rmaRecords[0],
                NoteType = RmaNoteType.DiagnosisNote,
                NoteText = "SMART warnings reproduced after extended power-cycle testing.",
                CreatedBy = "Giles",
                CreatedAt = SeededAt.AddDays(-4)
            },
            new RmaNote
            {
                RmaRecord = rmaRecords[1],
                NoteType = RmaNoteType.InternalNote,
                NoteText = "Legacy device booked in without a matching Build Record. Continue manual intake.",
                CreatedBy = "Support Team",
                CreatedAt = SeededAt.AddDays(-3)
            }
        ];
    }

    private static List<RmaCommunication> CreateRmaCommunications(IReadOnlyList<RmaRecord> rmaRecords)
    {
        return
        [
            new RmaCommunication
            {
                RmaRecord = rmaRecords[0],
                CommunicationDate = SeededAt.AddDays(-7),
                ContactMethod = "Email",
                ContactPerson = "Alex Carter",
                Summary = "Customer approved the quoted SSD replacement and requested courier return.",
                FollowUpRequired = false,
                CreatedBy = "Support Team",
                CreatedAt = SeededAt.AddDays(-7)
            }
        ];
    }

    private static List<RmaAttachment> CreateRmaAttachments(IReadOnlyList<RmaRecord> rmaRecords)
    {
        return
        [
            new RmaAttachment
            {
                RmaRecord = rmaRecords[0],
                FileName = "boot-diagnostics.png",
                StoredFilePath = "development-seed/rma-0001/boot-diagnostics.png",
                ContentType = "image/png",
                AttachmentType = "Screenshot",
                Description = "Boot diagnostics screenshot captured during intake.",
                UploadedBy = "Giles",
                UploadedAt = SeededAt.AddDays(-5)
            }
        ];
    }

    private static List<RmaPart> CreateRmaParts(IReadOnlyList<RmaRecord> rmaRecords)
    {
        return
        [
            new RmaPart
            {
                RmaRecord = rmaRecords[0],
                PartName = "Industrial SSD",
                PartNumber = "SSD-512-IND",
                Quantity = 1,
                Supplier = "Demo Components",
                UnitCost = 118.50m,
                Notes = "Replacement drive fitted during repair."
            }
        ];
    }

    private static List<RmaStatusHistory> CreateRmaStatusHistory(IReadOnlyList<RmaRecord> rmaRecords)
    {
        return
        [
            new RmaStatusHistory
            {
                RmaRecord = rmaRecords[0],
                OldStatus = null,
                NewStatus = RmaStatus.BookedIn,
                ChangedBy = "Support Team",
                ChangedAt = SeededAt.AddDays(-8),
                Reason = "RMA created."
            },
            new RmaStatusHistory
            {
                RmaRecord = rmaRecords[0],
                OldStatus = RmaStatus.BookedIn,
                NewStatus = RmaStatus.WorkInProgress,
                ChangedBy = "Giles",
                ChangedAt = SeededAt.AddDays(-6),
                Reason = "Repair work started."
            },
            new RmaStatusHistory
            {
                RmaRecord = rmaRecords[1],
                OldStatus = null,
                NewStatus = RmaStatus.BookedIn,
                ChangedBy = "Support Team",
                ChangedAt = SeededAt.AddDays(-3),
                Reason = "Legacy RMA created."
            }
        ];
    }

    private static List<RmaAudit> CreateRmaAuditEntries(IReadOnlyList<RmaRecord> rmaRecords)
    {
        return
        [
            new RmaAudit
            {
                RmaRecord = rmaRecords[0],
                OccurredAt = SeededAt.AddDays(-8),
                User = "Support Team",
                Action = "Created",
                NewValue = "Development seed RMA created."
            },
            new RmaAudit
            {
                RmaRecord = rmaRecords[0],
                OccurredAt = SeededAt.AddDays(-6),
                User = "Giles",
                Action = "Status changed",
                FieldChanged = "Status",
                OldValue = RmaStatus.BookedIn.ToString(),
                NewValue = RmaStatus.WorkInProgress.ToString()
            },
            new RmaAudit
            {
                RmaRecord = rmaRecords[1],
                OccurredAt = SeededAt.AddDays(-3),
                User = "Support Team",
                Action = "Created",
                NewValue = "Development seed RMA created."
            }
        ];
    }

    private static DateOnly ToDateOnly(DateTimeOffset value)
    {
        return DateOnly.FromDateTime(value.Date);
    }
}
