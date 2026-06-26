using System.IO.Compression;
using System.Text;
using BuildBook.Application.BuildRecords;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRegisterExcelExporter(
    IBuildRegisterReader buildRegisterReader) : IBuildRegisterExcelExporter
{
    public async Task<byte[]> ExportAsync(
        BuildRegisterFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await buildRegisterReader.ListAsync(filter, cancellationToken);
        using var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            CreateEntry(archive, "[Content_Types].xml", ContentTypesXml);
            CreateEntry(archive, "_rels/.rels", RootRelationshipsXml);
            CreateEntry(archive, "xl/workbook.xml", WorkbookXml);
            CreateEntry(archive, "xl/_rels/workbook.xml.rels", WorkbookRelationshipsXml);
            CreateEntry(archive, "xl/styles.xml", StylesXml);
            CreateEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(rows));
        }

        return stream.ToArray();
    }

    private static void CreateEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);

        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content);
    }

    private static string BuildWorksheetXml(IReadOnlyList<BuildRegisterRow> rows)
    {
        var builder = new StringBuilder();
        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        builder.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
        builder.Append("<sheetData>");

        AppendRow(builder, 1, [.. BuildRegisterExportProjection.Headers]);

        for (var index = 0; index < rows.Count; index++)
        {
            AppendRow(builder, index + 2, BuildRegisterExportProjection.Project(rows[index]));
        }

        builder.Append("</sheetData></worksheet>");
        return builder.ToString();
    }

    private static void AppendRow(StringBuilder builder, int rowNumber, params string?[] values)
    {
        builder.Append("<row r=\"");
        builder.Append(rowNumber);
        builder.Append("\">");

        for (var columnIndex = 0; columnIndex < values.Length; columnIndex++)
        {
            AppendCell(builder, rowNumber, columnIndex, values[columnIndex]);
        }

        builder.Append("</row>");
    }

    private static void AppendCell(StringBuilder builder, int rowNumber, int columnIndex, string? value)
    {
        builder.Append("<c r=\"");
        builder.Append(GetCellReference(columnIndex, rowNumber));
        builder.Append("\" t=\"inlineStr\"><is><t");

        if (value?.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])))
        {
            builder.Append(" xml:space=\"preserve\"");
        }

        builder.Append(">");
        builder.Append(EscapeXml(value));
        builder.Append("</t></is></c>");
    }

    private static string GetCellReference(int columnIndex, int rowNumber)
    {
        var dividend = columnIndex + 1;
        var columnName = new StringBuilder();

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName.Insert(0, (char)('A' + modulo));
            dividend = (dividend - modulo) / 26;
        }

        columnName.Append(rowNumber);
        return columnName.ToString();
    }

    private static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
    }
    private const string ContentTypesXml =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
          <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
          <Default Extension="xml" ContentType="application/xml"/>
          <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
          <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
          <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
        </Types>
        """;

    private const string RootRelationshipsXml =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
        </Relationships>
        """;

    private const string WorkbookXml =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                  xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
          <sheets>
            <sheet name="Build Register" sheetId="1" r:id="rId1"/>
          </sheets>
        </workbook>
        """;

    private const string WorkbookRelationshipsXml =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
          <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
        </Relationships>
        """;

    private const string StylesXml =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <fonts count="1">
            <font>
              <sz val="11"/>
              <name val="Calibri"/>
            </font>
          </fonts>
          <fills count="1">
            <fill>
              <patternFill patternType="none"/>
            </fill>
          </fills>
          <borders count="1">
            <border/>
          </borders>
          <cellStyleXfs count="1">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
          </cellStyleXfs>
          <cellXfs count="1">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
          </cellXfs>
        </styleSheet>
        """;
}
