using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRecordDetailReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IBuildRecordDetailReader
{
    public async Task<BuildRecordDetailModel?> GetByIdAsync(
        int buildRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecord = await dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.Id == buildRecordId && buildRecord.IsActive)
            .Select(buildRecord => new
            {
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.ProductClassification,
                buildRecord.SerialNumber,
                buildRecord.InternalStatus,
                buildRecord.CustomerId,
                CustomerName = buildRecord.Customer == null ? null : buildRecord.Customer.Name,
                buildRecord.CustomerOrder,
                buildRecord.OANumber,
                buildRecord.InvoiceNumber,
                buildRecord.MachineName,
                buildRecord.PanelDeviceModel,
                buildRecord.PanelDeviceSerial,
                buildRecord.PanelFirmwareVersion,
                buildRecord.RadioSerialNumber,
                buildRecord.RouterUsed,
                buildRecord.HardwareNotes,
                buildRecord.WifiSsid,
                buildRecord.Note,
                buildRecord.DiskImageVersion,
                buildRecord.RadSightVersion,
                buildRecord.WindowsVersion,
                buildRecord.WindowsLatestPatch,
                buildRecord.BleuvioFirmwareVersion,
                buildRecord.CharthouseIrdaFirmwareVersion,
                buildRecord.RadioFirmware,
                buildRecord.RadSightUserLogin,
                buildRecord.KioskUser,
                buildRecord.WindowsAdminUser,
                buildRecord.DateShipped,
                buildRecord.AssembledIn,
                buildRecord.AssembledBy,
                buildRecord.DateAssembled,
                buildRecord.HardwareManufacturer,
                buildRecord.ManufacturerPartNumber,
                buildRecord.ManufacturerRevision,
                buildRecord.ManufacturerSerialNumber,
                buildRecord.PackingList,
                buildRecord.CheckedBy,
                LinkedOrders = buildRecord.OrderLinks
                    .OrderByDescending(link => link.LinkedAt)
                    .Select(link => new LinkedBuildRecordOrderSummary(
                        link.OrderRecordId,
                        link.OrderRecord != null ? link.OrderRecord.OrderNumber : string.Empty,
                        link.OrderRecord != null ? link.OrderRecord.OrderTitle : string.Empty,
                        link.OrderRecord != null ? link.OrderRecord.Status : string.Empty,
                        link.OrderRecord != null && link.OrderRecord.Customer != null ? link.OrderRecord.Customer.Name : null,
                        link.LinkType,
                        link.LinkedAt))
                    .ToArray(),
                SecretTypesSet = buildRecord.Secrets
                    .Select(secret => secret.SecretType)
                    .ToArray(),
                buildRecord.LastUpdatedAt,
                buildRecord.LastUpdatedBy
            })
            .SingleOrDefaultAsync(cancellationToken);

        return buildRecord is null
            ? null
            : new BuildRecordDetailModel(
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.ProductClassification,
                buildRecord.SerialNumber,
                buildRecord.InternalStatus?.ToString(),
                buildRecord.CustomerId,
                buildRecord.CustomerName,
                buildRecord.CustomerOrder,
                buildRecord.OANumber,
                buildRecord.InvoiceNumber,
                buildRecord.MachineName,
                buildRecord.PanelDeviceModel,
                buildRecord.PanelDeviceSerial,
                buildRecord.PanelFirmwareVersion,
                buildRecord.RadioSerialNumber,
                buildRecord.RouterUsed,
                buildRecord.HardwareNotes,
                buildRecord.WifiSsid,
                buildRecord.Note,
                buildRecord.DiskImageVersion,
                buildRecord.RadSightVersion,
                buildRecord.WindowsVersion,
                buildRecord.WindowsLatestPatch,
                buildRecord.BleuvioFirmwareVersion,
                buildRecord.CharthouseIrdaFirmwareVersion,
                buildRecord.RadioFirmware,
                buildRecord.RadSightUserLogin,
                buildRecord.KioskUser,
                buildRecord.WindowsAdminUser,
                buildRecord.DateShipped,
                buildRecord.AssembledIn,
                buildRecord.AssembledBy,
                buildRecord.DateAssembled,
                buildRecord.HardwareManufacturer,
                buildRecord.ManufacturerPartNumber,
                buildRecord.ManufacturerRevision,
                buildRecord.ManufacturerSerialNumber,
                buildRecord.PackingList,
                buildRecord.CheckedBy,
                buildRecord.LinkedOrders,
                buildRecord.SecretTypesSet,
                buildRecord.LastUpdatedAt,
                buildRecord.LastUpdatedBy);
    }
}
