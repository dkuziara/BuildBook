using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderRecordCreator(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IOrderRecordCreator
{
    private const int NotesSummaryMaximumLength = 1024;
    private const string SummarySuffix = "...";

    public async Task<CreateOrderResult> CreateAsync(
        CreateOrderRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = CreateOrderValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return CreateOrderResult.Failure(validationErrors);
        }

        var normalizedCreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Unknown" : createdBy.Trim();
        var normalizedTitle = request.OrderTitle.Trim();
        var normalizedStatus = request.Status.Trim();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (request.CustomerId is not null)
        {
            var customerName = await dbContext.Customers
                .Where(customer => customer.Id == request.CustomerId.Value && customer.IsActive)
                .Select(customer => customer.Name)
                .SingleOrDefaultAsync(cancellationToken);

            if (customerName is null)
            {
                return CreateOrderResult.Failure("Selected customer was not found.");
            }
        }

        var matchedUser = await dbContext.ApplicationUsers
            .Where(user => user.IsActive)
            .Where(user => user.WindowsUserName == normalizedCreatedBy
                || user.DisplayName == normalizedCreatedBy
                || user.EmailAddress == normalizedCreatedBy)
            .Select(user => new
            {
                user.Id
            })
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var orderRecord = new OrderRecord
        {
            OrderNumber = $"TMP-{Guid.NewGuid():N}",
            OrderTitle = normalizedTitle,
            OrderDescription = NormalizeOptionalValue(request.OrderDescription),
            CustomerId = request.CustomerId,
            Status = normalizedStatus,
            Priority = request.Priority,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            CreatedAt = now,
            CreatedByUserId = matchedUser?.Id,
            ImportedCreatedByText = normalizedCreatedBy,
            LastUpdatedAt = now,
            LastUpdatedByUserId = matchedUser?.Id,
            IsRecurring = request.IsRecurring,
            CustomerReference = NormalizeOptionalValue(request.CustomerReference),
            CustomerPurchaseOrderNumber = NormalizeOptionalValue(request.CustomerPurchaseOrderNumber),
            InternalOrderReference = NormalizeOptionalValue(request.InternalOrderReference),
            QuoteNumber = NormalizeOptionalValue(request.QuoteNumber),
            NotesSummary = SummarizeText(request.OrderDescription),
            SupportTicketNo = NormalizeOptionalValue(request.SupportTicketNo),
            IsActive = true
        };

        orderRecord.StatusHistoryEntries.Add(new OrderStatusHistory
        {
            OldStatus = null,
            NewStatus = normalizedStatus,
            ChangedByUserId = matchedUser?.Id,
            ChangedAt = now,
            Reason = "Order created manually."
        });

        dbContext.OrderRecords.Add(orderRecord);
        await dbContext.SaveChangesAsync(cancellationToken);

        orderRecord.OrderNumber = BuildOrderNumber(orderRecord.Id, now);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreateOrderResult.Success(orderRecord.Id);
    }

    private static string BuildOrderNumber(int orderId, DateTimeOffset createdAt)
    {
        return $"ORD-{createdAt:yyyyMMdd}-{orderId:D4}";
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? SummarizeText(string? value)
    {
        var normalized = NormalizeOptionalValue(value);
        if (normalized is null)
        {
            return null;
        }

        return normalized.Length <= NotesSummaryMaximumLength
            ? normalized
            : TruncateWithSuffix(normalized, NotesSummaryMaximumLength);
    }

    private static string TruncateWithSuffix(string value, int maximumLength)
    {
        if (maximumLength <= 0)
        {
            return string.Empty;
        }

        if (value.Length <= maximumLength)
        {
            return value;
        }

        if (maximumLength <= SummarySuffix.Length)
        {
            return SummarySuffix[..maximumLength];
        }

        return $"{value[..(maximumLength - SummarySuffix.Length)].TrimEnd()}{SummarySuffix}";
    }
}
