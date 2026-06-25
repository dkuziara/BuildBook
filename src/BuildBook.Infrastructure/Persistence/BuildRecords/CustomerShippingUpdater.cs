using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class CustomerShippingUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IBuildRecordAuditService buildRecordAuditService) : ICustomerShippingUpdater
{
    public async Task<UpdateCustomerShippingResult> UpdateAsync(
        int buildRecordId,
        UpdateCustomerShippingRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = string.IsNullOrWhiteSpace(updatedBy) ? "Unknown" : updatedBy.Trim();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecord = await dbContext.BuildRecords
            .Include(record => record.Customer)
            .SingleOrDefaultAsync(
                record => record.Id == buildRecordId && record.IsActive,
                cancellationToken);

        if (buildRecord is null)
        {
            return UpdateCustomerShippingResult.Failure("Build Record was not found.");
        }

        string? newCustomerName = null;

        if (request.CustomerId is not null)
        {
            newCustomerName = await dbContext.Customers
                .Where(customer => customer.Id == request.CustomerId && customer.IsActive)
                .Select(customer => customer.Name)
                .SingleOrDefaultAsync(cancellationToken);

            if (newCustomerName is null)
            {
                return UpdateCustomerShippingResult.Failure("Selected customer was not found.");
            }
        }

        var customerOrder = NormalizeOptionalValue(request.CustomerOrder);
        var oaNumber = NormalizeOptionalValue(request.OANumber);
        var invoiceNumber = NormalizeOptionalValue(request.InvoiceNumber);

        var auditEntries = buildRecordAuditService.CreateRecordUpdatedEntries(
            buildRecord,
            [
                new BuildRecordAuditChange("Customer", buildRecord.Customer?.Name, newCustomerName),
                new BuildRecordAuditChange("CustomerOrder", buildRecord.CustomerOrder, customerOrder),
                new BuildRecordAuditChange("OANumber", buildRecord.OANumber, oaNumber),
                new BuildRecordAuditChange("InvoiceNumber", buildRecord.InvoiceNumber, invoiceNumber),
                new BuildRecordAuditChange("DateShipped", FormatDate(buildRecord.DateShipped), FormatDate(request.DateShipped))
            ],
            userName);

        if (auditEntries.Count == 0)
        {
            return UpdateCustomerShippingResult.Success();
        }

        buildRecord.CustomerId = request.CustomerId;
        buildRecord.CustomerOrder = customerOrder;
        buildRecord.OANumber = oaNumber;
        buildRecord.InvoiceNumber = invoiceNumber;
        buildRecord.DateShipped = request.DateShipped;
        buildRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        buildRecord.LastUpdatedBy = userName;

        await dbContext.BuildRecordAudit.AddRangeAsync(auditEntries, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateCustomerShippingResult.Success();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? FormatDate(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd");
    }
}
