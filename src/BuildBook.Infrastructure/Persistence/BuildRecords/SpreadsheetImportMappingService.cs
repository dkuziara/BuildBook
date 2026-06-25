using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using BuildBook.Application.BuildRecords;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class SpreadsheetImportMappingService : ISpreadsheetImportMappingService
{
    private const int MaximumPreviewRows = 10;
    private const string MaskedPreviewValue = "************";

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

    private static readonly Dictionary<string, SpreadsheetImportFieldOption> FieldLookup = AvailableFields.ToDictionary(
        field => field.Key,
        field => field,
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

        var worksheetData = await ReadWorksheetDataAsync(fileName, fileStream, cancellationToken);

        return new SpreadsheetColumnMappingReview
        {
            AvailableFields = AvailableFields,
            Notices = worksheetData.Notices,
            ColumnMappings = worksheetData.Headers
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(header => new SpreadsheetImportColumnMapping
                {
                    SourceColumnName = header,
                    SuggestedFieldKey = SuggestFieldKey(header)
                })
                .ToArray()
        };
    }

    public async Task<SpreadsheetImportPreview> BuildPreviewAsync(
        string fileName,
        Stream fileStream,
        IReadOnlyDictionary<string, string> selectedMappings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentNullException.ThrowIfNull(selectedMappings);

        var worksheetData = await ReadWorksheetDataAsync(fileName, fileStream, cancellationToken);
        var effectiveMappings = selectedMappings
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.Value))
            .Where(mapping => worksheetData.Headers.Contains(mapping.Key, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        var previewColumns = effectiveMappings
            .Select(mapping =>
            {
                var field = FieldLookup[mapping.Value];
                return new SpreadsheetImportPreviewColumn
                {
                    FieldKey = field.Key,
                    FieldLabel = field.Label,
                    SourceColumnName = mapping.Key,
                    IsSensitive = field.IsSensitive
                };
            })
            .ToArray();

        var rowIndexByHeader = worksheetData.Headers
            .Select((header, index) => new { header, index })
            .ToDictionary(item => item.header, item => item.index, StringComparer.OrdinalIgnoreCase);

        var previewRows = worksheetData.Rows
            .Take(MaximumPreviewRows)
            .Select(row => new SpreadsheetImportPreviewRow
            {
                SourceRowNumber = row.RowNumber,
                Values = previewColumns.ToDictionary(
                    column => column.FieldKey,
                    column =>
                    {
                        var value = row.Cells[rowIndexByHeader[column.SourceColumnName]];
                        return column.IsSensitive && !string.IsNullOrWhiteSpace(value)
                            ? MaskedPreviewValue
                            : value;
                    },
                    StringComparer.OrdinalIgnoreCase)
            })
            .ToArray();

        var notices = worksheetData.Notices.ToList();
        if (previewRows.Length > 0)
        {
            notices.Add("Sensitive values are masked in the preview.");
        }

        if (worksheetData.Rows.Count > MaximumPreviewRows)
        {
            notices.Add($"Showing the first {MaximumPreviewRows} rows of {worksheetData.Rows.Count} data rows.");
        }

        return new SpreadsheetImportPreview
        {
            Columns = previewColumns,
            Rows = previewRows,
            Notices = notices,
            RowsRead = worksheetData.Rows.Count,
            RowsShown = previewRows.Length
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

    private static async Task<WorksheetData> ReadWorksheetDataAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return await ReadCsvWorksheetDataAsync(fileStream, cancellationToken);
        }

        if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return ReadXlsxWorksheetData(fileStream);
        }

        if (extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
        {
            return new WorksheetData([], [], ["Header review and preview are available for .xlsx and .csv files. Save legacy .xls files as .xlsx or .csv to continue."]);
        }

        return new WorksheetData([], [], ["No spreadsheet headers were detected. Check the first row and try again."]);
    }

    private static async Task<WorksheetData> ReadCsvWorksheetDataAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        stream.Position = 0;

        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        string[] headers = [];
        var rows = new List<WorksheetRow>();
        var sourceRowNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            sourceRowNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cells = ParseCsvLine(line).Select(value => value.Trim()).ToArray();
            if (headers.Length == 0)
            {
                headers = cells.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
                continue;
            }

            rows.Add(new WorksheetRow(sourceRowNumber, NormalizeRowCells(cells, headers.Length)));
        }

        return CreateWorksheetData(headers, rows);
    }

    private static WorksheetData ReadXlsxWorksheetData(Stream stream)
    {
        stream.Position = 0;

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var worksheetEntry = ResolveFirstWorksheetEntry(archive);
        if (worksheetEntry is null)
        {
            return new WorksheetData([], [], ["No spreadsheet headers were detected. Check the first row and try again."]);
        }

        var sharedStrings = ReadSharedStrings(archive);
        using var worksheetStream = worksheetEntry.Open();
        var worksheetDocument = XDocument.Load(worksheetStream);
        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var worksheetRows = worksheetDocument.Root?
            .Element(worksheetNs + "sheetData")?
            .Elements(worksheetNs + "row")
            .ToArray()
            ?? [];

        if (worksheetRows.Length == 0)
        {
            return new WorksheetData([], [], ["No spreadsheet headers were detected. Check the first row and try again."]);
        }

        var headers = ReadRowCells(worksheetRows[0], sharedStrings, worksheetNs)
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        var dataRows = worksheetRows
            .Skip(1)
            .Select(row =>
            {
                var rowNumber = int.TryParse((string?)row.Attribute("r"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedRowNumber)
                    ? parsedRowNumber
                    : 0;
                var cells = NormalizeRowCells(ReadRowCells(row, sharedStrings, worksheetNs), headers.Length);
                return new WorksheetRow(rowNumber, cells);
            })
            .ToList();

        return CreateWorksheetData(headers, dataRows);
    }

    private static WorksheetData CreateWorksheetData(
        IReadOnlyList<string> headers,
        IReadOnlyList<WorksheetRow> rows)
    {
        if (headers.Count == 0)
        {
            return new WorksheetData([], [], ["No spreadsheet headers were detected. Check the first row and try again."]);
        }

        var dataRows = rows
            .Where(row => row.Cells.Any(cell => !string.IsNullOrWhiteSpace(cell)))
            .ToArray();

        return new WorksheetData(headers.ToArray(), dataRows, []);
    }

    private static string[] NormalizeRowCells(IReadOnlyList<string> cells, int headerCount)
    {
        var normalized = new string[headerCount];
        for (var index = 0; index < headerCount; index++)
        {
            normalized[index] = index < cells.Count ? cells[index].Trim() : string.Empty;
        }

        return normalized;
    }

    private static string[] ReadRowCells(
        XElement row,
        IReadOnlyList<string> sharedStrings,
        XNamespace worksheetNs)
    {
        var cells = row.Elements(worksheetNs + "c")
            .Select(cell => new
            {
                ColumnIndex = GetColumnIndex((string?)cell.Attribute("r")),
                Value = ReadCellValue(cell, sharedStrings, worksheetNs)
            })
            .ToArray();
        if (cells.Length == 0)
        {
            return [];
        }

        var values = new string[cells.Max(cell => cell.ColumnIndex)];
        foreach (var cell in cells)
        {
            values[cell.ColumnIndex - 1] = cell.Value;
        }

        return values.Select(value => value ?? string.Empty).ToArray();
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
            .Select(item => string.Concat(item.Descendants(spreadsheetNs + "t").Select(text => text.Value)))
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

    private sealed record WorksheetData(
        IReadOnlyList<string> Headers,
        IReadOnlyList<WorksheetRow> Rows,
        IReadOnlyList<string> Notices);

    private sealed record WorksheetRow(
        int RowNumber,
        IReadOnlyList<string> Cells);
}
