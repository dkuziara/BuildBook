using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using BuildBook.Application.BuildRecords;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class SpreadsheetImportMappingService : ISpreadsheetImportMappingService
{
    private static readonly IReadOnlyList<SpreadsheetImportFieldOption> AvailableFields =
    [
        CreateField("ProductCode", "Product code", isRequired: true),
        CreateField("ProductName", "Product name", isRequired: true),
        CreateField("ProductClassification", "Product classification"),
        CreateField("SerialNumber", "Serial number", isRequired: true),
        CreateField("InternalStatus", "Internal status"),
        CreateField("AssembledIn", "Assembled in"),
        CreateField("AssembledBy", "Assembled by"),
        CreateField("DateAssembled", "Date assembled"),
        CreateField("HardwareManufacturer", "H/W manufacturer"),
        CreateField("ManufacturerPartNumber", "Manufacturer part no."),
        CreateField("ManufacturerRevision", "Manufacturer revision"),
        CreateField("ManufacturerSerialNumber", "Manufacturer serial no."),
        CreateField("Customer", "Customer"),
        CreateField("CustomerOrder", "Customer order"),
        CreateField("OANumber", "OA number"),
        CreateField("InvoiceNumber", "Invoice number"),
        CreateField("DateShipped", "Date shipped"),
        CreateField("ShippingNotes", "Shipping notes"),
        CreateField("PanelDeviceModel", "Panel device model"),
        CreateField("PanelDeviceSerial", "Panel device serial"),
        CreateField("PanelFirmwareVersion", "Panel firmware version"),
        CreateField("MachineName", "Machine name"),
        CreateField("RadioSerialNumber", "Radio serial number"),
        CreateField("RouterUsed", "Router used"),
        CreateField("HardwareNotes", "Hardware notes"),
        CreateField("DiskImageVersion", "Disk image version"),
        CreateField("RadSightVersion", "RadSight version"),
        CreateField("WindowsVersion", "Windows version"),
        CreateField("WindowsLatestPatch", "Windows latest patch"),
        CreateField("BleuvioFirmwareVersion", "Bleuvio firmware version"),
        CreateField("CharthouseIrdaFirmwareVersion", "Charthouse IRDA firmware version"),
        CreateField("RadioFirmware", "Radio firmware"),
        CreateField("RadSightUserLogin", "RadSight user login"),
        CreateField("KioskUser", "Kiosk user"),
        CreateField("WindowsAdminUser", "Windows admin user"),
        CreateField("WifiSsid", "Wi-Fi SSID"),
        CreateField("Note", "Note"),
        CreateField("PackingList", "Packing list"),
        CreateField("CheckedBy", "Checked by"),
        CreateField("OriginalSpreadsheetRowNumber", "Original spreadsheet row number"),
        CreateField("RadSightUserPassword", "RadSight user password", isSensitive: true),
        CreateField("WindowsAdminPassword", "Windows admin password", isSensitive: true),
        CreateField("KioskPassword", "Kiosk password", isSensitive: true),
        CreateField("WifiPassword", "Wi-Fi password", isSensitive: true),
        CreateField("RouterPassword", "Router password", isSensitive: true),
        CreateField("BitLockerRecoveryKey", "BitLocker recovery key", isSensitive: true)
    ];

    private static readonly Dictionary<string, string> FieldLookup = AvailableFields.ToDictionary(
        field => field.Key,
        field => field.Label,
        StringComparer.Ordinal);

    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Product Code"] = "ProductCode",
        ["Product"] = "ProductCode",
        ["Product Name"] = "ProductName",
        ["Product Classification"] = "ProductClassification",
        ["Classification"] = "ProductClassification",
        ["Serial Number"] = "SerialNumber",
        ["Serial No"] = "SerialNumber",
        ["Serial No."] = "SerialNumber",
        ["Status"] = "InternalStatus",
        ["Internal Status"] = "InternalStatus",
        ["Assembled In"] = "AssembledIn",
        ["Assembled By"] = "AssembledBy",
        ["Date Assembled"] = "DateAssembled",
        ["H/W Manufacturer"] = "HardwareManufacturer",
        ["Hardware Manufacturer"] = "HardwareManufacturer",
        ["Manufacturer Part No"] = "ManufacturerPartNumber",
        ["Manufacturer Part No."] = "ManufacturerPartNumber",
        ["Manufacturer Revision"] = "ManufacturerRevision",
        ["Manufacturer Serial No"] = "ManufacturerSerialNumber",
        ["Manufacturer Serial No."] = "ManufacturerSerialNumber",
        ["Customer"] = "Customer",
        ["Customer Order"] = "CustomerOrder",
        ["OA Number"] = "OANumber",
        ["Invoice Number"] = "InvoiceNumber",
        ["Date Shipped"] = "DateShipped",
        ["Shipping Notes"] = "ShippingNotes",
        ["Panel Device Model"] = "PanelDeviceModel",
        ["Panel Device Serial"] = "PanelDeviceSerial",
        ["Panel Firmware Version"] = "PanelFirmwareVersion",
        ["Machine Name"] = "MachineName",
        ["Radio Serial Number"] = "RadioSerialNumber",
        ["Router Used"] = "RouterUsed",
        ["Hardware Notes"] = "HardwareNotes",
        ["Disk Image Version"] = "DiskImageVersion",
        ["RadSight Version"] = "RadSightVersion",
        ["Windows Version"] = "WindowsVersion",
        ["Windows Latest Patch"] = "WindowsLatestPatch",
        ["Bleuvio Firmware Version"] = "BleuvioFirmwareVersion",
        ["Charthouse IRDA Firmware Version"] = "CharthouseIrdaFirmwareVersion",
        ["Radio Firmware"] = "RadioFirmware",
        ["RadSight User Login"] = "RadSightUserLogin",
        ["Kiosk User"] = "KioskUser",
        ["Windows Admin User"] = "WindowsAdminUser",
        ["Wi-Fi SSID"] = "WifiSsid",
        ["Wifi SSID"] = "WifiSsid",
        ["Note"] = "Note",
        ["Notes"] = "Note",
        ["Packing List"] = "PackingList",
        ["Checked By"] = "CheckedBy",
        ["Row Number"] = "OriginalSpreadsheetRowNumber",
        ["Original Spreadsheet Row Number"] = "OriginalSpreadsheetRowNumber",
        ["RadSight User Password"] = "RadSightUserPassword",
        ["Windows Admin Password"] = "WindowsAdminPassword",
        ["Kiosk Password"] = "KioskPassword",
        ["Wi-Fi Password"] = "WifiPassword",
        ["Wifi Password"] = "WifiPassword",
        ["Router Password"] = "RouterPassword",
        ["BitLocker Recovery Key"] = "BitLockerRecoveryKey"
    };

    public async Task<SpreadsheetColumnMappingReview> BuildReviewAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(fileStream);

        var extension = Path.GetExtension(fileName);
        var notices = new List<string>();
        IReadOnlyList<string> sourceColumns;

        if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            sourceColumns = await ReadCsvHeadersAsync(fileStream, cancellationToken);
        }
        else if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            sourceColumns = ReadXlsxHeaders(fileStream);
        }
        else if (extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
        {
            notices.Add("Header review is available for .xlsx and .csv files. Save legacy .xls files as .xlsx or .csv to review column mappings.");
            sourceColumns = [];
        }
        else
        {
            sourceColumns = [];
        }

        if (sourceColumns.Count == 0 && notices.Count == 0)
        {
            notices.Add("No spreadsheet headers were detected. Check the first row and try again.");
        }

        return new SpreadsheetColumnMappingReview
        {
            AvailableFields = AvailableFields,
            Notices = notices,
            ColumnMappings = sourceColumns
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(header => new SpreadsheetImportColumnMapping
                {
                    SourceColumnName = header,
                    SuggestedFieldKey = SuggestFieldKey(header)
                })
                .ToArray()
        };
    }

    private static SpreadsheetImportFieldOption CreateField(
        string key,
        string label,
        bool isRequired = false,
        bool isSensitive = false)
    {
        return new SpreadsheetImportFieldOption
        {
            Key = key,
            Label = label,
            IsRequired = isRequired,
            IsSensitive = isSensitive
        };
    }

    private static async Task<IReadOnlyList<string>> ReadCsvHeadersAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        stream.Position = 0;

        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                return ParseCsvLine(line)
                    .Select(header => header.Trim())
                    .Where(header => !string.IsNullOrWhiteSpace(header))
                    .ToArray();
            }
        }

        return [];
    }

    private static IReadOnlyList<string> ReadXlsxHeaders(Stream stream)
    {
        stream.Position = 0;

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var worksheetEntry = ResolveFirstWorksheetEntry(archive);
        if (worksheetEntry is null)
        {
            return [];
        }

        var sharedStrings = ReadSharedStrings(archive);
        using var worksheetStream = worksheetEntry.Open();
        var worksheetDocument = XDocument.Load(worksheetStream);
        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var headerRow = worksheetDocument.Root?
            .Element(worksheetNs + "sheetData")?
            .Elements(worksheetNs + "row")
            .FirstOrDefault();
        if (headerRow is null)
        {
            return [];
        }

        return headerRow.Elements(worksheetNs + "c")
            .Select(cell => new
            {
                ColumnIndex = GetColumnIndex((string?)cell.Attribute("r")),
                Value = ReadCellValue(cell, sharedStrings, worksheetNs)
            })
            .Where(cell => !string.IsNullOrWhiteSpace(cell.Value))
            .OrderBy(cell => cell.ColumnIndex)
            .Select(cell => cell.Value.Trim())
            .ToArray();
    }

    private static ZipArchiveEntry? ResolveFirstWorksheetEntry(ZipArchive archive)
    {
        XNamespace spreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace relationshipsNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        var workbookEntry = archive.GetEntry("xl/workbook.xml");
        var workbookRelationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
        if (workbookEntry is null || workbookRelationshipsEntry is null)
        {
            return archive.GetEntry("xl/worksheets/sheet1.xml");
        }

        using var workbookStream = workbookEntry.Open();
        using var workbookRelationshipsStream = workbookRelationshipsEntry.Open();
        var workbookDocument = XDocument.Load(workbookStream);
        var relationshipsDocument = XDocument.Load(workbookRelationshipsStream);

        var firstSheet = workbookDocument.Root?
            .Element(spreadsheetNs + "sheets")?
            .Elements(spreadsheetNs + "sheet")
            .FirstOrDefault();
        var relationshipId = (string?)firstSheet?.Attribute(XName.Get("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"));
        if (string.IsNullOrWhiteSpace(relationshipId))
        {
            return archive.GetEntry("xl/worksheets/sheet1.xml");
        }

        var target = relationshipsDocument.Root?
            .Elements(relationshipsNs + "Relationship")
            .FirstOrDefault(relationship => string.Equals((string?)relationship.Attribute("Id"), relationshipId, StringComparison.Ordinal))?
            .Attribute("Target")?
            .Value;

        if (string.IsNullOrWhiteSpace(target))
        {
            return archive.GetEntry("xl/worksheets/sheet1.xml");
        }

        var normalizedTarget = target.Replace('\\', '/');
        if (!normalizedTarget.StartsWith("/", StringComparison.Ordinal))
        {
            normalizedTarget = $"xl/{normalizedTarget.TrimStart('/')}";
        }

        return archive.GetEntry(normalizedTarget.TrimStart('/'));
    }

    private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
    {
        XNamespace spreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (sharedStringsEntry is null)
        {
            return [];
        }

        using var sharedStringsStream = sharedStringsEntry.Open();
        var sharedStringsDocument = XDocument.Load(sharedStringsStream);

        return sharedStringsDocument.Root?
            .Elements(spreadsheetNs + "si")
            .Select(item => string.Concat(
                item.Descendants(spreadsheetNs + "t")
                    .Select(text => text.Value)))
            .ToArray()
            ?? [];
    }

    private static string ReadCellValue(
        XElement cell,
        IReadOnlyList<string> sharedStrings,
        XNamespace worksheetNs)
    {
        var value = cell.Element(worksheetNs + "v")?.Value ?? string.Empty;
        var type = (string?)cell.Attribute("t");

        return type switch
        {
            "s" when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)
                && index >= 0
                && index < sharedStrings.Count => sharedStrings[index],
            "inlineStr" => string.Concat(cell.Descendants(worksheetNs + "t").Select(text => text.Value)),
            _ => value
        };
    }

    private static int GetColumnIndex(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return int.MaxValue;
        }

        var columnLetters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        var index = 0;

        foreach (var character in columnLetters.ToUpperInvariant())
        {
            index = (index * 26) + (character - 'A' + 1);
        }

        return index;
    }

    private static string? SuggestFieldKey(string sourceColumnName)
    {
        if (HeaderAliases.TryGetValue(sourceColumnName.Trim(), out var directMatch))
        {
            return directMatch;
        }

        var normalizedHeader = Normalize(sourceColumnName);
        foreach (var field in AvailableFields)
        {
            if (normalizedHeader == Normalize(field.Label))
            {
                return field.Key;
            }
        }

        return null;
    }

    private static string Normalize(string value)
    {
        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var buffer = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    buffer.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ',' && !inQuotes)
            {
                values.Add(buffer.ToString());
                buffer.Clear();
            }
            else
            {
                buffer.Append(character);
            }
        }

        values.Add(buffer.ToString());
        return values;
    }
}
