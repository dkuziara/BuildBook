using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRecordSearchService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IBuildRecordSearchService
{
    public async Task<IReadOnlyList<BuildRecordSearchResult>> SearchAsync(
        string? searchText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return [];
        }

        var likePattern = $"%{EscapeLikePattern(searchText.Trim())}%";

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.IsActive)
            .Where(buildRecord =>
                EF.Functions.Like(buildRecord.ProductCode, likePattern, @"\")
                || EF.Functions.Like(buildRecord.ProductName, likePattern, @"\")
                || EF.Functions.Like(buildRecord.ProductClassification!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.SerialNumber, likePattern, @"\")
                || EF.Functions.Like(buildRecord.AssembledIn!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.AssembledBy!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.HardwareManufacturer!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.ManufacturerPartNumber!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.ManufacturerRevision!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.ManufacturerSerialNumber!, likePattern, @"\")
                || (buildRecord.Customer != null && EF.Functions.Like(buildRecord.Customer.Name, likePattern, @"\"))
                || EF.Functions.Like(buildRecord.CustomerOrder!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.OANumber!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.InvoiceNumber!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.PanelDeviceModel!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.PanelDeviceSerial!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.PanelFirmwareVersion!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.DiskImageVersion!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.RadSightUserLogin!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.KioskUser!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.MachineName!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.RadSightVersion!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.WindowsVersion!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.WindowsLatestPatch!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.BleuvioFirmwareVersion!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.CharthouseIrdaFirmwareVersion!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.RadioFirmware!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.RadioSerialNumber!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.WifiSsid!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.RouterUsed!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.PackingList!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.CheckedBy!, likePattern, @"\")
                || EF.Functions.Like(buildRecord.Note!, likePattern, @"\"))
            .OrderByDescending(buildRecord => buildRecord.LastUpdatedAt)
            .ThenBy(buildRecord => buildRecord.SerialNumber)
            .Select(buildRecord => new BuildRecordSearchResult(
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.SerialNumber,
                buildRecord.Customer == null ? null : buildRecord.Customer.Name,
                buildRecord.MachineName,
                buildRecord.RadSightVersion,
                buildRecord.WindowsVersion,
                buildRecord.DateShipped))
            .ToListAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal)
            .Replace("[", @"\[", StringComparison.Ordinal);
    }
}
