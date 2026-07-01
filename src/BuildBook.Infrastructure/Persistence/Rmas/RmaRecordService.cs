using System.Data;
using BuildBook.Application.Rmas;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Customers;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public sealed class RmaRecordService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IRmaAuditService rmaAuditService,
    IRmaStatusTransitionService rmaStatusTransitionService,
    IRmaAttachmentStorage rmaAttachmentStorage) : IRmaRecordService, IRmaRegisterReader
{
    private static readonly HashSet<string> ReadyToShipDeferredChecklistItems =
    [
        "Arrange courier/collection",
        "Mark shipped",
        "Close RMA"
    ];

    public async Task<CreateRmaResult> CreateAsync(
        CreateRmaRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = CreateRmaValidator.Validate(request);

        if (validationErrors.Count > 0)
        {
            return CreateRmaResult.Failure([.. validationErrors]);
        }

        var userName = NormalizeUserName(createdBy);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var linkedBuildRecord = await ResolveLinkedBuildRecordAsync(dbContext, request.LinkedBuildRecordId, cancellationToken);
        if (request.LinkedBuildRecordId is not null && linkedBuildRecord is null)
        {
            return CreateRmaResult.Failure("Selected Build Record was not found.");
        }

        var customer = await GetOrCreateCustomerAsync(
            dbContext,
            request.CustomerName,
            userName,
            linkedBuildRecord?.CustomerId,
            cancellationToken);
        var contractPriority = CustomerContractRmaDefaults.GetPriority(customer.SupportContractStatus, customer.SupportContractLevel);
        var contractWarrantyStatus = CustomerContractRmaDefaults.GetWarrantyStatus(customer.SupportContractStatus, customer.SupportContractLevel);
        var contractWarrantyExpiryDate = CustomerContractRmaDefaults.GetWarrantyExpiryDate(customer.SupportContractStatus, customer.SupportContractEndDate);
        var now = DateTimeOffset.UtcNow;
        var rmaRecord = new RmaRecord
        {
            RmaNumber = await RmaNumberGenerator.GenerateNextAsync(dbContext, cancellationToken),
            BuildRecord = linkedBuildRecord,
            Status = RmaStatus.BookedIn,
            CreatedAt = now,
            CreatedBy = userName,
            LastUpdatedAt = now,
            LastUpdatedBy = userName,
            Customer = customer,
            Priority = contractPriority,
            ProductName = request.ProductName.Trim(),
            ProductCode = NormalizeOptionalValue(request.ProductCode),
            SerialNumber = NormalizeOptionalValue(request.SerialNumber),
            FaultSummary = request.FaultSummary.Trim(),
            InitialFaultDescription = request.InitialFaultDescription.Trim(),
            ContactName = NormalizeOptionalValue(request.ContactName),
            ContactEmail = NormalizeOptionalValue(request.ContactEmail),
            ContactPhone = NormalizeOptionalValue(request.ContactPhone),
            SupportTicketNumber = NormalizeOptionalValue(request.SupportTicketNumber),
            OriginalOrderNumber = NormalizeOptionalValue(request.OriginalOrderNumber),
            OriginalInvoiceNumber = NormalizeOptionalValue(request.OriginalInvoiceNumber),
            MigrationSource = NormalizeOptionalValue(request.MigrationSource),
            OriginalPlannerTaskTitle = NormalizeOptionalValue(request.OriginalPlannerTaskTitle),
            OriginalPlannerNotes = NormalizeOptionalValue(request.OriginalPlannerNotes),
            WarrantyStatus = contractWarrantyStatus,
            WarrantyExpiryDate = contractWarrantyExpiryDate,
            IsActive = true
        };

        dbContext.RmaRecords.Add(rmaRecord);
        dbContext.RmaStatusHistory.Add(new RmaStatusHistory
        {
            RmaRecord = rmaRecord,
            NewStatus = RmaStatus.BookedIn,
            ChangedBy = userName,
            ChangedAt = now,
            Reason = "RMA created."
        });

        for (var index = 0; index < RmaChecklistTemplate.DefaultItems.Length; index++)
        {
            dbContext.RmaChecklistItems.Add(new RmaChecklistItem
            {
                RmaRecord = rmaRecord,
                DisplayOrder = index + 1,
                Text = RmaChecklistTemplate.DefaultItems[index],
                ShowInBoardView = index < 4
            });
        }

        dbContext.RmaAudit.Add(rmaAuditService.CreateRecordCreatedEntry(rmaRecord, userName));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreateRmaResult.Success(rmaRecord.Id);
    }

    public async Task<RmaDetailModel?> GetByIdAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord => rmaRecord.Id == rmaRecordId && rmaRecord.IsActive)
            .Select(rmaRecord => new RmaDetailModel(
                rmaRecord.Id,
                rmaRecord.RmaNumber,
                rmaRecord.Status,
                rmaRecord.Priority,
                rmaRecord.ProductName,
                rmaRecord.ProductCode,
                rmaRecord.SerialNumber,
                rmaRecord.CustomerId,
                rmaRecord.Customer == null ? null : rmaRecord.Customer.Name,
                rmaRecord.FaultSummary,
                rmaRecord.InitialFaultDescription,
                rmaRecord.FaultDescription,
                rmaRecord.FaultCategory,
                rmaRecord.FaultSubcategory,
                rmaRecord.ReportedSymptoms,
                rmaRecord.IntermittentFault,
                rmaRecord.SafetyConcern,
                rmaRecord.DataLossConcern,
                rmaRecord.CustomerImpact,
                rmaRecord.Reproducible,
                rmaRecord.InitialDiagnosis,
                rmaRecord.DiagnosisNotes,
                rmaRecord.RootCause,
                rmaRecord.RootCauseCategory,
                rmaRecord.RepairActionTaken,
                rmaRecord.RepairCompletedDate,
                rmaRecord.RepairCompletedBy,
                rmaRecord.WarrantyStatus,
                rmaRecord.WarrantyExpiryDate,
                rmaRecord.ChargeableRepair,
                rmaRecord.CustomerApprovalRequired,
                rmaRecord.CustomerApprovalReceived,
                rmaRecord.CustomerApprovalDate,
                rmaRecord.QuoteNumber,
                rmaRecord.PurchaseOrderNumber,
                rmaRecord.RepairInvoiceNumber,
                rmaRecord.EstimatedRepairCost,
                rmaRecord.ActualRepairCost,
                rmaRecord.TestRequired,
                rmaRecord.TestPlanUsed,
                rmaRecord.TestResult,
                rmaRecord.TestedBy,
                rmaRecord.TestDate,
                rmaRecord.TestNotes,
                rmaRecord.QaRequired,
                rmaRecord.QaResult,
                rmaRecord.QaCheckedBy,
                rmaRecord.QaDate,
                rmaRecord.ReleaseApproved,
                rmaRecord.ReleaseApprovedBy,
                rmaRecord.ReleaseApprovedAt,
                rmaRecord.ContactName,
                rmaRecord.ContactEmail,
                rmaRecord.ContactPhone,
                rmaRecord.CustomerAddress,
                rmaRecord.CustomerReference,
                rmaRecord.SupportTicketNumber,
                rmaRecord.SupportTicketUrl,
                rmaRecord.OriginalOrderNumber,
                rmaRecord.OriginalOrderDate,
                rmaRecord.OriginalInvoiceNumber,
                rmaRecord.MigrationSource,
                rmaRecord.OriginalPlannerTaskTitle,
                rmaRecord.OriginalPlannerNotes,
                rmaRecord.ReturnMethod,
                rmaRecord.Courier,
                rmaRecord.TrackingNumber,
                rmaRecord.CollectionArranged,
                rmaRecord.CollectionDate,
                rmaRecord.ShippedDate,
                rmaRecord.ShippedBy,
                rmaRecord.ReturnAddress,
                rmaRecord.ShippingNotes,
                rmaRecord.ProofOfDeliveryReceived,
                rmaRecord.ProofOfDeliveryDate,
                rmaRecord.CustomerFacingSummary,
                rmaRecord.BuildRecordId,
                rmaRecord.BuildRecord == null ? null : rmaRecord.BuildRecord.SerialNumber,
                rmaRecord.BuildRecord == null ? null : rmaRecord.BuildRecord.ProductName,
                rmaRecord.BuildRecord == null || rmaRecord.BuildRecord.Customer == null ? null : rmaRecord.BuildRecord.Customer.Name,
                rmaRecord.DateItemReceived,
                rmaRecord.ReceivedBy,
                rmaRecord.DueDate,
                rmaRecord.TargetCompletionDate,
                rmaRecord.AssignedTo,
                rmaRecord.OnHoldReason,
                rmaRecord.Outcome,
                rmaRecord.ClosureNotes,
                rmaRecord.ClosedAt,
                rmaRecord.ClosedBy,
                rmaRecord.CreatedAt,
                rmaRecord.CreatedBy,
                rmaRecord.LastUpdatedAt,
                rmaRecord.LastUpdatedBy))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<RmaDashboardSummary> GetDashboardSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var activeRmas = dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord => rmaRecord.IsActive);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var openCount = await activeRmas.CountAsync(rmaRecord => rmaRecord.Status != RmaStatus.Closed, cancellationToken);
        var overdueCount = await activeRmas.CountAsync(
            rmaRecord => rmaRecord.Status != RmaStatus.Closed
                && rmaRecord.Status != RmaStatus.CancelledNoReply
                && rmaRecord.Status != RmaStatus.CustomerFixed
                && rmaRecord.DueDate != null
                && rmaRecord.DueDate < today,
            cancellationToken);
        var waitingForCustomerCount = await activeRmas.CountAsync(
            rmaRecord => rmaRecord.Status == RmaStatus.OnHold
                && rmaRecord.OnHoldReason == RmaOnHoldReasons.WaitingForCustomer,
            cancellationToken);
        var waitingForPartsCount = await activeRmas.CountAsync(
            rmaRecord => rmaRecord.Status == RmaStatus.OnHold
                && rmaRecord.OnHoldReason == RmaOnHoldReasons.WaitingForParts,
            cancellationToken);
        var readyToShipCount = await activeRmas.CountAsync(
            rmaRecord => rmaRecord.Status == RmaStatus.ReadyToShip,
            cancellationToken);
        var shippedNotClosedCount = await activeRmas.CountAsync(
            rmaRecord => rmaRecord.Status == RmaStatus.Shipped,
            cancellationToken);

        return new RmaDashboardSummary(
            openCount,
            overdueCount,
            waitingForCustomerCount,
            waitingForPartsCount,
            readyToShipCount,
            shippedNotClosedCount);
    }

    public async Task<IReadOnlyList<RmaBoardCardModel>> GetBoardAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var records = await dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord => rmaRecord.IsActive)
            .Select(rmaRecord => new
            {
                rmaRecord.Id,
                rmaRecord.RmaNumber,
                rmaRecord.Status,
                CustomerName = rmaRecord.Customer == null ? null : rmaRecord.Customer.Name,
                rmaRecord.ProductName,
                rmaRecord.SerialNumber,
                rmaRecord.FaultSummary,
                rmaRecord.Priority,
                rmaRecord.AssignedTo,
                rmaRecord.DueDate,
                rmaRecord.BuildRecordId,
                rmaRecord.RepairActionTaken,
                rmaRecord.RepairCompletedDate,
                rmaRecord.RepairCompletedBy,
                rmaRecord.TestRequired,
                rmaRecord.TestResult,
                rmaRecord.QaRequired,
                rmaRecord.QaResult,
                rmaRecord.ReleaseApproved,
                rmaRecord.CustomerApprovalRequired,
                rmaRecord.CustomerApprovalReceived
            })
            .OrderBy(rmaRecord => rmaRecord.Status)
            .ThenBy(rmaRecord => rmaRecord.DueDate)
            .ThenBy(rmaRecord => rmaRecord.RmaNumber)
            .ToListAsync(cancellationToken);

        var recordIds = records.Select(record => record.Id).ToArray();
        var checklistItems = await dbContext.RmaChecklistItems
            .AsNoTracking()
            .Where(item => recordIds.Contains(item.RmaRecordId))
            .Select(item => new
            {
                item.RmaRecordId,
                item.Text,
                item.IsCompleted
            })
            .ToListAsync(cancellationToken);

        var checklistLookup = checklistItems
            .GroupBy(item => item.RmaRecordId)
            .ToDictionary(group => group.Key, group => group.ToList());

        return records
            .Select(record =>
            {
                checklistLookup.TryGetValue(record.Id, out var recordChecklistItems);
                recordChecklistItems ??= [];
                var boardChecklistItems = recordChecklistItems
                    .Select(item => (item.Text, item.IsCompleted))
                    .ToList();

                var completedChecklistCount = boardChecklistItems.Count(item => item.IsCompleted);
                var warnings = BuildBoardWarnings(
                    record.Status,
                    today,
                    record.DueDate,
                    record.SerialNumber,
                    record.BuildRecordId,
                    record.RepairActionTaken,
                    record.RepairCompletedDate,
                    record.RepairCompletedBy,
                    record.TestRequired,
                    record.TestResult,
                    record.QaRequired,
                    record.QaResult,
                    record.ReleaseApproved,
                    record.CustomerApprovalRequired,
                    record.CustomerApprovalReceived,
                    boardChecklistItems);

                var previousRmaCount = records.Count(other =>
                    other.Id != record.Id
                    && HasRepeatLink(
                        record.BuildRecordId,
                        record.SerialNumber,
                        other.BuildRecordId,
                        other.SerialNumber));

                return new RmaBoardCardModel(
                    record.Id,
                    record.RmaNumber,
                    record.Status,
                    record.CustomerName,
                    record.ProductName,
                    record.SerialNumber,
                    record.FaultSummary,
                    record.Priority,
                    record.AssignedTo,
                    record.DueDate,
                    record.BuildRecordId is not null,
                    record.BuildRecordId,
                    IsOverdue(record.Status, record.DueDate, today),
                    completedChecklistCount,
                    boardChecklistItems.Count,
                    previousRmaCount,
                    warnings);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<RmaStatusHistoryEntry>> GetStatusHistoryAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.RmaStatusHistory
            .AsNoTracking()
            .Where(entry => entry.RmaRecordId == rmaRecordId)
            .OrderByDescending(entry => entry.ChangedAt)
            .ThenByDescending(entry => entry.Id)
            .Select(entry => new RmaStatusHistoryEntry(
                entry.Id,
                entry.OldStatus,
                entry.NewStatus,
                entry.ChangedBy,
                entry.ChangedAt,
                entry.Reason))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RmaChecklistItemModel>> GetChecklistAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.RmaChecklistItems
            .AsNoTracking()
            .Where(item => item.RmaRecordId == rmaRecordId)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Id)
            .Select(item => new RmaChecklistItemModel(
                item.Id,
                item.DisplayOrder,
                item.Text,
                item.IsCompleted,
                item.CompletedBy,
                item.CompletedAt,
                item.ShowInBoardView,
                item.DisplayOrder > RmaChecklistTemplate.DefaultItems.Length))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RmaPartModel>> GetPartsAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.RmaParts
            .AsNoTracking()
            .Where(part => part.RmaRecordId == rmaRecordId)
            .OrderBy(part => part.PartName)
            .ThenBy(part => part.Id)
            .Select(part => new RmaPartModel(
                part.Id,
                part.PartName,
                part.PartNumber,
                part.Quantity,
                part.SerialNumber,
                part.Supplier,
                part.UnitCost,
                part.Notes))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RmaNoteModel>> GetNotesAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.RmaNotes
            .AsNoTracking()
            .Where(note => note.RmaRecordId == rmaRecordId)
            .OrderByDescending(note => note.CreatedAt)
            .ThenByDescending(note => note.Id)
            .Select(note => new RmaNoteModel(
                note.Id,
                note.NoteType,
                note.NoteText,
                note.CreatedBy,
                note.CreatedAt,
                note.LastUpdatedBy,
                note.LastUpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RmaCommunicationModel>> GetCommunicationsAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.RmaCommunications
            .AsNoTracking()
            .Where(communication => communication.RmaRecordId == rmaRecordId)
            .OrderByDescending(communication => communication.CommunicationDate)
            .ThenByDescending(communication => communication.Id)
            .Select(communication => new RmaCommunicationModel(
                communication.Id,
                communication.CommunicationDate,
                communication.ContactMethod,
                communication.ContactPerson,
                communication.Summary,
                communication.FollowUpRequired,
                communication.FollowUpDate,
                communication.CreatedBy,
                communication.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RmaAttachmentModel>> GetAttachmentsAsync(
        int rmaRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.RmaAttachments
            .AsNoTracking()
            .Where(attachment => attachment.RmaRecordId == rmaRecordId)
            .OrderByDescending(attachment => attachment.UploadedAt)
            .ThenByDescending(attachment => attachment.Id)
            .Select(attachment => new RmaAttachmentModel(
                attachment.Id,
                attachment.FileName,
                attachment.ContentType,
                attachment.AttachmentType,
                attachment.Description,
                attachment.UploadedBy,
                attachment.UploadedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<RmaAttachmentContentModel?> GetAttachmentContentAsync(
        int rmaRecordId,
        int attachmentId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await dbContext.RmaAttachments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == attachmentId && item.RmaRecordId == rmaRecordId,
                cancellationToken);

        if (attachment is null)
        {
            return null;
        }

        var content = await rmaAttachmentStorage.ReadAsync(attachment.StoredFilePath, cancellationToken);
        return content is null
            ? null
            : new RmaAttachmentContentModel(
                attachment.FileName,
                attachment.ContentType,
                content);
    }

    public async Task<IReadOnlyList<BuildRecordRmaHistoryRow>> GetBuildRecordHistoryAsync(
        int buildRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord => rmaRecord.IsActive && rmaRecord.BuildRecordId == buildRecordId)
            .OrderByDescending(rmaRecord => rmaRecord.CreatedAt)
            .ThenByDescending(rmaRecord => rmaRecord.Id)
            .Select(rmaRecord => new BuildRecordRmaHistoryRow(
                rmaRecord.Id,
                rmaRecord.RmaNumber,
                rmaRecord.Status,
                rmaRecord.FaultSummary,
                rmaRecord.CreatedAt,
                rmaRecord.ClosedAt,
                rmaRecord.Outcome))
            .ToListAsync(cancellationToken);
    }

    public async Task<RmaRepeatReturnSummary> GetRepeatReturnSummaryAsync(
        RmaRepeatReturnRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var serialNumber = NormalizeOptionalValue(request.SerialNumber);
        var linkedBuildRecordId = request.LinkedBuildRecordId;
        var currentRmaRecordId = request.CurrentRmaRecordId;
        var hasLinkedBuildRecordId = linkedBuildRecordId.HasValue;
        var hasSerialNumber = !string.IsNullOrWhiteSpace(serialNumber);

        if (!hasLinkedBuildRecordId && !hasSerialNumber)
        {
            return RmaRepeatReturnSummary.Empty;
        }

        var rows = await dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord =>
                rmaRecord.IsActive
                && (!currentRmaRecordId.HasValue || rmaRecord.Id != currentRmaRecordId.Value)
                && ((hasLinkedBuildRecordId && rmaRecord.BuildRecordId == linkedBuildRecordId!.Value)
                    || (hasSerialNumber && rmaRecord.SerialNumber == serialNumber)))
            .OrderByDescending(rmaRecord => rmaRecord.CreatedAt)
            .ThenByDescending(rmaRecord => rmaRecord.Id)
            .Select(rmaRecord => new RmaRegisterRow(
                rmaRecord.Id,
                rmaRecord.RmaNumber,
                rmaRecord.Status,
                rmaRecord.Customer == null ? null : rmaRecord.Customer.Name,
                rmaRecord.ProductName,
                rmaRecord.SerialNumber,
                rmaRecord.FaultSummary,
                rmaRecord.Priority,
                rmaRecord.AssignedTo,
                rmaRecord.DueDate,
                rmaRecord.BuildRecordId != null,
                rmaRecord.BuildRecordId,
                rmaRecord.LastUpdatedAt))
            .ToListAsync(cancellationToken);

        return new RmaRepeatReturnSummary(rows.Count, rows);
    }

    public async Task<RmaCreatePrefillModel?> GetCreatePrefillAsync(
        int buildRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.Id == buildRecordId && buildRecord.IsActive)
            .Select(buildRecord => new RmaCreatePrefillModel(
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.SerialNumber,
                buildRecord.Customer == null ? null : buildRecord.Customer.Name))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RmaRegisterRow>> SearchAsync(
        RmaRegisterFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.RmaRecords
            .AsNoTracking()
            .Where(rmaRecord => rmaRecord.IsActive);

        if (filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var pattern = CreateLikePattern(filter.Search);
                query = query.Where(rmaRecord =>
                    EF.Functions.Like(rmaRecord.RmaNumber, pattern, @"\")
                    || EF.Functions.Like(rmaRecord.ProductName, pattern, @"\")
                    || (rmaRecord.ProductCode != null && EF.Functions.Like(rmaRecord.ProductCode, pattern, @"\"))
                    || (rmaRecord.SerialNumber != null && EF.Functions.Like(rmaRecord.SerialNumber, pattern, @"\"))
                    || EF.Functions.Like(rmaRecord.FaultSummary, pattern, @"\")
                    || (rmaRecord.Customer != null && EF.Functions.Like(rmaRecord.Customer.Name, pattern, @"\"))
                    || (rmaRecord.AssignedTo != null && EF.Functions.Like(rmaRecord.AssignedTo, pattern, @"\")));
            }

            if (filter.Status is not null)
            {
                query = query.Where(rmaRecord => rmaRecord.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Customer))
            {
                var pattern = CreateLikePattern(filter.Customer);
                query = query.Where(rmaRecord =>
                    rmaRecord.Customer != null
                    && EF.Functions.Like(rmaRecord.Customer.Name, pattern, @"\"));
            }

            if (!string.IsNullOrWhiteSpace(filter.Product))
            {
                var pattern = CreateLikePattern(filter.Product);
                query = query.Where(rmaRecord =>
                    EF.Functions.Like(rmaRecord.ProductName, pattern, @"\")
                    || (rmaRecord.ProductCode != null && EF.Functions.Like(rmaRecord.ProductCode, pattern, @"\")));
            }

            if (!string.IsNullOrWhiteSpace(filter.SerialNumber))
            {
                var pattern = CreateLikePattern(filter.SerialNumber);
                query = query.Where(rmaRecord =>
                    rmaRecord.SerialNumber != null
                    && EF.Functions.Like(rmaRecord.SerialNumber, pattern, @"\"));
            }

            if (!string.IsNullOrWhiteSpace(filter.AssignedTo))
            {
                var pattern = CreateLikePattern(filter.AssignedTo);
                query = query.Where(rmaRecord =>
                    rmaRecord.AssignedTo != null
                    && EF.Functions.Like(rmaRecord.AssignedTo, pattern, @"\"));
            }

            if (filter.Priority is not null)
            {
                query = query.Where(rmaRecord => rmaRecord.Priority == filter.Priority);
            }

            if (filter.DueDate is not null)
            {
                query = query.Where(rmaRecord => rmaRecord.DueDate == filter.DueDate);
            }

            if (filter.HasLinkedBuildRecord is not null)
            {
                query = filter.HasLinkedBuildRecord.Value
                    ? query.Where(rmaRecord => rmaRecord.BuildRecordId != null)
                    : query.Where(rmaRecord => rmaRecord.BuildRecordId == null);
            }
        }

        query = ApplySorting(query, filter);

        return await query
            .Select(rmaRecord => new RmaRegisterRow(
                rmaRecord.Id,
                rmaRecord.RmaNumber,
                rmaRecord.Status,
                rmaRecord.Customer == null ? null : rmaRecord.Customer.Name,
                rmaRecord.ProductName,
                rmaRecord.SerialNumber,
                rmaRecord.FaultSummary,
                rmaRecord.Priority,
                rmaRecord.AssignedTo,
                rmaRecord.DueDate,
                rmaRecord.BuildRecordId != null,
                rmaRecord.BuildRecordId,
                rmaRecord.LastUpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<RmaRegisterRow>> ListAsync(
        RmaRegisterFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        return SearchAsync(filter, cancellationToken);
    }

    public async Task<IReadOnlyList<RmaBuildRecordMatchSuggestion>> SuggestBuildRecordMatchesAsync(
        RmaBuildRecordMatchRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var normalizedSerialNumber = NormalizeOptionalValue(request.SerialNumber);
        var normalizedProductCode = NormalizeOptionalValue(request.ProductCode);
        var normalizedProductName = NormalizeOptionalValue(request.ProductName);
        var normalizedCustomerName = NormalizeOptionalValue(request.CustomerName);

        if (normalizedSerialNumber is null
            && normalizedProductCode is null
            && normalizedProductName is null
            && normalizedCustomerName is null)
        {
            return [];
        }

        var query = dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.IsActive);

        if (normalizedSerialNumber is not null)
        {
            query = query.Where(buildRecord =>
                buildRecord.SerialNumber == normalizedSerialNumber
                || EF.Functions.Like(buildRecord.SerialNumber, CreateLikePattern(normalizedSerialNumber), @"\"));
        }
        else
        {
            if (normalizedProductCode is not null)
            {
                query = query.Where(buildRecord =>
                    EF.Functions.Like(buildRecord.ProductCode, CreateLikePattern(normalizedProductCode), @"\"));
            }

            if (normalizedProductName is not null)
            {
                query = query.Where(buildRecord =>
                    EF.Functions.Like(buildRecord.ProductName, CreateLikePattern(normalizedProductName), @"\"));
            }

            if (normalizedCustomerName is not null)
            {
                query = query.Where(buildRecord =>
                    buildRecord.Customer != null
                    && EF.Functions.Like(buildRecord.Customer.Name, CreateLikePattern(normalizedCustomerName), @"\"));
            }
        }

        var matches = await query
            .OrderByDescending(buildRecord => normalizedSerialNumber != null && buildRecord.SerialNumber == normalizedSerialNumber)
            .ThenByDescending(buildRecord => normalizedProductCode != null && buildRecord.ProductCode == normalizedProductCode)
            .ThenBy(buildRecord => buildRecord.SerialNumber)
            .Take(8)
            .Select(buildRecord => new
            {
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.SerialNumber,
                CustomerName = buildRecord.Customer == null ? null : buildRecord.Customer.Name,
                buildRecord.MachineName,
                buildRecord.DateShipped
            })
            .ToListAsync(cancellationToken);

        return matches
            .Select(match => new RmaBuildRecordMatchSuggestion(
                match.Id,
                match.ProductCode,
                match.ProductName,
                match.SerialNumber,
                match.CustomerName,
                match.MachineName,
                match.DateShipped,
                BuildMatchReasons(match.SerialNumber, match.ProductCode, match.ProductName, match.CustomerName, normalizedSerialNumber, normalizedProductCode, normalizedProductName, normalizedCustomerName)))
            .ToList();
    }

    public async Task<UpdateRmaIntakeResult> UpdateIntakeAsync(
        int rmaRecordId,
        UpdateRmaIntakeRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return UpdateRmaIntakeResult.Failure("Customer is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            return UpdateRmaIntakeResult.Failure("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FaultSummary))
        {
            return UpdateRmaIntakeResult.Failure("Fault summary is required.");
        }

        if (string.IsNullOrWhiteSpace(request.InitialFaultDescription))
        {
            return UpdateRmaIntakeResult.Failure("Fault description is required.");
        }

        var userName = NormalizeUserName(updatedBy);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .Include(record => record.Customer)
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return UpdateRmaIntakeResult.Failure("RMA Record was not found.");
        }

        var customer = await GetOrCreateCustomerAsync(
            dbContext,
            request.CustomerName,
            userName,
            rmaRecord.CustomerId,
            cancellationToken);
        var contractPriority = CustomerContractRmaDefaults.GetPriority(customer.SupportContractStatus, customer.SupportContractLevel);
        var contractWarrantyStatus = CustomerContractRmaDefaults.GetWarrantyStatus(customer.SupportContractStatus, customer.SupportContractLevel);
        var contractWarrantyExpiryDate = CustomerContractRmaDefaults.GetWarrantyExpiryDate(customer.SupportContractStatus, customer.SupportContractEndDate);

        var productName = request.ProductName.Trim();
        var productCode = NormalizeOptionalValue(request.ProductCode);
        var serialNumber = NormalizeOptionalValue(request.SerialNumber);
        var faultSummary = request.FaultSummary.Trim();
        var initialFaultDescription = request.InitialFaultDescription.Trim();
        var faultDescription = NormalizeOptionalValue(request.FaultDescription);
        var contactName = NormalizeOptionalValue(request.ContactName);
        var contactEmail = NormalizeOptionalValue(request.ContactEmail);
        var contactPhone = NormalizeOptionalValue(request.ContactPhone);
        var customerAddress = NormalizeOptionalValue(request.CustomerAddress);
        var customerReference = NormalizeOptionalValue(request.CustomerReference);
        var supportTicketNumber = NormalizeOptionalValue(request.SupportTicketNumber);
        var originalOrderNumber = NormalizeOptionalValue(request.OriginalOrderNumber);
        var originalInvoiceNumber = NormalizeOptionalValue(request.OriginalInvoiceNumber);
        var migrationSource = NormalizeOptionalValue(request.MigrationSource);
        var originalPlannerTaskTitle = NormalizeOptionalValue(request.OriginalPlannerTaskTitle);
        var originalPlannerNotes = NormalizeOptionalValue(request.OriginalPlannerNotes);

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("Customer", rmaRecord.Customer?.Name, customer.Name),
                new RmaAuditChange("ProductName", rmaRecord.ProductName, productName),
                new RmaAuditChange("ProductCode", rmaRecord.ProductCode, productCode),
                new RmaAuditChange("SerialNumber", rmaRecord.SerialNumber, serialNumber),
                new RmaAuditChange("FaultSummary", rmaRecord.FaultSummary, faultSummary),
                new RmaAuditChange("InitialFaultDescription", rmaRecord.InitialFaultDescription, initialFaultDescription),
                new RmaAuditChange("FaultDescription", rmaRecord.FaultDescription, faultDescription),
                new RmaAuditChange("ContactName", rmaRecord.ContactName, contactName),
                new RmaAuditChange("ContactEmail", rmaRecord.ContactEmail, contactEmail),
                new RmaAuditChange("ContactPhone", rmaRecord.ContactPhone, contactPhone),
                new RmaAuditChange("CustomerAddress", rmaRecord.CustomerAddress, customerAddress),
                new RmaAuditChange("CustomerReference", rmaRecord.CustomerReference, customerReference),
                new RmaAuditChange("SupportTicketNumber", rmaRecord.SupportTicketNumber, supportTicketNumber),
                new RmaAuditChange("OriginalOrderNumber", rmaRecord.OriginalOrderNumber, originalOrderNumber),
                new RmaAuditChange("OriginalOrderDate", FormatDate(rmaRecord.OriginalOrderDate), FormatDate(request.OriginalOrderDate)),
                new RmaAuditChange("OriginalInvoiceNumber", rmaRecord.OriginalInvoiceNumber, originalInvoiceNumber),
                new RmaAuditChange("MigrationSource", rmaRecord.MigrationSource, migrationSource),
                new RmaAuditChange("OriginalPlannerTaskTitle", rmaRecord.OriginalPlannerTaskTitle, originalPlannerTaskTitle),
                new RmaAuditChange("OriginalPlannerNotes", rmaRecord.OriginalPlannerNotes, originalPlannerNotes),
                new RmaAuditChange("Priority", FormatPriority(rmaRecord.Priority), rmaRecord.Priority is null ? FormatPriority(contractPriority) : FormatPriority(rmaRecord.Priority)),
                new RmaAuditChange("WarrantyStatus", FormatWarrantyStatus(rmaRecord.WarrantyStatus), rmaRecord.WarrantyStatus is null ? FormatWarrantyStatus(contractWarrantyStatus) : FormatWarrantyStatus(rmaRecord.WarrantyStatus)),
                new RmaAuditChange("WarrantyExpiryDate", FormatDate(rmaRecord.WarrantyExpiryDate), rmaRecord.WarrantyExpiryDate is null ? FormatDate(contractWarrantyExpiryDate) : FormatDate(rmaRecord.WarrantyExpiryDate))
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateRmaIntakeResult.Success();
        }

        rmaRecord.Customer = customer;
        rmaRecord.CustomerId = customer.Id;
        rmaRecord.ProductName = productName;
        rmaRecord.ProductCode = productCode;
        rmaRecord.SerialNumber = serialNumber;
        rmaRecord.FaultSummary = faultSummary;
        rmaRecord.InitialFaultDescription = initialFaultDescription;
        rmaRecord.FaultDescription = faultDescription;
        rmaRecord.ContactName = contactName;
        rmaRecord.ContactEmail = contactEmail;
        rmaRecord.ContactPhone = contactPhone;
        rmaRecord.CustomerAddress = customerAddress;
        rmaRecord.CustomerReference = customerReference;
        rmaRecord.SupportTicketNumber = supportTicketNumber;
        rmaRecord.OriginalOrderNumber = originalOrderNumber;
        rmaRecord.OriginalOrderDate = request.OriginalOrderDate;
        rmaRecord.OriginalInvoiceNumber = originalInvoiceNumber;
        rmaRecord.MigrationSource = migrationSource;
        rmaRecord.OriginalPlannerTaskTitle = originalPlannerTaskTitle;
        rmaRecord.OriginalPlannerNotes = originalPlannerNotes;
        rmaRecord.Priority ??= contractPriority;
        rmaRecord.WarrantyStatus ??= contractWarrantyStatus;
        rmaRecord.WarrantyExpiryDate ??= contractWarrantyExpiryDate;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateRmaIntakeResult.Success();
    }

    public async Task<UpdateRmaFaultDetailsResult> UpdateFaultDetailsAsync(
        int rmaRecordId,
        UpdateRmaFaultDetailsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FaultSummary))
        {
            return UpdateRmaFaultDetailsResult.Failure("Fault summary is required.");
        }

        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return UpdateRmaFaultDetailsResult.Failure("RMA Record was not found.");
        }

        var faultSummary = request.FaultSummary.Trim();
        var faultDescription = NormalizeOptionalValue(request.FaultDescription);
        var reportedSymptoms = NormalizeOptionalValue(request.ReportedSymptoms);
        var faultSubcategory = NormalizeOptionalValue(request.FaultSubcategory);
        var initialDiagnosis = NormalizeOptionalValue(request.InitialDiagnosis);

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("FaultSummary", rmaRecord.FaultSummary, faultSummary),
                new RmaAuditChange("FaultDescription", rmaRecord.FaultDescription, faultDescription),
                new RmaAuditChange("ReportedSymptoms", rmaRecord.ReportedSymptoms, reportedSymptoms),
                new RmaAuditChange("FaultCategory", FormatFaultCategory(rmaRecord.FaultCategory), FormatFaultCategory(request.FaultCategory)),
                new RmaAuditChange("FaultSubcategory", rmaRecord.FaultSubcategory, faultSubcategory),
                new RmaAuditChange("IntermittentFault", FormatBool(rmaRecord.IntermittentFault), FormatBool(request.IntermittentFault)),
                new RmaAuditChange("SafetyConcern", FormatBool(rmaRecord.SafetyConcern), FormatBool(request.SafetyConcern)),
                new RmaAuditChange("DataLossConcern", FormatBool(rmaRecord.DataLossConcern), FormatBool(request.DataLossConcern)),
                new RmaAuditChange("CustomerImpact", FormatCustomerImpact(rmaRecord.CustomerImpact), FormatCustomerImpact(request.CustomerImpact)),
                new RmaAuditChange("Reproducible", FormatYesNoUnknown(rmaRecord.Reproducible), FormatYesNoUnknown(request.Reproducible)),
                new RmaAuditChange("InitialDiagnosis", rmaRecord.InitialDiagnosis, initialDiagnosis)
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateRmaFaultDetailsResult.Success();
        }

        rmaRecord.FaultSummary = faultSummary;
        rmaRecord.FaultDescription = faultDescription;
        rmaRecord.ReportedSymptoms = reportedSymptoms;
        rmaRecord.FaultCategory = request.FaultCategory;
        rmaRecord.FaultSubcategory = faultSubcategory;
        rmaRecord.IntermittentFault = request.IntermittentFault;
        rmaRecord.SafetyConcern = request.SafetyConcern;
        rmaRecord.DataLossConcern = request.DataLossConcern;
        rmaRecord.CustomerImpact = request.CustomerImpact;
        rmaRecord.Reproducible = request.Reproducible;
        rmaRecord.InitialDiagnosis = initialDiagnosis;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateRmaFaultDetailsResult.Success();
    }

    public async Task<UpdateRmaRepairDetailsResult> UpdateRepairDetailsAsync(
        int rmaRecordId,
        UpdateRmaRepairDetailsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return UpdateRmaRepairDetailsResult.Failure("RMA Record was not found.");
        }

        var diagnosisNotes = NormalizeOptionalValue(request.DiagnosisNotes);
        var rootCause = NormalizeOptionalValue(request.RootCause);
        var repairActionTaken = NormalizeOptionalValue(request.RepairActionTaken);
        var repairCompletedBy = NormalizeOptionalValue(request.RepairCompletedBy);

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("DiagnosisNotes", rmaRecord.DiagnosisNotes, diagnosisNotes),
                new RmaAuditChange("RootCause", rmaRecord.RootCause, rootCause),
                new RmaAuditChange("RootCauseCategory", FormatRootCauseCategory(rmaRecord.RootCauseCategory), FormatRootCauseCategory(request.RootCauseCategory)),
                new RmaAuditChange("RepairActionTaken", rmaRecord.RepairActionTaken, repairActionTaken),
                new RmaAuditChange("RepairCompletedDate", FormatDate(rmaRecord.RepairCompletedDate), FormatDate(request.RepairCompletedDate)),
                new RmaAuditChange("RepairCompletedBy", rmaRecord.RepairCompletedBy, repairCompletedBy)
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateRmaRepairDetailsResult.Success();
        }

        rmaRecord.DiagnosisNotes = diagnosisNotes;
        rmaRecord.RootCause = rootCause;
        rmaRecord.RootCauseCategory = request.RootCauseCategory;
        rmaRecord.RepairActionTaken = repairActionTaken;
        rmaRecord.RepairCompletedDate = request.RepairCompletedDate;
        rmaRecord.RepairCompletedBy = repairCompletedBy;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateRmaRepairDetailsResult.Success();
    }

    public async Task<RmaOperationResult> UpdateWarrantyCommercialAsync(
        int rmaRecordId,
        UpdateRmaWarrantyCommercialRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        var quoteNumber = NormalizeOptionalValue(request.QuoteNumber);
        var purchaseOrderNumber = NormalizeOptionalValue(request.PurchaseOrderNumber);
        var repairInvoiceNumber = NormalizeOptionalValue(request.RepairInvoiceNumber);

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("WarrantyStatus", FormatWarrantyStatus(rmaRecord.WarrantyStatus), FormatWarrantyStatus(request.WarrantyStatus)),
                new RmaAuditChange("WarrantyExpiryDate", FormatDate(rmaRecord.WarrantyExpiryDate), FormatDate(request.WarrantyExpiryDate)),
                new RmaAuditChange("ChargeableRepair", FormatBool(rmaRecord.ChargeableRepair), FormatBool(request.ChargeableRepair)),
                new RmaAuditChange("CustomerApprovalRequired", FormatBool(rmaRecord.CustomerApprovalRequired), FormatBool(request.CustomerApprovalRequired)),
                new RmaAuditChange("CustomerApprovalReceived", FormatBool(rmaRecord.CustomerApprovalReceived), FormatBool(request.CustomerApprovalReceived)),
                new RmaAuditChange("CustomerApprovalDate", FormatDate(rmaRecord.CustomerApprovalDate), FormatDate(request.CustomerApprovalDate)),
                new RmaAuditChange("QuoteNumber", rmaRecord.QuoteNumber, quoteNumber),
                new RmaAuditChange("PurchaseOrderNumber", rmaRecord.PurchaseOrderNumber, purchaseOrderNumber),
                new RmaAuditChange("RepairInvoiceNumber", rmaRecord.RepairInvoiceNumber, repairInvoiceNumber),
                new RmaAuditChange("EstimatedRepairCost", FormatCurrency(rmaRecord.EstimatedRepairCost), FormatCurrency(request.EstimatedRepairCost)),
                new RmaAuditChange("ActualRepairCost", FormatCurrency(rmaRecord.ActualRepairCost), FormatCurrency(request.ActualRepairCost))
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return RmaOperationResult.Success();
        }

        rmaRecord.WarrantyStatus = request.WarrantyStatus;
        rmaRecord.WarrantyExpiryDate = request.WarrantyExpiryDate;
        rmaRecord.ChargeableRepair = request.ChargeableRepair;
        rmaRecord.CustomerApprovalRequired = request.CustomerApprovalRequired;
        rmaRecord.CustomerApprovalReceived = request.CustomerApprovalReceived;
        rmaRecord.CustomerApprovalDate = request.CustomerApprovalDate;
        rmaRecord.QuoteNumber = quoteNumber;
        rmaRecord.PurchaseOrderNumber = purchaseOrderNumber;
        rmaRecord.RepairInvoiceNumber = repairInvoiceNumber;
        rmaRecord.EstimatedRepairCost = request.EstimatedRepairCost;
        rmaRecord.ActualRepairCost = request.ActualRepairCost;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RmaOperationResult.Success();
    }

    public async Task<UpdateRmaWorkflowResult> UpdateWorkflowAsync(
        int rmaRecordId,
        UpdateRmaWorkflowRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return UpdateRmaWorkflowResult.Failure("RMA Record was not found.");
        }

        var assignedTo = NormalizeOptionalValue(request.AssignedTo);
        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("AssignedTo", rmaRecord.AssignedTo, assignedTo),
                new RmaAuditChange("Priority", FormatPriority(rmaRecord.Priority), FormatPriority(request.Priority)),
                new RmaAuditChange("DueDate", FormatDate(rmaRecord.DueDate), FormatDate(request.DueDate)),
                new RmaAuditChange("TargetCompletionDate", FormatDate(rmaRecord.TargetCompletionDate), FormatDate(request.TargetCompletionDate))
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateRmaWorkflowResult.Success();
        }

        rmaRecord.AssignedTo = assignedTo;
        rmaRecord.Priority = request.Priority;
        rmaRecord.DueDate = request.DueDate;
        rmaRecord.TargetCompletionDate = request.TargetCompletionDate;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateRmaWorkflowResult.Success();
    }

    public async Task<RmaOperationResult> UpdateShippingAsync(
        int rmaRecordId,
        UpdateRmaShippingRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        var returnMethod = NormalizeOptionalValue(request.ReturnMethod);
        var courier = NormalizeOptionalValue(request.Courier);
        var trackingNumber = NormalizeOptionalValue(request.TrackingNumber);
        var shippedBy = NormalizeOptionalValue(request.ShippedBy);
        var returnAddress = NormalizeOptionalValue(request.ReturnAddress);
        var shippingNotes = NormalizeOptionalValue(request.ShippingNotes);

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("ReturnMethod", rmaRecord.ReturnMethod, returnMethod),
                new RmaAuditChange("Courier", rmaRecord.Courier, courier),
                new RmaAuditChange("TrackingNumber", rmaRecord.TrackingNumber, trackingNumber),
                new RmaAuditChange("CollectionArranged", FormatBool(rmaRecord.CollectionArranged), FormatBool(request.CollectionArranged)),
                new RmaAuditChange("CollectionDate", FormatDate(rmaRecord.CollectionDate), FormatDate(request.CollectionDate)),
                new RmaAuditChange("ShippedDate", FormatDate(rmaRecord.ShippedDate), FormatDate(request.ShippedDate)),
                new RmaAuditChange("ShippedBy", rmaRecord.ShippedBy, shippedBy),
                new RmaAuditChange("ReturnAddress", rmaRecord.ReturnAddress, returnAddress),
                new RmaAuditChange("ShippingNotes", rmaRecord.ShippingNotes, shippingNotes),
                new RmaAuditChange("ProofOfDeliveryReceived", FormatBool(rmaRecord.ProofOfDeliveryReceived), FormatBool(request.ProofOfDeliveryReceived)),
                new RmaAuditChange("ProofOfDeliveryDate", FormatDate(rmaRecord.ProofOfDeliveryDate), FormatDate(request.ProofOfDeliveryDate))
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return RmaOperationResult.Success();
        }

        rmaRecord.ReturnMethod = returnMethod;
        rmaRecord.Courier = courier;
        rmaRecord.TrackingNumber = trackingNumber;
        rmaRecord.CollectionArranged = request.CollectionArranged;
        rmaRecord.CollectionDate = request.CollectionDate;
        rmaRecord.ShippedDate = request.ShippedDate;
        rmaRecord.ShippedBy = shippedBy;
        rmaRecord.ReturnAddress = returnAddress;
        rmaRecord.ShippingNotes = shippingNotes;
        rmaRecord.ProofOfDeliveryReceived = request.ProofOfDeliveryReceived;
        rmaRecord.ProofOfDeliveryDate = request.ProofOfDeliveryDate;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RmaOperationResult.Success();
    }

    public async Task<RmaOperationResult> UpdateCustomerSummaryAsync(
        int rmaRecordId,
        UpdateRmaCustomerSummaryRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        var customerFacingSummary = NormalizeOptionalValue(request.CustomerFacingSummary);
        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [new RmaAuditChange("CustomerFacingSummary", rmaRecord.CustomerFacingSummary, customerFacingSummary)],
            userName);

        if (auditEntries.Count == 0)
        {
            return RmaOperationResult.Success();
        }

        rmaRecord.CustomerFacingSummary = customerFacingSummary;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RmaOperationResult.Success();
    }

    public async Task<UpdateRmaTestingQaResult> UpdateTestingQaAsync(
        int rmaRecordId,
        UpdateRmaTestingQaRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return UpdateRmaTestingQaResult.Failure("RMA Record was not found.");
        }

        var testPlanUsed = NormalizeOptionalValue(request.TestPlanUsed);
        var testedBy = NormalizeOptionalValue(request.TestedBy);
        var testNotes = NormalizeOptionalValue(request.TestNotes);
        var qaCheckedBy = NormalizeOptionalValue(request.QaCheckedBy);
        var releaseApprovedBy = NormalizeOptionalValue(request.ReleaseApprovedBy);
        var releaseApprovedAt = request.ReleaseApproved == true
            ? (rmaRecord.ReleaseApprovedAt ?? DateTimeOffset.UtcNow)
            : (DateTimeOffset?)null;

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("TestRequired", FormatBool(rmaRecord.TestRequired), FormatBool(request.TestRequired)),
                new RmaAuditChange("TestPlanUsed", rmaRecord.TestPlanUsed, testPlanUsed),
                new RmaAuditChange("TestResult", FormatTestResult(rmaRecord.TestResult), FormatTestResult(request.TestResult)),
                new RmaAuditChange("TestedBy", rmaRecord.TestedBy, testedBy),
                new RmaAuditChange("TestDate", FormatDate(rmaRecord.TestDate), FormatDate(request.TestDate)),
                new RmaAuditChange("TestNotes", rmaRecord.TestNotes, testNotes),
                new RmaAuditChange("QaRequired", FormatBool(rmaRecord.QaRequired), FormatBool(request.QaRequired)),
                new RmaAuditChange("QaResult", FormatQaResult(rmaRecord.QaResult), FormatQaResult(request.QaResult)),
                new RmaAuditChange("QaCheckedBy", rmaRecord.QaCheckedBy, qaCheckedBy),
                new RmaAuditChange("QaDate", FormatDate(rmaRecord.QaDate), FormatDate(request.QaDate)),
                new RmaAuditChange("ReleaseApproved", FormatBool(rmaRecord.ReleaseApproved), FormatBool(request.ReleaseApproved)),
                new RmaAuditChange("ReleaseApprovedBy", rmaRecord.ReleaseApprovedBy, releaseApprovedBy),
                new RmaAuditChange("ReleaseApprovedAt", FormatDateTimeOffset(rmaRecord.ReleaseApprovedAt), FormatDateTimeOffset(releaseApprovedAt))
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateRmaTestingQaResult.Success();
        }

        rmaRecord.TestRequired = request.TestRequired;
        rmaRecord.TestPlanUsed = testPlanUsed;
        rmaRecord.TestResult = request.TestResult;
        rmaRecord.TestedBy = testedBy;
        rmaRecord.TestDate = request.TestDate;
        rmaRecord.TestNotes = testNotes;
        rmaRecord.QaRequired = request.QaRequired;
        rmaRecord.QaResult = request.QaResult;
        rmaRecord.QaCheckedBy = qaCheckedBy;
        rmaRecord.QaDate = request.QaDate;
        rmaRecord.ReleaseApproved = request.ReleaseApproved;
        rmaRecord.ReleaseApprovedBy = releaseApprovedBy;
        rmaRecord.ReleaseApprovedAt = releaseApprovedAt;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateRmaTestingQaResult.Success();
    }

    public async Task<SaveRmaPartResult> SavePartAsync(
        int rmaRecordId,
        SaveRmaPartRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.PartName))
        {
            return SaveRmaPartResult.Failure("Part name is required.");
        }

        if (request.Quantity < 1)
        {
            return SaveRmaPartResult.Failure("Quantity must be at least 1.");
        }

        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return SaveRmaPartResult.Failure("RMA Record was not found.");
        }

        var partName = request.PartName.Trim();
        var partNumber = NormalizeOptionalValue(request.PartNumber);
        var serialNumber = NormalizeOptionalValue(request.SerialNumber);
        var supplier = NormalizeOptionalValue(request.Supplier);
        var notes = NormalizeOptionalValue(request.Notes);
        var quantityText = request.Quantity.ToString();
        var unitCostText = FormatCurrency(request.UnitCost);
        RmaPart? part;
        List<RmaAuditChange> changes;

        if (request.PartId is null)
        {
            part = new RmaPart
            {
                RmaRecordId = rmaRecordId,
                PartName = partName,
                PartNumber = partNumber,
                Quantity = request.Quantity,
                SerialNumber = serialNumber,
                Supplier = supplier,
                UnitCost = request.UnitCost,
                Notes = notes
            };

            dbContext.RmaParts.Add(part);
            changes =
            [
                new RmaAuditChange("PartAdded", null, $"{partName} x{quantityText}")
            ];
        }
        else
        {
            part = await dbContext.RmaParts
                .SingleOrDefaultAsync(
                    existingPart => existingPart.Id == request.PartId.Value && existingPart.RmaRecordId == rmaRecordId,
                    cancellationToken);

            if (part is null)
            {
                return SaveRmaPartResult.Failure("Part was not found.");
            }

            changes =
            [
                new RmaAuditChange($"Part:{part.Id}:PartName", part.PartName, partName),
                new RmaAuditChange($"Part:{part.Id}:PartNumber", part.PartNumber, partNumber),
                new RmaAuditChange($"Part:{part.Id}:Quantity", part.Quantity.ToString(), quantityText),
                new RmaAuditChange($"Part:{part.Id}:SerialNumber", part.SerialNumber, serialNumber),
                new RmaAuditChange($"Part:{part.Id}:Supplier", part.Supplier, supplier),
                new RmaAuditChange($"Part:{part.Id}:UnitCost", FormatCurrency(part.UnitCost), unitCostText),
                new RmaAuditChange($"Part:{part.Id}:Notes", part.Notes, notes)
            ];

            part.PartName = partName;
            part.PartNumber = partNumber;
            part.Quantity = request.Quantity;
            part.SerialNumber = serialNumber;
            part.Supplier = supplier;
            part.UnitCost = request.UnitCost;
            part.Notes = notes;
        }

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(rmaRecord, changes, userName);

        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return SaveRmaPartResult.Success(part.Id);
    }

    public async Task<SaveRmaPartResult> DeletePartAsync(
        int rmaRecordId,
        int partId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return SaveRmaPartResult.Failure("RMA Record was not found.");
        }

        var part = await dbContext.RmaParts
            .SingleOrDefaultAsync(existingPart => existingPart.Id == partId && existingPart.RmaRecordId == rmaRecordId, cancellationToken);

        if (part is null)
        {
            return SaveRmaPartResult.Failure("Part was not found.");
        }

        dbContext.RmaParts.Remove(part);
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(
            rmaAuditService.CreateRecordUpdatedEntries(
                rmaRecord,
                [new RmaAuditChange("PartRemoved", $"{part.PartName} x{part.Quantity}", null)],
                userName),
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return SaveRmaPartResult.Success();
    }

    public async Task<UpdateRmaChecklistResult> UpdateChecklistItemAsync(
        int rmaRecordId,
        UpdateRmaChecklistItemRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return UpdateRmaChecklistResult.Failure("RMA Record was not found.");
        }

        if (request.ChecklistItemId is null)
        {
            var text = NormalizeOptionalValue(request.Text);
            if (text is null)
            {
                return UpdateRmaChecklistResult.Failure("Checklist item text is required.");
            }

            var maxDisplayOrder = await dbContext.RmaChecklistItems
                .Where(item => item.RmaRecordId == rmaRecordId)
                .Select(item => (int?)item.DisplayOrder)
                .MaxAsync(cancellationToken)
                ?? 0;

            dbContext.RmaChecklistItems.Add(new RmaChecklistItem
            {
                RmaRecordId = rmaRecordId,
                DisplayOrder = maxDisplayOrder + 1,
                Text = text,
                ShowInBoardView = false
            });

            rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
            rmaRecord.LastUpdatedBy = userName;

            await dbContext.RmaAudit.AddRangeAsync(
                rmaAuditService.CreateRecordUpdatedEntries(
                    rmaRecord,
                    [new RmaAuditChange("ChecklistItemAdded", null, text)],
                    userName),
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return UpdateRmaChecklistResult.Success();
        }

        var checklistItem = await dbContext.RmaChecklistItems
            .SingleOrDefaultAsync(item => item.Id == request.ChecklistItemId.Value && item.RmaRecordId == rmaRecordId, cancellationToken);

        if (checklistItem is null)
        {
            return UpdateRmaChecklistResult.Failure("Checklist item was not found.");
        }

        if (request.IsCompleted is null)
        {
            return UpdateRmaChecklistResult.Failure("Checklist completion state was not provided.");
        }

        var completedBy = request.IsCompleted.Value ? userName : null;
        var completedAt = request.IsCompleted.Value ? DateTimeOffset.UtcNow : (DateTimeOffset?)null;

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange($"Checklist:{checklistItem.Id}:Completed", FormatBool(checklistItem.IsCompleted), FormatBool(request.IsCompleted.Value)),
                new RmaAuditChange($"Checklist:{checklistItem.Id}:CompletedBy", checklistItem.CompletedBy, completedBy),
                new RmaAuditChange($"Checklist:{checklistItem.Id}:CompletedAt", FormatDateTimeOffset(checklistItem.CompletedAt), FormatDateTimeOffset(completedAt))
            ],
            userName);

        checklistItem.IsCompleted = request.IsCompleted.Value;
        checklistItem.CompletedBy = completedBy;
        checklistItem.CompletedAt = completedAt;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateRmaChecklistResult.Success();
    }

    public async Task<RmaOperationResult> SaveNoteAsync(
        int rmaRecordId,
        SaveRmaNoteRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NoteText))
        {
            return RmaOperationResult.Failure("Note text is required.");
        }

        var userName = NormalizeUserName(updatedBy);
        var noteText = request.NoteText.Trim();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        if (request.NoteId is null)
        {
            var note = new RmaNote
            {
                RmaRecordId = rmaRecordId,
                NoteType = request.NoteType,
                NoteText = noteText,
                CreatedBy = userName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            dbContext.RmaNotes.Add(note);
            rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
            rmaRecord.LastUpdatedBy = userName;
            await dbContext.SaveChangesAsync(cancellationToken);

            var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
                rmaRecord,
                [
                    new RmaAuditChange($"Note:{note.Id}:Type", null, note.NoteType.ToString()),
                    new RmaAuditChange($"Note:{note.Id}:Text", null, note.NoteText)
                ],
                userName);

            if (auditEntries.Count > 0)
            {
                await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return RmaOperationResult.Success();
        }

        var existingNote = await dbContext.RmaNotes
            .SingleOrDefaultAsync(note => note.Id == request.NoteId.Value && note.RmaRecordId == rmaRecordId, cancellationToken);

        if (existingNote is null)
        {
            return RmaOperationResult.Failure("Note was not found.");
        }

        var auditEntriesForUpdate = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange($"Note:{existingNote.Id}:Type", existingNote.NoteType.ToString(), request.NoteType.ToString()),
                new RmaAuditChange($"Note:{existingNote.Id}:Text", existingNote.NoteText, noteText)
            ],
            userName);

        if (auditEntriesForUpdate.Count == 0)
        {
            return RmaOperationResult.Success();
        }

        existingNote.NoteType = request.NoteType;
        existingNote.NoteText = noteText;
        existingNote.LastUpdatedBy = userName;
        existingNote.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntriesForUpdate, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RmaOperationResult.Success();
    }

    public async Task<RmaOperationResult> DeleteNoteAsync(
        int rmaRecordId,
        int noteId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);
        var note = await dbContext.RmaNotes
            .SingleOrDefaultAsync(item => item.Id == noteId && item.RmaRecordId == rmaRecordId, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        if (note is null)
        {
            return RmaOperationResult.Failure("Note was not found.");
        }

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange($"Note:{note.Id}:Type", note.NoteType.ToString(), null),
                new RmaAuditChange($"Note:{note.Id}:Text", note.NoteText, null)
            ],
            userName);

        dbContext.RmaNotes.Remove(note);
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        if (auditEntries.Count > 0)
        {
            await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return RmaOperationResult.Success();
    }

    public async Task<RmaOperationResult> SaveCommunicationAsync(
        int rmaRecordId,
        SaveRmaCommunicationRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (request.CommunicationDate is null)
        {
            return RmaOperationResult.Failure("Communication date is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ContactMethod))
        {
            return RmaOperationResult.Failure("Contact method is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Summary))
        {
            return RmaOperationResult.Failure("Communication summary is required.");
        }

        if (request.FollowUpRequired && request.FollowUpDate is null)
        {
            return RmaOperationResult.Failure("Follow-up date is required when follow-up is marked as required.");
        }

        var userName = NormalizeUserName(updatedBy);
        var contactMethod = request.ContactMethod.Trim();
        var contactPerson = NormalizeOptionalValue(request.ContactPerson);
        var summary = request.Summary.Trim();
        var communicationDate = new DateTimeOffset(
            request.CommunicationDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        if (request.CommunicationId is null)
        {
            var communication = new RmaCommunication
            {
                RmaRecordId = rmaRecordId,
                CommunicationDate = communicationDate,
                ContactMethod = contactMethod,
                ContactPerson = contactPerson,
                Summary = summary,
                FollowUpRequired = request.FollowUpRequired,
                FollowUpDate = request.FollowUpDate,
                CreatedBy = userName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            dbContext.RmaCommunications.Add(communication);
            rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
            rmaRecord.LastUpdatedBy = userName;
            await dbContext.SaveChangesAsync(cancellationToken);

            var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
                rmaRecord,
                [
                    new RmaAuditChange($"Communication:{communication.Id}:Date", null, FormatDateTimeOffset(communication.CommunicationDate)),
                    new RmaAuditChange($"Communication:{communication.Id}:Method", null, communication.ContactMethod),
                    new RmaAuditChange($"Communication:{communication.Id}:ContactPerson", null, communication.ContactPerson),
                    new RmaAuditChange($"Communication:{communication.Id}:Summary", null, communication.Summary),
                    new RmaAuditChange($"Communication:{communication.Id}:FollowUpRequired", null, FormatBool(communication.FollowUpRequired)),
                    new RmaAuditChange($"Communication:{communication.Id}:FollowUpDate", null, FormatDate(communication.FollowUpDate))
                ],
                userName);

            if (auditEntries.Count > 0)
            {
                await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return RmaOperationResult.Success();
        }

        var existingCommunication = await dbContext.RmaCommunications
            .SingleOrDefaultAsync(
                communication => communication.Id == request.CommunicationId.Value && communication.RmaRecordId == rmaRecordId,
                cancellationToken);

        if (existingCommunication is null)
        {
            return RmaOperationResult.Failure("Communication entry was not found.");
        }

        var auditEntriesForUpdate = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange($"Communication:{existingCommunication.Id}:Date", FormatDateTimeOffset(existingCommunication.CommunicationDate), FormatDateTimeOffset(communicationDate)),
                new RmaAuditChange($"Communication:{existingCommunication.Id}:Method", existingCommunication.ContactMethod, contactMethod),
                new RmaAuditChange($"Communication:{existingCommunication.Id}:ContactPerson", existingCommunication.ContactPerson, contactPerson),
                new RmaAuditChange($"Communication:{existingCommunication.Id}:Summary", existingCommunication.Summary, summary),
                new RmaAuditChange($"Communication:{existingCommunication.Id}:FollowUpRequired", FormatBool(existingCommunication.FollowUpRequired), FormatBool(request.FollowUpRequired)),
                new RmaAuditChange($"Communication:{existingCommunication.Id}:FollowUpDate", FormatDate(existingCommunication.FollowUpDate), FormatDate(request.FollowUpDate))
            ],
            userName);

        if (auditEntriesForUpdate.Count == 0)
        {
            return RmaOperationResult.Success();
        }

        existingCommunication.CommunicationDate = communicationDate;
        existingCommunication.ContactMethod = contactMethod;
        existingCommunication.ContactPerson = contactPerson;
        existingCommunication.Summary = summary;
        existingCommunication.FollowUpRequired = request.FollowUpRequired;
        existingCommunication.FollowUpDate = request.FollowUpDate;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntriesForUpdate, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RmaOperationResult.Success();
    }

    public async Task<RmaOperationResult> DeleteCommunicationAsync(
        int rmaRecordId,
        int communicationId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);
        var communication = await dbContext.RmaCommunications
            .SingleOrDefaultAsync(item => item.Id == communicationId && item.RmaRecordId == rmaRecordId, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        if (communication is null)
        {
            return RmaOperationResult.Failure("Communication entry was not found.");
        }

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange($"Communication:{communication.Id}:Date", FormatDateTimeOffset(communication.CommunicationDate), null),
                new RmaAuditChange($"Communication:{communication.Id}:Method", communication.ContactMethod, null),
                new RmaAuditChange($"Communication:{communication.Id}:ContactPerson", communication.ContactPerson, null),
                new RmaAuditChange($"Communication:{communication.Id}:Summary", communication.Summary, null)
            ],
            userName);

        dbContext.RmaCommunications.Remove(communication);
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        if (auditEntries.Count > 0)
        {
            await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return RmaOperationResult.Success();
    }

    public async Task<RmaOperationResult> SaveAttachmentAsync(
        int rmaRecordId,
        SaveRmaAttachmentRequest request,
        Stream content,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return RmaOperationResult.Failure("A file is required.");
        }

        if (string.IsNullOrWhiteSpace(request.AttachmentType))
        {
            return RmaOperationResult.Failure("Attachment type is required.");
        }

        var userName = NormalizeUserName(updatedBy);
        var fileName = Path.GetFileName(request.FileName);
        var contentType = string.IsNullOrWhiteSpace(request.ContentType)
            ? "application/octet-stream"
            : request.ContentType.Trim();
        var attachmentType = request.AttachmentType.Trim();
        var description = NormalizeOptionalValue(request.Description);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        var storedFilePath = await rmaAttachmentStorage.SaveAsync(rmaRecordId, fileName, content, cancellationToken);
        var attachment = new RmaAttachment
        {
            RmaRecordId = rmaRecordId,
            FileName = fileName,
            StoredFilePath = storedFilePath,
            ContentType = contentType,
            AttachmentType = attachmentType,
            Description = description,
            UploadedBy = userName,
            UploadedAt = DateTimeOffset.UtcNow
        };

        dbContext.RmaAttachments.Add(attachment);
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;
        await dbContext.SaveChangesAsync(cancellationToken);

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange($"Attachment:{attachment.Id}:FileName", null, attachment.FileName),
                new RmaAuditChange($"Attachment:{attachment.Id}:Type", null, attachment.AttachmentType),
                new RmaAuditChange($"Attachment:{attachment.Id}:Description", null, attachment.Description)
            ],
            userName);

        if (auditEntries.Count > 0)
        {
            await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return RmaOperationResult.Success();
    }

    public async Task<RmaOperationResult> DeleteAttachmentAsync(
        int rmaRecordId,
        int attachmentId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);
        var attachment = await dbContext.RmaAttachments
            .SingleOrDefaultAsync(item => item.Id == attachmentId && item.RmaRecordId == rmaRecordId, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaOperationResult.Failure("RMA Record was not found.");
        }

        if (attachment is null)
        {
            return RmaOperationResult.Failure("Attachment was not found.");
        }

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange($"Attachment:{attachment.Id}:FileName", attachment.FileName, null),
                new RmaAuditChange($"Attachment:{attachment.Id}:Type", attachment.AttachmentType, null),
                new RmaAuditChange($"Attachment:{attachment.Id}:Description", attachment.Description, null)
            ],
            userName);

        dbContext.RmaAttachments.Remove(attachment);
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        if (auditEntries.Count > 0)
        {
            await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await rmaAttachmentStorage.DeleteAsync(attachment.StoredFilePath, cancellationToken);

        return RmaOperationResult.Success();
    }

    public async Task<ChangeRmaStatusResult> ChangeStatusAsync(
        int rmaRecordId,
        ChangeRmaStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return ChangeRmaStatusResult.Failure("RMA Record was not found.");
        }

        var validationResult = rmaStatusTransitionService.Validate(rmaRecord.Status, request);
        if (!validationResult.Succeeded)
        {
            return validationResult;
        }

        if (request.NewStatus == RmaStatus.ReadyToShip && !request.IgnoreReadinessWarnings)
        {
            var readinessWarnings = await BuildReadyToShipWarningsAsync(dbContext, rmaRecordId, rmaRecord, cancellationToken);
            if (readinessWarnings.Count > 0)
            {
                return ChangeRmaStatusResult.WarningConfirmationRequired(readinessWarnings);
            }
        }

        var oldStatus = rmaRecord.Status;
        var now = DateTimeOffset.UtcNow;
        var reason = NormalizeOptionalValue(request.Reason);
        var onHoldReason = request.NewStatus == RmaStatus.OnHold
            ? NormalizeOptionalValue(request.OnHoldReason)
            : null;
        var closureNotes = request.NewStatus == RmaStatus.Closed
            ? NormalizeOptionalValue(request.ClosureNotes)
            : rmaRecord.ClosureNotes;
        var outcome = request.NewStatus == RmaStatus.Closed
            ? request.Outcome
            : rmaRecord.Outcome;
        var closedAt = request.NewStatus == RmaStatus.Closed ? now : rmaRecord.ClosedAt;
        var closedBy = request.NewStatus == RmaStatus.Closed ? userName : rmaRecord.ClosedBy;
        var shippedDate = request.NewStatus == RmaStatus.Shipped && rmaRecord.ShippedDate is null
            ? DateOnly.FromDateTime(now.UtcDateTime)
            : rmaRecord.ShippedDate;

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [
                new RmaAuditChange("Status", rmaRecord.Status.ToString(), request.NewStatus.ToString()),
                new RmaAuditChange("OnHoldReason", rmaRecord.OnHoldReason, onHoldReason),
                new RmaAuditChange("Outcome", FormatOutcome(rmaRecord.Outcome), FormatOutcome(outcome)),
                new RmaAuditChange("ClosureNotes", rmaRecord.ClosureNotes, closureNotes),
                new RmaAuditChange("ClosedAt", FormatDateTimeOffset(rmaRecord.ClosedAt), FormatDateTimeOffset(closedAt)),
                new RmaAuditChange("ClosedBy", rmaRecord.ClosedBy, closedBy),
                new RmaAuditChange("ShippedDate", FormatDate(rmaRecord.ShippedDate), FormatDate(shippedDate))
            ],
            userName);

        rmaRecord.Status = request.NewStatus;
        rmaRecord.OnHoldReason = onHoldReason;
        rmaRecord.Outcome = outcome;
        rmaRecord.ClosureNotes = closureNotes;
        rmaRecord.ClosedAt = closedAt;
        rmaRecord.ClosedBy = closedBy;
        rmaRecord.ShippedDate = shippedDate;
        rmaRecord.LastUpdatedAt = now;
        rmaRecord.LastUpdatedBy = userName;

        dbContext.RmaStatusHistory.Add(new RmaStatusHistory
        {
            RmaRecord = rmaRecord,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus,
            ChangedBy = userName,
            ChangedAt = now,
            Reason = reason
        });

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ChangeRmaStatusResult.Success();
    }

    public async Task<RmaLinkResult> LinkBuildRecordAsync(
        int rmaRecordId,
        int buildRecordId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .Include(record => record.BuildRecord)
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaLinkResult.Failure("RMA Record was not found.");
        }

        var buildRecord = await dbContext.BuildRecords
            .Include(record => record.Customer)
            .SingleOrDefaultAsync(record => record.Id == buildRecordId && record.IsActive, cancellationToken);

        if (buildRecord is null)
        {
            return RmaLinkResult.Failure("Build Record was not found.");
        }

        if (rmaRecord.BuildRecordId == buildRecordId)
        {
            return RmaLinkResult.Success();
        }

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [new RmaAuditChange("BuildRecordId", rmaRecord.BuildRecordId?.ToString(), buildRecordId.ToString())],
            userName);

        rmaRecord.BuildRecord = buildRecord;
        rmaRecord.BuildRecordId = buildRecordId;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RmaLinkResult.Success();
    }

    public async Task<RmaLinkResult> UnlinkBuildRecordAsync(
        int rmaRecordId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(updatedBy);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var rmaRecord = await dbContext.RmaRecords
            .Include(record => record.BuildRecord)
            .SingleOrDefaultAsync(record => record.Id == rmaRecordId && record.IsActive, cancellationToken);

        if (rmaRecord is null)
        {
            return RmaLinkResult.Failure("RMA Record was not found.");
        }

        if (rmaRecord.BuildRecordId is null)
        {
            return RmaLinkResult.Success();
        }

        var auditEntries = rmaAuditService.CreateRecordUpdatedEntries(
            rmaRecord,
            [new RmaAuditChange("BuildRecordId", rmaRecord.BuildRecordId.ToString(), null)],
            userName);

        rmaRecord.BuildRecord = null;
        rmaRecord.BuildRecordId = null;
        rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        rmaRecord.LastUpdatedBy = userName;

        await dbContext.RmaAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RmaLinkResult.Success();
    }

    private static async Task<Domain.BuildRecords.BuildRecord?> ResolveLinkedBuildRecordAsync(
        BuildBookDbContext dbContext,
        int? buildRecordId,
        CancellationToken cancellationToken)
    {
        if (buildRecordId is null)
        {
            return null;
        }

        return await dbContext.BuildRecords
            .SingleOrDefaultAsync(record => record.Id == buildRecordId && record.IsActive, cancellationToken);
    }

    private static IQueryable<RmaRecord> ApplySorting(IQueryable<RmaRecord> query, RmaRegisterFilter? filter)
    {
        var sortBy = filter?.SortBy ?? RmaRegisterSortColumn.LastUpdated;
        var sortDescending = filter?.SortDescending ?? true;

        IOrderedQueryable<RmaRecord> orderedQuery = (sortBy, sortDescending) switch
        {
            (RmaRegisterSortColumn.RmaNumber, false) => query.OrderBy(rmaRecord => rmaRecord.RmaNumber),
            (RmaRegisterSortColumn.RmaNumber, true) => query.OrderByDescending(rmaRecord => rmaRecord.RmaNumber),
            (RmaRegisterSortColumn.Status, false) => query.OrderBy(rmaRecord => rmaRecord.Status),
            (RmaRegisterSortColumn.Status, true) => query.OrderByDescending(rmaRecord => rmaRecord.Status),
            (RmaRegisterSortColumn.Customer, false) => query.OrderBy(rmaRecord => rmaRecord.Customer == null ? null : rmaRecord.Customer.Name),
            (RmaRegisterSortColumn.Customer, true) => query.OrderByDescending(rmaRecord => rmaRecord.Customer == null ? null : rmaRecord.Customer.Name),
            (RmaRegisterSortColumn.ProductName, false) => query.OrderBy(rmaRecord => rmaRecord.ProductName),
            (RmaRegisterSortColumn.ProductName, true) => query.OrderByDescending(rmaRecord => rmaRecord.ProductName),
            (RmaRegisterSortColumn.SerialNumber, false) => query.OrderBy(rmaRecord => rmaRecord.SerialNumber),
            (RmaRegisterSortColumn.SerialNumber, true) => query.OrderByDescending(rmaRecord => rmaRecord.SerialNumber),
            (RmaRegisterSortColumn.FaultSummary, false) => query.OrderBy(rmaRecord => rmaRecord.FaultSummary),
            (RmaRegisterSortColumn.FaultSummary, true) => query.OrderByDescending(rmaRecord => rmaRecord.FaultSummary),
            (RmaRegisterSortColumn.Priority, false) => query.OrderBy(rmaRecord => rmaRecord.Priority),
            (RmaRegisterSortColumn.Priority, true) => query.OrderByDescending(rmaRecord => rmaRecord.Priority),
            (RmaRegisterSortColumn.AssignedTo, false) => query.OrderBy(rmaRecord => rmaRecord.AssignedTo),
            (RmaRegisterSortColumn.AssignedTo, true) => query.OrderByDescending(rmaRecord => rmaRecord.AssignedTo),
            (RmaRegisterSortColumn.DueDate, false) => query.OrderBy(rmaRecord => rmaRecord.DueDate),
            (RmaRegisterSortColumn.DueDate, true) => query.OrderByDescending(rmaRecord => rmaRecord.DueDate),
            (RmaRegisterSortColumn.LinkedBuildRecord, false) => query.OrderBy(rmaRecord => rmaRecord.BuildRecordId != null),
            (RmaRegisterSortColumn.LinkedBuildRecord, true) => query.OrderByDescending(rmaRecord => rmaRecord.BuildRecordId != null),
            (RmaRegisterSortColumn.LastUpdated, false) => query.OrderBy(rmaRecord => rmaRecord.LastUpdatedAt),
            _ => query.OrderByDescending(rmaRecord => rmaRecord.LastUpdatedAt)
        };

        return orderedQuery.ThenBy(rmaRecord => rmaRecord.RmaNumber);
    }

    private static IReadOnlyList<string> BuildMatchReasons(
        string serialNumber,
        string productCode,
        string productName,
        string? customerName,
        string? requestedSerialNumber,
        string? requestedProductCode,
        string? requestedProductName,
        string? requestedCustomerName)
    {
        var reasons = new List<string>();

        if (requestedSerialNumber is not null
            && string.Equals(serialNumber, requestedSerialNumber, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Serial number match");
        }

        if (requestedProductCode is not null
            && string.Equals(productCode, requestedProductCode, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Product code match");
        }

        if (requestedProductName is not null
            && string.Equals(productName, requestedProductName, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Product name match");
        }

        if (requestedCustomerName is not null
            && customerName is not null
            && string.Equals(customerName, requestedCustomerName, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Customer match");
        }

        return reasons.Count == 0 ? ["Possible related Build Record"] : reasons;
    }

    private static string CreateLikePattern(string value)
    {
        return $"%{EscapeLikePattern(value.Trim())}%";
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal)
            .Replace("[", @"\[", StringComparison.Ordinal);
    }

    private static async Task<Customer> GetOrCreateCustomerAsync(
        BuildBookDbContext dbContext,
        string customerName,
        string userName,
        int? preferredCustomerId,
        CancellationToken cancellationToken)
    {
        var normalizedCustomerName = customerName.Trim();

        if (preferredCustomerId is not null)
        {
            var preferredCustomer = await dbContext.Customers
                .Include(customer => customer.SupportContractLevel)
                .FirstOrDefaultAsync(
                    customer => customer.IsActive
                        && customer.Id == preferredCustomerId.Value
                        && customer.Name == normalizedCustomerName,
                    cancellationToken);

            if (preferredCustomer is not null)
            {
                return preferredCustomer;
            }
        }

        // Some existing data contains duplicate active customers with the same name.
        // Prefer the earliest matching record instead of failing the whole save.
        var existingCustomer = await dbContext.Customers
            .Include(customer => customer.SupportContractLevel)
            .Where(customer => customer.IsActive && customer.Name == normalizedCustomerName)
            .OrderBy(customer => customer.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingCustomer is not null)
        {
            return existingCustomer;
        }

        var now = DateTimeOffset.UtcNow;
        var customer = new Customer
        {
            Name = normalizedCustomerName,
            CreatedAt = now,
            CreatedBy = userName,
            LastUpdatedAt = now,
            LastUpdatedBy = userName,
            IsActive = true
        };

        dbContext.Customers.Add(customer);
        return customer;
    }

    private static bool HasRepeatLink(
        int? buildRecordId,
        string? serialNumber,
        int? otherBuildRecordId,
        string? otherSerialNumber)
    {
        return (buildRecordId is not null && otherBuildRecordId == buildRecordId)
            || (serialNumber is not null
                && otherSerialNumber is not null
                && string.Equals(serialNumber, otherSerialNumber, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsOverdue(
        RmaStatus status,
        DateOnly? dueDate,
        DateOnly today)
    {
        return dueDate is not null
            && dueDate < today
            && status != RmaStatus.Closed
            && status != RmaStatus.CancelledNoReply
            && status != RmaStatus.CustomerFixed;
    }

    private static IReadOnlyList<string> BuildBoardWarnings(
        RmaStatus status,
        DateOnly today,
        DateOnly? dueDate,
        string? serialNumber,
        int? buildRecordId,
        string? repairActionTaken,
        DateOnly? repairCompletedDate,
        string? repairCompletedBy,
        bool? testRequired,
        RmaTestResult? testResult,
        bool? qaRequired,
        RmaQaResult? qaResult,
        bool? releaseApproved,
        bool? customerApprovalRequired,
        bool? customerApprovalReceived,
        IReadOnlyList<(string Text, bool IsCompleted)> checklistItems)
    {
        var warnings = new List<string>();

        if (IsOverdue(status, dueDate, today))
        {
            warnings.Add("Overdue");
        }

        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            warnings.Add("Serial number missing");
        }

        if (buildRecordId is null)
        {
            warnings.Add("No linked Build Record");
        }

        if (status == RmaStatus.ReadyToShip)
        {
            var incompleteChecklist = checklistItems
                .Where(item => !item.IsCompleted && !ReadyToShipDeferredChecklistItems.Contains(item.Text))
                .Select(item => item.Text)
                .ToList();

            if (incompleteChecklist.Count > 0)
            {
                warnings.Add("Checklist incomplete");
            }

            if (string.IsNullOrWhiteSpace(repairActionTaken))
            {
                warnings.Add("Repair action missing");
            }

            if (repairCompletedDate is null || string.IsNullOrWhiteSpace(repairCompletedBy))
            {
                warnings.Add("Repair completion missing");
            }

            if (testRequired == true && testResult != RmaTestResult.Pass)
            {
                warnings.Add("Required test not passed");
            }

            if (qaRequired == true && qaResult != RmaQaResult.Pass)
            {
                warnings.Add("QA sign-off missing");
            }

            if (releaseApproved != true)
            {
                warnings.Add("Release approval missing");
            }

            if (customerApprovalRequired == true && customerApprovalReceived != true)
            {
                warnings.Add("Customer approval missing");
            }
        }

        return warnings;
    }

    private static string NormalizeUserName(string userName)
    {
        return string.IsNullOrWhiteSpace(userName) ? "Unknown" : userName.Trim();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? FormatDate(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd");
    }

    private static string? FormatBool(bool? value)
    {
        return value?.ToString();
    }

    private static string? FormatDateTimeOffset(DateTimeOffset? value)
    {
        return value?.ToString("O");
    }

    private static string? FormatPriority(RmaPriority? value)
    {
        return value?.ToString();
    }

    private static string? FormatWarrantyStatus(RmaWarrantyStatus? value)
    {
        return value?.ToString();
    }

    private static string? FormatOutcome(RmaOutcome? value)
    {
        return value?.ToString();
    }

    private static string? FormatFaultCategory(RmaFaultCategory? value)
    {
        return value?.ToString();
    }

    private static string? FormatCustomerImpact(RmaCustomerImpact? value)
    {
        return value?.ToString();
    }

    private static string? FormatYesNoUnknown(RmaYesNoUnknown? value)
    {
        return value?.ToString();
    }

    private static string? FormatRootCauseCategory(RmaRootCauseCategory? value)
    {
        return value?.ToString();
    }

    private static string? FormatTestResult(RmaTestResult? value)
    {
        return value?.ToString();
    }

    private static string? FormatQaResult(RmaQaResult? value)
    {
        return value?.ToString();
    }

    private static string? FormatCurrency(decimal? value)
    {
        return value?.ToString("0.00");
    }

    private static async Task<IReadOnlyList<string>> BuildReadyToShipWarningsAsync(
        BuildBookDbContext dbContext,
        int rmaRecordId,
        RmaRecord rmaRecord,
        CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var checklistItems = await dbContext.RmaChecklistItems
            .AsNoTracking()
            .Where(item => item.RmaRecordId == rmaRecordId)
            .OrderBy(item => item.DisplayOrder)
            .ToListAsync(cancellationToken);

        var incompleteChecklist = checklistItems
            .Where(item => !item.IsCompleted && !ReadyToShipDeferredChecklistItems.Contains(item.Text))
            .Select(item => item.Text)
            .ToList();

        if (incompleteChecklist.Count > 0)
        {
            warnings.Add($"Checklist items still open: {string.Join(", ", incompleteChecklist)}.");
        }

        if (string.IsNullOrWhiteSpace(rmaRecord.RepairActionTaken))
        {
            warnings.Add("Repair action taken has not been recorded.");
        }

        if (rmaRecord.RepairCompletedDate is null || string.IsNullOrWhiteSpace(rmaRecord.RepairCompletedBy))
        {
            warnings.Add("Repair completion details are incomplete.");
        }

        if (rmaRecord.TestRequired is null)
        {
            warnings.Add("Test required has not been confirmed.");
        }
        else if (rmaRecord.TestRequired.Value && rmaRecord.TestResult != RmaTestResult.Pass)
        {
            warnings.Add("A required test has not passed.");
        }

        if (rmaRecord.QaRequired is null)
        {
            warnings.Add("QA required has not been confirmed.");
        }
        else if (rmaRecord.QaRequired.Value && rmaRecord.QaResult != RmaQaResult.Pass)
        {
            warnings.Add("Required QA sign-off is missing or not passed.");
        }

        if (rmaRecord.ReleaseApproved != true)
        {
            warnings.Add("Release approval has not been recorded.");
        }

        if (rmaRecord.CustomerApprovalRequired == true && rmaRecord.CustomerApprovalReceived != true)
        {
            warnings.Add("Customer approval is required but has not been marked as received.");
        }

        return warnings;
    }
}
