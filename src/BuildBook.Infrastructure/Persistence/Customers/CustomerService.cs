using BuildBook.Application.Customers;
using BuildBook.Application.Rmas;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Customers;

public sealed class CustomerService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IRmaAuditService rmaAuditService) : ICustomerService, ICustomerListReader
{
    public Task<IReadOnlyList<CustomerListItem>> ListAsync(
        CustomerListFilter filter,
        CancellationToken cancellationToken = default)
    {
        return SearchAsync(filter, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerListItem>> SearchAsync(
        CustomerListFilter filter,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.Customers
            .AsNoTracking()
            .Include(customer => customer.SupportContractLevel)
            .AsQueryable();

        var search = NormalizeOptionalValue(filter.Search);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(customer =>
                customer.Name.Contains(search)
                || (customer.PrimaryContactName != null && customer.PrimaryContactName.Contains(search))
                || (customer.PrimaryContactEmail != null && customer.PrimaryContactEmail.Contains(search))
                || (customer.MainEmail != null && customer.MainEmail.Contains(search))
                || (customer.MainPhone != null && customer.MainPhone.Contains(search))
                || (customer.AccountCode != null && customer.AccountCode.Contains(search)));
        }

        if (filter.SupportContractLevelId is not null)
        {
            query = query.Where(customer => customer.SupportContractLevelId == filter.SupportContractLevelId);
        }

        if (!string.IsNullOrWhiteSpace(filter.SupportContractStatus))
        {
            query = query.Where(customer => customer.SupportContractStatus == filter.SupportContractStatus);
        }

        if (filter.IsActive is not null)
        {
            query = query.Where(customer => customer.IsActive == filter.IsActive);
        }

        query = ApplySort(query, filter);

        return await query
            .Select(customer => new CustomerListItem(
                customer.Id,
                customer.Name,
                customer.PrimaryContactName,
                customer.MainEmail,
                customer.MainPhone,
                customer.SupportContractLevel != null ? customer.SupportContractLevel.Name : null,
                customer.SupportContractStatus,
                customer.SupportContractEndDate,
                customer.IsActive,
                customer.LastUpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDetailModel?> GetDetailAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.Id == customerId)
            .Select(customer => new CustomerDetailModel(
                customer.Id,
                customer.Name,
                customer.AccountCode,
                customer.AddressLine1,
                customer.AddressLine2,
                customer.TownCity,
                customer.CountyRegion,
                customer.Postcode,
                customer.Country,
                customer.MainPhone,
                customer.MainEmail,
                customer.Website,
                customer.PrimaryContactName,
                customer.PrimaryContactEmail,
                customer.PrimaryContactPhone,
                customer.SupportContractLevelId,
                customer.SupportContractLevel != null ? customer.SupportContractLevel.Name : null,
                customer.SupportContractStatus,
                customer.SupportContractStartDate,
                customer.SupportContractEndDate,
                customer.SupportNotes,
                customer.IsActive,
                customer.CreatedAt,
                customer.CreatedBy,
                customer.LastUpdatedAt,
                customer.LastUpdatedBy,
                customer.BuildRecords
                    .Where(buildRecord => buildRecord.IsActive)
                    .OrderByDescending(buildRecord => buildRecord.LastUpdatedAt)
                    .Select(buildRecord => new LinkedCustomerBuildRecord(
                        buildRecord.Id,
                        buildRecord.ProductCode,
                        buildRecord.ProductName,
                        buildRecord.SerialNumber,
                        buildRecord.MachineName,
                        buildRecord.RadSightVersion,
                        buildRecord.DateShipped,
                        buildRecord.LastUpdatedAt))
                    .ToList(),
                customer.RmaRecords
                    .Where(rmaRecord => rmaRecord.IsActive)
                    .OrderByDescending(rmaRecord => rmaRecord.LastUpdatedAt)
                    .Select(rmaRecord => new LinkedCustomerRma(
                        rmaRecord.Id,
                        rmaRecord.RmaNumber,
                        rmaRecord.Status,
                        rmaRecord.ProductName,
                        rmaRecord.SerialNumber,
                        rmaRecord.FaultSummary,
                        rmaRecord.SupportTicketNumber,
                        rmaRecord.DueDate,
                        rmaRecord.LastUpdatedAt,
                        rmaRecord.Status != RmaStatus.Closed
                            && rmaRecord.Status != RmaStatus.Shipped
                            && rmaRecord.Status != RmaStatus.CancelledNoReply
                            && rmaRecord.Status != RmaStatus.CustomerFixed,
                        rmaRecord.DueDate != null
                            && rmaRecord.DueDate < DateOnly.FromDateTime(DateTime.UtcNow)
                            && rmaRecord.Status != RmaStatus.Closed
                            && rmaRecord.Status != RmaStatus.Shipped
                            && rmaRecord.Status != RmaStatus.CancelledNoReply
                            && rmaRecord.Status != RmaStatus.CustomerFixed))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<CustomerSaveResult> CreateAsync(
        CreateCustomerRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = CustomerFormValidator.Validate(
            request.Name,
            request.SupportContractStatus,
            request.SupportContractStartDate,
            request.SupportContractEndDate);

        if (validationErrors.Count > 0)
        {
            return CustomerSaveResult.Failure(validationErrors.ToArray());
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedName = NormalizeRequiredValue(request.Name);

        if (await CustomerNameExistsAsync(dbContext, normalizedName, null, cancellationToken))
        {
            return CustomerSaveResult.Failure("A customer with this name already exists.");
        }

        if (!await SupportContractLevelExistsAsync(dbContext, request.SupportContractLevelId, cancellationToken))
        {
            return CustomerSaveResult.Failure("Selected support contract level was not found.");
        }

        var userName = NormalizeUserName(createdBy);
        var customer = new Customer
        {
            Name = normalizedName,
            AccountCode = NormalizeOptionalValue(request.AccountCode),
            AddressLine1 = NormalizeOptionalValue(request.AddressLine1),
            AddressLine2 = NormalizeOptionalValue(request.AddressLine2),
            TownCity = NormalizeOptionalValue(request.TownCity),
            CountyRegion = NormalizeOptionalValue(request.CountyRegion),
            Postcode = NormalizeOptionalValue(request.Postcode),
            Country = NormalizeOptionalValue(request.Country),
            MainPhone = NormalizeOptionalValue(request.MainPhone),
            MainEmail = NormalizeOptionalValue(request.MainEmail),
            Website = NormalizeOptionalValue(request.Website),
            PrimaryContactName = NormalizeOptionalValue(request.PrimaryContactName),
            PrimaryContactEmail = NormalizeOptionalValue(request.PrimaryContactEmail),
            PrimaryContactPhone = NormalizeOptionalValue(request.PrimaryContactPhone),
            SupportContractLevelId = request.SupportContractLevelId,
            SupportContractStatus = NormalizeContractStatus(request.SupportContractStatus),
            SupportContractStartDate = request.SupportContractStartDate,
            SupportContractEndDate = request.SupportContractEndDate,
            SupportNotes = NormalizeOptionalValue(request.SupportNotes),
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userName,
            LastUpdatedAt = DateTimeOffset.UtcNow,
            LastUpdatedBy = userName
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CustomerSaveResult.Success(customer.Id);
    }

    public async Task<CustomerSaveResult> UpdateAsync(
        int customerId,
        UpdateCustomerRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = CustomerFormValidator.Validate(
            request.Name,
            request.SupportContractStatus,
            request.SupportContractStartDate,
            request.SupportContractEndDate);

        if (validationErrors.Count > 0)
        {
            return CustomerSaveResult.Failure(validationErrors.ToArray());
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var customer = await dbContext.Customers
            .SingleOrDefaultAsync(existingCustomer => existingCustomer.Id == customerId, cancellationToken);

        if (customer is null)
        {
            return CustomerSaveResult.Failure("Customer was not found.");
        }

        var normalizedName = NormalizeRequiredValue(request.Name);
        if (await CustomerNameExistsAsync(dbContext, normalizedName, customerId, cancellationToken))
        {
            return CustomerSaveResult.Failure("A customer with this name already exists.");
        }

        if (!await SupportContractLevelExistsAsync(dbContext, request.SupportContractLevelId, cancellationToken))
        {
            return CustomerSaveResult.Failure("Selected support contract level was not found.");
        }

        var userName = NormalizeUserName(updatedBy);
        customer.Name = normalizedName;
        customer.AccountCode = NormalizeOptionalValue(request.AccountCode);
        customer.AddressLine1 = NormalizeOptionalValue(request.AddressLine1);
        customer.AddressLine2 = NormalizeOptionalValue(request.AddressLine2);
        customer.TownCity = NormalizeOptionalValue(request.TownCity);
        customer.CountyRegion = NormalizeOptionalValue(request.CountyRegion);
        customer.Postcode = NormalizeOptionalValue(request.Postcode);
        customer.Country = NormalizeOptionalValue(request.Country);
        customer.MainPhone = NormalizeOptionalValue(request.MainPhone);
        customer.MainEmail = NormalizeOptionalValue(request.MainEmail);
        customer.Website = NormalizeOptionalValue(request.Website);
        customer.PrimaryContactName = NormalizeOptionalValue(request.PrimaryContactName);
        customer.PrimaryContactEmail = NormalizeOptionalValue(request.PrimaryContactEmail);
        customer.PrimaryContactPhone = NormalizeOptionalValue(request.PrimaryContactPhone);
        customer.SupportContractLevelId = request.SupportContractLevelId;
        customer.SupportContractStatus = NormalizeContractStatus(request.SupportContractStatus);
        customer.SupportContractStartDate = request.SupportContractStartDate;
        customer.SupportContractEndDate = request.SupportContractEndDate;
        customer.SupportNotes = NormalizeOptionalValue(request.SupportNotes);
        customer.IsActive = request.IsActive;
        customer.LastUpdatedAt = DateTimeOffset.UtcNow;
        customer.LastUpdatedBy = userName;

        await ApplySupportContractDefaultsToLinkedRmasAsync(
            dbContext,
            customer,
            userName,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return CustomerSaveResult.Success(customer.Id);
    }

    private static IQueryable<Customer> ApplySort(IQueryable<Customer> query, CustomerListFilter filter)
    {
        return (filter.SortBy, filter.SortDescending) switch
        {
            (CustomerSortColumn.SupportContractLevel, false) => query
                .OrderBy(customer => customer.SupportContractLevel != null ? customer.SupportContractLevel.Name : string.Empty)
                .ThenBy(customer => customer.Name),
            (CustomerSortColumn.SupportContractLevel, true) => query
                .OrderByDescending(customer => customer.SupportContractLevel != null ? customer.SupportContractLevel.Name : string.Empty)
                .ThenBy(customer => customer.Name),
            (CustomerSortColumn.SupportContractStatus, false) => query
                .OrderBy(customer => customer.SupportContractStatus)
                .ThenBy(customer => customer.Name),
            (CustomerSortColumn.SupportContractStatus, true) => query
                .OrderByDescending(customer => customer.SupportContractStatus)
                .ThenBy(customer => customer.Name),
            (CustomerSortColumn.ContractEndDate, false) => query
                .OrderBy(customer => customer.SupportContractEndDate)
                .ThenBy(customer => customer.Name),
            (CustomerSortColumn.ContractEndDate, true) => query
                .OrderByDescending(customer => customer.SupportContractEndDate)
                .ThenBy(customer => customer.Name),
            (CustomerSortColumn.LastUpdated, false) => query
                .OrderBy(customer => customer.LastUpdatedAt)
                .ThenBy(customer => customer.Name),
            (CustomerSortColumn.LastUpdated, true) => query
                .OrderByDescending(customer => customer.LastUpdatedAt)
                .ThenBy(customer => customer.Name),
            (_, true) => query.OrderByDescending(customer => customer.Name),
            _ => query.OrderBy(customer => customer.Name)
        };
    }

    private static async Task<bool> CustomerNameExistsAsync(
        BuildBookDbContext dbContext,
        string normalizedName,
        int? excludedCustomerId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Customers.AnyAsync(
            customer => customer.Name.ToLower() == normalizedName.ToLower()
                && (excludedCustomerId == null || customer.Id != excludedCustomerId),
            cancellationToken);
    }

    private static async Task<bool> SupportContractLevelExistsAsync(
        BuildBookDbContext dbContext,
        int? supportContractLevelId,
        CancellationToken cancellationToken)
    {
        return supportContractLevelId is null
            || await dbContext.SupportContractLevels.AnyAsync(
                level => level.Id == supportContractLevelId,
                cancellationToken);
    }

    private static string NormalizeRequiredValue(string value)
    {
        return value.Trim();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeUserName(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
    }

    private static string NormalizeContractStatus(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? CustomerSupportContractStatuses.NoContract
            : value.Trim();
    }

    private async Task ApplySupportContractDefaultsToLinkedRmasAsync(
        BuildBookDbContext dbContext,
        Customer customer,
        string userName,
        CancellationToken cancellationToken)
    {
        var supportContractLevel = customer.SupportContractLevelId is null
            ? null
            : await dbContext.SupportContractLevels
                .SingleOrDefaultAsync(level => level.Id == customer.SupportContractLevelId.Value, cancellationToken);

        var contractPriority = CustomerContractRmaDefaults.GetPriority(customer.SupportContractStatus, supportContractLevel);
        var contractWarrantyStatus = CustomerContractRmaDefaults.GetWarrantyStatus(customer.SupportContractStatus, supportContractLevel);
        var contractWarrantyExpiryDate = CustomerContractRmaDefaults.GetWarrantyExpiryDate(customer.SupportContractStatus, customer.SupportContractEndDate);

        if (contractPriority is null && contractWarrantyStatus is null && contractWarrantyExpiryDate is null)
        {
            return;
        }

        var linkedRmaRecords = await dbContext.RmaRecords
            .Where(rmaRecord => rmaRecord.CustomerId == customer.Id && rmaRecord.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var rmaRecord in linkedRmaRecords)
        {
            List<RmaAuditChange> changes = [];

            if (rmaRecord.Priority is null && contractPriority is not null)
            {
                changes.Add(new RmaAuditChange("Priority", null, contractPriority.Value.ToString()));
                rmaRecord.Priority = contractPriority;
            }

            if (rmaRecord.WarrantyStatus is null && contractWarrantyStatus is not null)
            {
                changes.Add(new RmaAuditChange("WarrantyStatus", null, contractWarrantyStatus.Value.ToString()));
                rmaRecord.WarrantyStatus = contractWarrantyStatus;
            }

            if (rmaRecord.WarrantyExpiryDate is null && contractWarrantyExpiryDate is not null)
            {
                changes.Add(new RmaAuditChange("WarrantyExpiryDate", null, contractWarrantyExpiryDate.Value.ToString("yyyy-MM-dd")));
                rmaRecord.WarrantyExpiryDate = contractWarrantyExpiryDate;
            }

            if (changes.Count == 0)
            {
                continue;
            }

            rmaRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
            rmaRecord.LastUpdatedBy = userName;
            dbContext.RmaAudit.AddRange(rmaAuditService.CreateRecordUpdatedEntries(rmaRecord, changes, userName));
        }
    }
}
