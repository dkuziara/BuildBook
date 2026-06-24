using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class CustomerShippingUpdater(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : ICustomerShippingUpdater
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

        var auditEntries = CreateAuditEntries(
            buildRecord,
            newCustomerName,
            customerOrder,
            oaNumber,
            invoiceNumber,
            request.DateShipped,
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

    private static List<BuildRecordAudit> CreateAuditEntries(
        BuildRecord buildRecord,
        string? customerName,
        string? customerOrder,
        string? oaNumber,
        string? invoiceNumber,
        DateOnly? dateShipped,
        string userName)
    {
        var auditEntries = new List<BuildRecordAudit>();

        AddAuditEntryIfChanged(auditEntries, buildRecord, "Customer", buildRecord.Customer?.Name, customerName, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "CustomerOrder", buildRecord.CustomerOrder, customerOrder, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "OANumber", buildRecord.OANumber, oaNumber, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "InvoiceNumber", buildRecord.InvoiceNumber, invoiceNumber, userName);
        AddAuditEntryIfChanged(auditEntries, buildRecord, "DateShipped", FormatDate(buildRecord.DateShipped), FormatDate(dateShipped), userName);

        return auditEntries;
    }

    private static string? FormatDate(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd");
    }

    private static void AddAuditEntryIfChanged(
        ICollection<BuildRecordAudit> auditEntries,
        BuildRecord buildRecord,
        string fieldChanged,
        string? oldValue,
        string? newValue,
        string userName)
    {
        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            return;
        }

        auditEntries.Add(new BuildRecordAudit
        {
            BuildRecordId = buildRecord.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            User = userName,
            Action = AuditAction.Updated,
            FieldChanged = fieldChanged,
            OldValue = oldValue,
            NewValue = newValue
        });
    }
}
