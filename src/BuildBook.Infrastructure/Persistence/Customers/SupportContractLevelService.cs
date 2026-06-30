using BuildBook.Application.Customers;
using BuildBook.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Customers;

public sealed class SupportContractLevelService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : ISupportContractLevelService
{
    public async Task<IReadOnlyList<SupportContractLevelModel>> ListAsync(
        bool includeInactive = true,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.SupportContractLevels
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(level => level.IsActive);
        }

        return await query
            .OrderBy(level => level.DisplayOrder)
            .ThenBy(level => level.Name)
            .Select(level => new SupportContractLevelModel(
                level.Id,
                level.Name,
                level.Description,
                level.TargetResponseTimeValue,
                level.TargetResponseTimeUnit,
                level.DefaultRmaPriority,
                level.RmaPriorityWeight,
                level.DisplayOrder,
                level.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<SupportContractLevelSaveResult> CreateAsync(
        CreateSupportContractLevelRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = SupportContractLevelValidator.Validate(
            request.Name,
            request.TargetResponseTimeValue,
            request.TargetResponseTimeUnit);

        if (validationErrors.Count > 0)
        {
            return SupportContractLevelSaveResult.Failure(validationErrors.ToArray());
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedName = NormalizeRequiredValue(request.Name);

        if (await NameExistsAsync(dbContext, normalizedName, null, cancellationToken))
        {
            return SupportContractLevelSaveResult.Failure("A support contract level with this name already exists.");
        }

        var userName = NormalizeUserName(createdBy);
        var level = new SupportContractLevel
        {
            Name = normalizedName,
            Description = NormalizeOptionalValue(request.Description),
            TargetResponseTimeValue = request.TargetResponseTimeValue,
            TargetResponseTimeUnit = request.TargetResponseTimeUnit,
            DefaultRmaPriority = request.DefaultRmaPriority,
            RmaPriorityWeight = request.RmaPriorityWeight,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userName,
            LastUpdatedAt = DateTimeOffset.UtcNow,
            LastUpdatedBy = userName
        };

        dbContext.SupportContractLevels.Add(level);
        await dbContext.SaveChangesAsync(cancellationToken);

        return SupportContractLevelSaveResult.Success(level.Id);
    }

    public async Task<SupportContractLevelSaveResult> UpdateAsync(
        int supportContractLevelId,
        UpdateSupportContractLevelRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = SupportContractLevelValidator.Validate(
            request.Name,
            request.TargetResponseTimeValue,
            request.TargetResponseTimeUnit);

        if (validationErrors.Count > 0)
        {
            return SupportContractLevelSaveResult.Failure(validationErrors.ToArray());
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var level = await dbContext.SupportContractLevels
            .SingleOrDefaultAsync(existingLevel => existingLevel.Id == supportContractLevelId, cancellationToken);

        if (level is null)
        {
            return SupportContractLevelSaveResult.Failure("Support contract level was not found.");
        }

        var normalizedName = NormalizeRequiredValue(request.Name);
        if (await NameExistsAsync(dbContext, normalizedName, supportContractLevelId, cancellationToken))
        {
            return SupportContractLevelSaveResult.Failure("A support contract level with this name already exists.");
        }

        level.Name = normalizedName;
        level.Description = NormalizeOptionalValue(request.Description);
        level.TargetResponseTimeValue = request.TargetResponseTimeValue;
        level.TargetResponseTimeUnit = request.TargetResponseTimeUnit;
        level.DefaultRmaPriority = request.DefaultRmaPriority;
        level.RmaPriorityWeight = request.RmaPriorityWeight;
        level.DisplayOrder = request.DisplayOrder;
        level.IsActive = request.IsActive;
        level.LastUpdatedAt = DateTimeOffset.UtcNow;
        level.LastUpdatedBy = NormalizeUserName(updatedBy);

        await dbContext.SaveChangesAsync(cancellationToken);

        return SupportContractLevelSaveResult.Success(level.Id);
    }

    private static async Task<bool> NameExistsAsync(
        BuildBookDbContext dbContext,
        string normalizedName,
        int? excludedSupportContractLevelId,
        CancellationToken cancellationToken)
    {
        return await dbContext.SupportContractLevels.AnyAsync(
            level => level.Name.ToLower() == normalizedName.ToLower()
                && (excludedSupportContractLevelId == null || level.Id != excludedSupportContractLevelId),
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
}
