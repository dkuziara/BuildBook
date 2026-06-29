using System.Data;
using BuildBook.Application.Rmas;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public sealed class RmaRecordService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IRmaAuditService rmaAuditService,
    IRmaStatusTransitionService rmaStatusTransitionService) : IRmaRecordService
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
            ProductName = request.ProductName.Trim(),
            ProductCode = NormalizeOptionalValue(request.ProductCode),
            SerialNumber = NormalizeOptionalValue(request.SerialNumber),
            FaultSummary = request.FaultSummary.Trim(),
            InitialFaultDescription = request.InitialFaultDescription.Trim(),
            ContactName = NormalizeOptionalValue(request.ContactName),
            ContactEmail = NormalizeOptionalValue(request.ContactEmail),
            ContactPhone = NormalizeOptionalValue(request.ContactPhone),
            SupportTicketNumber = NormalizeOptionalValue(request.SupportTicketNumber),
            SupportTicketUrl = NormalizeOptionalValue(request.SupportTicketUrl),
            OriginalOrderNumber = NormalizeOptionalValue(request.OriginalOrderNumber),
            OriginalInvoiceNumber = NormalizeOptionalValue(request.OriginalInvoiceNumber),
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
        var supportTicketUrl = NormalizeOptionalValue(request.SupportTicketUrl);
        var originalOrderNumber = NormalizeOptionalValue(request.OriginalOrderNumber);
        var originalInvoiceNumber = NormalizeOptionalValue(request.OriginalInvoiceNumber);

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
                new RmaAuditChange("SupportTicketUrl", rmaRecord.SupportTicketUrl, supportTicketUrl),
                new RmaAuditChange("OriginalOrderNumber", rmaRecord.OriginalOrderNumber, originalOrderNumber),
                new RmaAuditChange("OriginalOrderDate", FormatDate(rmaRecord.OriginalOrderDate), FormatDate(request.OriginalOrderDate)),
                new RmaAuditChange("OriginalInvoiceNumber", rmaRecord.OriginalInvoiceNumber, originalInvoiceNumber)
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
        rmaRecord.SupportTicketUrl = supportTicketUrl;
        rmaRecord.OriginalOrderNumber = originalOrderNumber;
        rmaRecord.OriginalOrderDate = request.OriginalOrderDate;
        rmaRecord.OriginalInvoiceNumber = originalInvoiceNumber;
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
