using BuildBook.Application.Rmas;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BuildBook.Tests;

public class RmaRecordServiceIntegrationTests
{
    [Fact]
    public async Task GenerateNextAsync_ReturnsNextSequentialRmaNumber()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaNumberGenerator");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                setupContext.RmaRecords.AddRange(
                    CreateRmaRecord("RMA-0007", "Device A", "SN-0007"),
                    CreateRmaRecord("Legacy-0012", "Device B", "SN-0012"));
                await setupContext.SaveChangesAsync();
            }

            var generator = new RmaNumberGenerator(new TestDbContextFactory(options));

            var nextRmaNumber = await generator.GenerateNextAsync();

            Assert.Equal("RMA-0013", nextRmaNumber);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task CreateAsync_PersistsLinkedRmaChecklistStatusHistoryAndAudit()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaCreate");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            var linkedBuildRecordId = await SeedBuildRecordAsync(
                options,
                "Acme Medical",
                "CDM61100",
                "RadSight Access Terminal",
                "SN-1000");

            var service = CreateService(options);

            var result = await service.CreateAsync(
                new CreateRmaRequest
                {
                    CustomerName = "  Acme Medical  ",
                    ProductName = "  RadSight Access Terminal  ",
                    ProductCode = "  CDM61100  ",
                    SerialNumber = "  SN-1000  ",
                    FaultSummary = "  Does not boot  ",
                    InitialFaultDescription = "  Device powers on and hangs at splash screen.  ",
                    ContactName = "  Alex Repair  ",
                    ContactEmail = "  alex@example.com  ",
                    ContactPhone = "  01234 567890  ",
                    SupportTicketNumber = "  TCK-100  ",
                    OriginalOrderNumber = "  ORD-200  ",
                    OriginalInvoiceNumber = "  INV-300  ",
                    MigrationSource = "  Planner manual recreation  ",
                    OriginalPlannerTaskTitle = "  Acme return task  ",
                    OriginalPlannerNotes = "  Planner notes for the original task.  ",
                    LinkedBuildRecordId = linkedBuildRecordId
                },
                " DOMAIN\\alice ");

            Assert.True(result.Succeeded);
            Assert.NotNull(result.RmaRecordId);

            await using var verifyContext = new BuildBookDbContext(options);
            var rmaRecord = await verifyContext.RmaRecords
                .Include(record => record.Customer)
                .SingleAsync(record => record.Id == result.RmaRecordId);
            var checklistItems = await verifyContext.RmaChecklistItems
                .Where(item => item.RmaRecordId == rmaRecord.Id)
                .OrderBy(item => item.DisplayOrder)
                .ToListAsync();
            var statusHistory = await verifyContext.RmaStatusHistory
                .SingleAsync(entry => entry.RmaRecordId == rmaRecord.Id);
            var auditEntry = await verifyContext.RmaAudit
                .SingleAsync(entry => entry.RmaRecordId == rmaRecord.Id);

            Assert.Equal("RMA-0001", rmaRecord.RmaNumber);
            Assert.Equal(RmaStatus.BookedIn, rmaRecord.Status);
            Assert.Equal(linkedBuildRecordId, rmaRecord.BuildRecordId);
            Assert.Equal("Acme Medical", rmaRecord.Customer?.Name);
            Assert.Equal("RadSight Access Terminal", rmaRecord.ProductName);
            Assert.Equal("CDM61100", rmaRecord.ProductCode);
            Assert.Equal("SN-1000", rmaRecord.SerialNumber);
            Assert.Equal("Does not boot", rmaRecord.FaultSummary);
            Assert.Equal("Device powers on and hangs at splash screen.", rmaRecord.InitialFaultDescription);
            Assert.Equal("Planner manual recreation", rmaRecord.MigrationSource);
            Assert.Equal("Acme return task", rmaRecord.OriginalPlannerTaskTitle);
            Assert.Equal("Planner notes for the original task.", rmaRecord.OriginalPlannerNotes);
            Assert.Equal("DOMAIN\\alice", rmaRecord.CreatedBy);
            Assert.Equal(RmaChecklistTemplate.DefaultItems.Length, checklistItems.Count);
            Assert.Equal(RmaChecklistTemplate.DefaultItems[0], checklistItems[0].Text);
            Assert.Equal(RmaStatus.BookedIn, statusHistory.NewStatus);
            Assert.Equal("RMA created.", statusHistory.Reason);
            Assert.Equal("Created", auditEntry.Action);
            Assert.Equal("DOMAIN\\alice", auditEntry.User);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task SearchAsync_FiltersByStatusCustomerAndLinkedBuildRecord()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaSearch");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                var acme = CreateCustomer("Acme Medical");
                var beta = CreateCustomer("Beta Clinic");
                var buildRecord = CreateBuildRecord("CDM61100", "RadSight Access Terminal", "SN-1000", acme);

                setupContext.Customers.AddRange(acme, beta);
                setupContext.BuildRecords.Add(buildRecord);
                setupContext.RmaRecords.AddRange(
                    CreateRmaRecord("RMA-0001", "RadSight Access Terminal", "SN-1000", acme, buildRecord, RmaStatus.BookedIn),
                    CreateRmaRecord("RMA-0002", "RadSight Access Terminal", "SN-1001", acme, null, RmaStatus.WorkInProgress),
                    CreateRmaRecord("RMA-0003", "Other Device", "SN-2000", beta, null, RmaStatus.BookedIn));

                await setupContext.SaveChangesAsync();
            }

            var service = CreateService(options);

            var rows = await service.SearchAsync(new RmaRegisterFilter
            {
                Status = RmaStatus.BookedIn,
                Customer = "Acme",
                HasLinkedBuildRecord = true
            });

            var row = Assert.Single(rows);
            Assert.Equal("RMA-0001", row.RmaNumber);
            Assert.True(row.HasLinkedBuildRecord);
            Assert.Equal("Acme Medical", row.CustomerName);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task SuggestBuildRecordMatchesAsync_ReturnsExactSerialMatchWithReasons()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaMatches");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = CreateCustomer("Acme Medical");
                setupContext.Customers.Add(customer);
                setupContext.BuildRecords.AddRange(
                    CreateBuildRecord("CDM61100", "RadSight Access Terminal", "SN-1000", customer),
                    CreateBuildRecord("CDM62200", "Other Device", "SN-9999", customer));
                await setupContext.SaveChangesAsync();
            }

            var service = CreateService(options);

            var matches = await service.SuggestBuildRecordMatchesAsync(
                new RmaBuildRecordMatchRequest("SN-1000", "CDM61100", "RadSight Access Terminal", "Acme Medical"));

            var match = Assert.Single(matches);
            Assert.Equal("SN-1000", match.SerialNumber);
            Assert.Contains("Serial number match", match.MatchReasons);
            Assert.Contains("Product code match", match.MatchReasons);
            Assert.Contains("Product name match", match.MatchReasons);
            Assert.Contains("Customer match", match.MatchReasons);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateIntakeAsync_PersistsChangesAndCreatesAuditEntries()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaUpdateIntake");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var existingCustomer = CreateCustomer("Old Customer");
                setupContext.Customers.Add(existingCustomer);

                var rmaRecord = CreateRmaRecord("RMA-0001", "Old Device", "OLD-001", existingCustomer);
                rmaRecord.ProductCode = "OLD-CODE";
                rmaRecord.FaultSummary = "Old fault";
                rmaRecord.InitialFaultDescription = "Old description";
                rmaRecord.ContactName = "Old Contact";
                rmaRecord.SupportTicketNumber = "OLD-TICKET";

                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();
                rmaRecordId = rmaRecord.Id;
            }

            var service = CreateService(options);

            var result = await service.UpdateIntakeAsync(
                rmaRecordId,
                new UpdateRmaIntakeRequest
                {
                    CustomerName = "New Customer",
                    ProductName = "Updated Device",
                    ProductCode = "NEW-CODE",
                    SerialNumber = "NEW-001",
                    FaultSummary = "Updated fault",
                    InitialFaultDescription = "Updated initial description",
                    FaultDescription = "Engineer notes added",
                    ContactName = "New Contact",
                    ContactEmail = "new@example.com",
                    ContactPhone = "01234 000000",
                    CustomerAddress = "1 Service Road",
                    CustomerReference = "CUST-REF",
                    SupportTicketNumber = "NEW-TICKET",
                    OriginalOrderNumber = "ORD-500",
                    OriginalOrderDate = new DateOnly(2026, 6, 20),
                    OriginalInvoiceNumber = "INV-600",
                    MigrationSource = "Planner board",
                    OriginalPlannerTaskTitle = "Old Planner card",
                    OriginalPlannerNotes = "Copied from Planner during migration."
                },
                "DOMAIN\\editor");

            Assert.True(result.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var savedRmaRecord = await verifyContext.RmaRecords
                .Include(record => record.Customer)
                .SingleAsync(record => record.Id == rmaRecordId);
            var auditEntries = await verifyContext.RmaAudit
                .Where(entry => entry.RmaRecordId == rmaRecordId)
                .ToListAsync();

            Assert.Equal("New Customer", savedRmaRecord.Customer?.Name);
            Assert.Equal("Updated Device", savedRmaRecord.ProductName);
            Assert.Equal("NEW-CODE", savedRmaRecord.ProductCode);
            Assert.Equal("NEW-001", savedRmaRecord.SerialNumber);
            Assert.Equal("Updated fault", savedRmaRecord.FaultSummary);
            Assert.Equal("Updated initial description", savedRmaRecord.InitialFaultDescription);
            Assert.Equal("Engineer notes added", savedRmaRecord.FaultDescription);
            Assert.Equal("Planner board", savedRmaRecord.MigrationSource);
            Assert.Equal("Old Planner card", savedRmaRecord.OriginalPlannerTaskTitle);
            Assert.Equal("Copied from Planner during migration.", savedRmaRecord.OriginalPlannerNotes);
            Assert.Equal("DOMAIN\\editor", savedRmaRecord.LastUpdatedBy);
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "Customer" && entry.NewValue == "New Customer");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "SupportTicketNumber" && entry.NewValue == "NEW-TICKET");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "OriginalOrderDate" && entry.NewValue == "2026-06-20");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "MigrationSource" && entry.NewValue == "Planner board");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "OriginalPlannerTaskTitle" && entry.NewValue == "Old Planner card");
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPlannerMigrationTraceabilityFields()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaPlannerTraceability");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var rmaRecord = CreateRmaRecord("RMA-0001", "Device A", "SN-1000");
                rmaRecord.MigrationSource = "Planner manual migration";
                rmaRecord.OriginalPlannerTaskTitle = "Planner issue 14";
                rmaRecord.OriginalPlannerNotes = "Original notes from the Planner card.";

                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();
                rmaRecordId = rmaRecord.Id;
            }

            var service = CreateService(options);

            var detail = await service.GetByIdAsync(rmaRecordId);

            Assert.NotNull(detail);
            Assert.Equal("Planner manual migration", detail!.MigrationSource);
            Assert.Equal("Planner issue 14", detail.OriginalPlannerTaskTitle);
            Assert.Equal("Original notes from the Planner card.", detail.OriginalPlannerNotes);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task LinkBuildRecordAsync_AndUnlinkBuildRecordAsync_UpdateRelationAndAudit()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaLinking");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;
            int buildRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = CreateCustomer("Acme Medical");
                var buildRecord = CreateBuildRecord("CDM61100", "RadSight Access Terminal", "SN-1000", customer);
                var rmaRecord = CreateRmaRecord("RMA-0001", "RadSight Access Terminal", "SN-1000", customer);

                setupContext.Customers.Add(customer);
                setupContext.BuildRecords.Add(buildRecord);
                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();

                rmaRecordId = rmaRecord.Id;
                buildRecordId = buildRecord.Id;
            }

            var service = CreateService(options);

            var linkResult = await service.LinkBuildRecordAsync(rmaRecordId, buildRecordId, "DOMAIN\\editor");
            var unlinkResult = await service.UnlinkBuildRecordAsync(rmaRecordId, "DOMAIN\\editor");

            Assert.True(linkResult.Succeeded);
            Assert.True(unlinkResult.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var savedRmaRecord = await verifyContext.RmaRecords.SingleAsync(record => record.Id == rmaRecordId);
            var auditEntries = await verifyContext.RmaAudit
                .Where(entry => entry.RmaRecordId == rmaRecordId && entry.FieldChanged == "BuildRecordId")
                .OrderBy(entry => entry.Id)
                .ToListAsync();

            Assert.Null(savedRmaRecord.BuildRecordId);
            Assert.Equal(2, auditEntries.Count);
            Assert.Equal(buildRecordId.ToString(), auditEntries[0].NewValue);
            Assert.Null(auditEntries[1].NewValue);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateFaultDetailsAsync_PersistsStructuredFaultFields()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaFaultDetails");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var rmaRecord = CreateRmaRecord("RMA-0001", "Device A", "SN-1000");
                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();
                rmaRecordId = rmaRecord.Id;
            }

            var service = CreateService(options);

            var result = await service.UpdateFaultDetailsAsync(
                rmaRecordId,
                new UpdateRmaFaultDetailsRequest
                {
                    FaultSummary = "Fails during startup",
                    FaultDescription = "The device freezes during boot.",
                    ReportedSymptoms = "Beeps twice, then hangs.",
                    FaultCategory = RmaFaultCategory.HardwareFailure,
                    FaultSubcategory = "Mainboard",
                    IntermittentFault = true,
                    SafetyConcern = false,
                    DataLossConcern = true,
                    CustomerImpact = RmaCustomerImpact.High,
                    Reproducible = RmaYesNoUnknown.Yes,
                    InitialDiagnosis = "Likely board-level fault."
                },
                "DOMAIN\\engineer");

            Assert.True(result.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var savedRecord = await verifyContext.RmaRecords.SingleAsync(record => record.Id == rmaRecordId);

            Assert.Equal("Fails during startup", savedRecord.FaultSummary);
            Assert.Equal("The device freezes during boot.", savedRecord.FaultDescription);
            Assert.Equal("Beeps twice, then hangs.", savedRecord.ReportedSymptoms);
            Assert.Equal(RmaFaultCategory.HardwareFailure, savedRecord.FaultCategory);
            Assert.Equal("Mainboard", savedRecord.FaultSubcategory);
            Assert.True(savedRecord.IntermittentFault);
            Assert.False(savedRecord.SafetyConcern);
            Assert.True(savedRecord.DataLossConcern);
            Assert.Equal(RmaCustomerImpact.High, savedRecord.CustomerImpact);
            Assert.Equal(RmaYesNoUnknown.Yes, savedRecord.Reproducible);
            Assert.Equal("Likely board-level fault.", savedRecord.InitialDiagnosis);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateTestingQaAsync_AndChangeStatusAsync_WarnsBeforeReadyToShip()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaReadyToShipWarnings");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var rmaRecord = CreateRmaRecord("RMA-0001", "Device A", "SN-1000");
                rmaRecord.Status = RmaStatus.WorkInProgress;
                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();
                rmaRecordId = rmaRecord.Id;
            }

            var service = CreateService(options);

            var testingResult = await service.UpdateTestingQaAsync(
                rmaRecordId,
                new UpdateRmaTestingQaRequest
                {
                    TestRequired = true,
                    TestResult = RmaTestResult.Fail,
                    QaRequired = true,
                    QaResult = RmaQaResult.Fail,
                    ReleaseApproved = false
                },
                "DOMAIN\\qa");

            var statusResult = await service.ChangeStatusAsync(
                rmaRecordId,
                new ChangeRmaStatusRequest
                {
                    NewStatus = RmaStatus.ReadyToShip
                },
                "DOMAIN\\qa");

            Assert.True(testingResult.Succeeded);
            Assert.False(statusResult.Succeeded);
            Assert.True(statusResult.RequiresConfirmation);
            Assert.NotEmpty(statusResult.Warnings);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task SavePartAsync_AndUpdateChecklistItemAsync_PersistRepairSupportRecords()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaPartsAndChecklist");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;
            int checklistItemId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var rmaRecord = CreateRmaRecord("RMA-0001", "Device A", "SN-1000");
                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();
                rmaRecordId = rmaRecord.Id;
                var checklistItem = new RmaChecklistItem
                {
                    RmaRecordId = rmaRecordId,
                    DisplayOrder = 1,
                    Text = "Diagnose fault"
                };
                setupContext.RmaChecklistItems.Add(checklistItem);
                await setupContext.SaveChangesAsync();
                checklistItemId = checklistItem.Id;
            }

            var service = CreateService(options);

            var partResult = await service.SavePartAsync(
                rmaRecordId,
                new SaveRmaPartRequest
                {
                    PartName = "SSD",
                    PartNumber = "SSD-100",
                    Quantity = 1,
                    Supplier = "Parts Co",
                    UnitCost = 89.50m
                },
                "DOMAIN\\repair");
            var checklistResult = await service.UpdateChecklistItemAsync(
                rmaRecordId,
                new UpdateRmaChecklistItemRequest
                {
                    ChecklistItemId = checklistItemId,
                    IsCompleted = true
                },
                "DOMAIN\\repair");

            Assert.True(partResult.Succeeded);
            Assert.True(checklistResult.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var savedPart = await verifyContext.RmaParts.SingleAsync(part => part.RmaRecordId == rmaRecordId);
            var savedChecklistItem = await verifyContext.RmaChecklistItems.SingleAsync(item => item.Id == checklistItemId);

            Assert.Equal("SSD", savedPart.PartName);
            Assert.Equal("SSD-100", savedPart.PartNumber);
            Assert.Equal(1, savedPart.Quantity);
            Assert.True(savedChecklistItem.IsCompleted);
            Assert.Equal("DOMAIN\\repair", savedChecklistItem.CompletedBy);
            Assert.NotNull(savedChecklistItem.CompletedAt);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateShippingAsync_AndUpdateCustomerSummaryAsync_PersistFeatureFields()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaShippingSummary");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int rmaRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var rmaRecord = CreateRmaRecord("RMA-0001", "Device A", "SN-1000");
                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();
                rmaRecordId = rmaRecord.Id;
            }

            var service = CreateService(options);

            var shippingResult = await service.UpdateShippingAsync(
                rmaRecordId,
                new UpdateRmaShippingRequest
                {
                    ReturnMethod = "Courier",
                    Courier = "DHL",
                    TrackingNumber = "DHL-12345",
                    CollectionArranged = true,
                    CollectionDate = new DateOnly(2026, 6, 29),
                    ShippedDate = new DateOnly(2026, 6, 30),
                    ShippedBy = "DOMAIN\\dispatch",
                    ReturnAddress = "1 Demo Street",
                    ShippingNotes = "Packed with charger.",
                    ProofOfDeliveryReceived = true,
                    ProofOfDeliveryDate = new DateOnly(2026, 7, 1)
                },
                "DOMAIN\\dispatch");
            var summaryResult = await service.UpdateCustomerSummaryAsync(
                rmaRecordId,
                new UpdateRmaCustomerSummaryRequest
                {
                    CustomerFacingSummary = "Drive replaced, system retested and returned."
                },
                "DOMAIN\\dispatch");

            Assert.True(shippingResult.Succeeded);
            Assert.True(summaryResult.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var savedRecord = await verifyContext.RmaRecords.SingleAsync(record => record.Id == rmaRecordId);
            var auditEntries = await verifyContext.RmaAudit
                .Where(entry => entry.RmaRecordId == rmaRecordId)
                .ToListAsync();

            Assert.Equal("Courier", savedRecord.ReturnMethod);
            Assert.Equal("DHL", savedRecord.Courier);
            Assert.Equal("DHL-12345", savedRecord.TrackingNumber);
            Assert.True(savedRecord.CollectionArranged);
            Assert.Equal(new DateOnly(2026, 6, 30), savedRecord.ShippedDate);
            Assert.Equal("DOMAIN\\dispatch", savedRecord.ShippedBy);
            Assert.Equal("Drive replaced, system retested and returned.", savedRecord.CustomerFacingSummary);
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "ReturnMethod" && entry.NewValue == "Courier");
            Assert.Contains(auditEntries, entry => entry.FieldChanged == "CustomerFacingSummary" && entry.NewValue == "Drive replaced, system retested and returned.");
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task SaveNoteCommunicationAndAttachmentAsync_PersistAndRetrieveFeatureRecords()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaFeatureRecords");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);
        var attachmentRoot = Path.Combine(Path.GetTempPath(), "BuildBookTests", Guid.NewGuid().ToString("N"));

        try
        {
            int rmaRecordId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var rmaRecord = CreateRmaRecord("RMA-0001", "Device A", "SN-1000");
                setupContext.RmaRecords.Add(rmaRecord);
                await setupContext.SaveChangesAsync();
                rmaRecordId = rmaRecord.Id;
            }

            var service = CreateService(options, attachmentRoot);

            var noteResult = await service.SaveNoteAsync(
                rmaRecordId,
                new SaveRmaNoteRequest
                {
                    NoteType = RmaNoteType.CommercialNote,
                    NoteText = "Awaiting customer purchase order."
                },
                "DOMAIN\\support");

            var communicationResult = await service.SaveCommunicationAsync(
                rmaRecordId,
                new SaveRmaCommunicationRequest
                {
                    CommunicationDate = new DateOnly(2026, 6, 29),
                    ContactMethod = "Email",
                    ContactPerson = "Alex Repair",
                    Summary = "Customer approved the quoted repair.",
                    FollowUpRequired = true,
                    FollowUpDate = new DateOnly(2026, 7, 2)
                },
                "DOMAIN\\support");

            await using var uploadStream = new MemoryStream("test-result"u8.ToArray());
            var attachmentResult = await service.SaveAttachmentAsync(
                rmaRecordId,
                new SaveRmaAttachmentRequest
                {
                    FileName = "result.txt",
                    ContentType = "text/plain",
                    AttachmentType = "Test result",
                    Description = "Bench test output."
                },
                uploadStream,
                "DOMAIN\\support");

            var notes = await service.GetNotesAsync(rmaRecordId);
            var communications = await service.GetCommunicationsAsync(rmaRecordId);
            var attachments = await service.GetAttachmentsAsync(rmaRecordId);

            Assert.True(noteResult.Succeeded);
            Assert.True(communicationResult.Succeeded);
            Assert.True(attachmentResult.Succeeded);

            var note = Assert.Single(notes);
            var communication = Assert.Single(communications);
            var attachment = Assert.Single(attachments);
            var attachmentContent = await service.GetAttachmentContentAsync(rmaRecordId, attachment.Id);

            Assert.Equal(RmaNoteType.CommercialNote, note.NoteType);
            Assert.Equal("Awaiting customer purchase order.", note.NoteText);
            Assert.Equal("Email", communication.ContactMethod);
            Assert.True(communication.FollowUpRequired);
            Assert.Equal("result.txt", attachment.FileName);
            Assert.NotNull(attachmentContent);
            Assert.Equal("text/plain", attachmentContent!.ContentType);
            Assert.Equal("test-result", System.Text.Encoding.UTF8.GetString(attachmentContent.Content));

            var deleteResult = await service.DeleteAttachmentAsync(rmaRecordId, attachment.Id, "DOMAIN\\support");
            Assert.True(deleteResult.Succeeded);
            Assert.Empty(await service.GetAttachmentsAsync(rmaRecordId));
        }
        finally
        {
            if (Directory.Exists(attachmentRoot))
            {
                Directory.Delete(attachmentRoot, recursive: true);
            }

            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task GetBoardAsync_ReturnsChecklistProgressRepeatCountsAndWarnings()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaBoard");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = CreateCustomer("Acme Medical");
                var buildRecord = CreateBuildRecord("CDM61100", "Device A", "SN-1000", customer);
                var previousRma = CreateRmaRecord("RMA-0001", "Device A", "SN-1000", customer, buildRecord, RmaStatus.Closed);
                var readyToShipRma = CreateRmaRecord("RMA-0002", "Device A", "SN-1000", customer, null, RmaStatus.ReadyToShip);
                readyToShipRma.DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));

                setupContext.Customers.Add(customer);
                setupContext.BuildRecords.Add(buildRecord);
                setupContext.RmaRecords.AddRange(previousRma, readyToShipRma);
                await setupContext.SaveChangesAsync();

                setupContext.RmaChecklistItems.AddRange(
                    new RmaChecklistItem
                    {
                        RmaRecordId = readyToShipRma.Id,
                        DisplayOrder = 1,
                        Text = "Diagnose fault",
                        IsCompleted = true
                    },
                    new RmaChecklistItem
                    {
                        RmaRecordId = readyToShipRma.Id,
                        DisplayOrder = 2,
                        Text = "Run functional test",
                        IsCompleted = false
                    });

                await setupContext.SaveChangesAsync();
            }

            var service = CreateService(options);
            var board = await service.GetBoardAsync();

            var card = Assert.Single(board, item => item.RmaNumber == "RMA-0002");
            Assert.Equal(1, card.CompletedChecklistCount);
            Assert.Equal(2, card.TotalChecklistCount);
            Assert.True(card.IsOverdue);
            Assert.Equal(1, card.PreviousRmaCount);
            Assert.Contains("No linked Build Record", card.Warnings);
            Assert.Contains("Checklist incomplete", card.Warnings);
            Assert.Contains("Repair action missing", card.Warnings);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task BuildRecordLinkageHelpers_ReturnHistoryRepeatSummaryAndCreatePrefill()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions("BuildBookRmaBuildRecordLinkage");
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            int buildRecordId;
            int currentRmaId;

            await using (var setupContext = new BuildBookDbContext(options))
            {
                var customer = CreateCustomer("Acme Medical");
                var buildRecord = CreateBuildRecord("CDM61100", "Device A", "SN-1000", customer);
                var olderRma = CreateRmaRecord("RMA-0001", "Device A", "SN-1000", customer, buildRecord, RmaStatus.Closed);
                var currentRma = CreateRmaRecord("RMA-0002", "Device A", "SN-1000", customer, buildRecord, RmaStatus.WorkInProgress);

                setupContext.Customers.Add(customer);
                setupContext.BuildRecords.Add(buildRecord);
                setupContext.RmaRecords.AddRange(olderRma, currentRma);
                await setupContext.SaveChangesAsync();

                buildRecordId = buildRecord.Id;
                currentRmaId = currentRma.Id;
            }

            var service = CreateService(options);
            var history = await service.GetBuildRecordHistoryAsync(buildRecordId);
            var repeatSummary = await service.GetRepeatReturnSummaryAsync(
                new RmaRepeatReturnRequest(currentRmaId, buildRecordId, "SN-1000"));
            var prefill = await service.GetCreatePrefillAsync(buildRecordId);

            Assert.Equal(2, history.Count);
            Assert.Equal("RMA-0002", history[0].RmaNumber);
            Assert.True(repeatSummary.HasPreviousRmas);
            Assert.Equal(1, repeatSummary.PreviousRmaCount);
            Assert.Equal("RMA-0001", Assert.Single(repeatSummary.PreviousRmas).RmaNumber);
            Assert.NotNull(prefill);
            Assert.Equal(buildRecordId, prefill!.BuildRecordId);
            Assert.Equal("CDM61100", prefill.ProductCode);
            Assert.Equal("SN-1000", prefill.SerialNumber);
            Assert.Equal("Acme Medical", prefill.CustomerName);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    private static RmaRecordService CreateService(DbContextOptions<BuildBookDbContext> options, string? attachmentRoot = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BuildBook:RmaAttachmentStorageDirectory"] = attachmentRoot ?? Path.Combine(Path.GetTempPath(), "BuildBookTests", Guid.NewGuid().ToString("N"))
            })
            .Build();

        return new RmaRecordService(
            new TestDbContextFactory(options),
            new RmaAuditService(),
            new RmaStatusTransitionService(),
            new LocalRmaAttachmentStorage(configuration));
    }

    private static async Task<int> SeedBuildRecordAsync(
        DbContextOptions<BuildBookDbContext> options,
        string customerName,
        string productCode,
        string productName,
        string serialNumber)
    {
        await using var context = new BuildBookDbContext(options);
        var customer = CreateCustomer(customerName);
        var buildRecord = CreateBuildRecord(productCode, productName, serialNumber, customer);

        context.Customers.Add(customer);
        context.BuildRecords.Add(buildRecord);
        await context.SaveChangesAsync();

        return buildRecord.Id;
    }

    private static Customer CreateCustomer(string name)
    {
        return new Customer
        {
            Name = name,
            CreatedBy = "tester",
            LastUpdatedBy = "tester",
            IsActive = true
        };
    }

    private static BuildRecord CreateBuildRecord(string productCode, string productName, string serialNumber, Customer customer)
    {
        return new BuildRecord
        {
            ProductCode = productCode,
            ProductName = productName,
            SerialNumber = serialNumber,
            Customer = customer,
            CreatedBy = "tester",
            LastUpdatedBy = "tester",
            IsActive = true
        };
    }

    private static RmaRecord CreateRmaRecord(
        string rmaNumber,
        string productName,
        string serialNumber,
        Customer? customer = null,
        BuildRecord? buildRecord = null,
        RmaStatus status = RmaStatus.BookedIn)
    {
        return new RmaRecord
        {
            RmaNumber = rmaNumber,
            Customer = customer,
            ProductName = productName,
            SerialNumber = serialNumber,
            FaultSummary = $"Fault for {serialNumber}",
            InitialFaultDescription = $"Initial fault for {serialNumber}",
            BuildRecord = buildRecord,
            Status = status,
            CreatedBy = "tester",
            LastUpdatedBy = "tester",
            IsActive = true
        };
    }
}
