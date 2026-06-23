using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildBook.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Imports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImportedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ImportedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RowsRead = table.Column<int>(type: "int", nullable: false),
                    RecordsCreated = table.Column<int>(type: "int", nullable: false),
                    RecordsSkipped = table.Column<int>(type: "int", nullable: false),
                    WarningCount = table.Column<int>(type: "int", nullable: false),
                    ErrorCount = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuildRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductClassification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InternalStatus = table.Column<int>(type: "int", nullable: true),
                    AssembledIn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssembledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAssembled = table.Column<DateOnly>(type: "date", nullable: true),
                    HardwareManufacturer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturerPartNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturerRevision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturerSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    CustomerOrder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OANumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateShipped = table.Column<DateOnly>(type: "date", nullable: true),
                    ShippingNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PanelDeviceModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PanelDeviceSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PanelFirmwareVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RadioSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RouterUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HardwareNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiskImageVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RadSightVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WindowsVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WindowsLatestPatch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BleuvioFirmwareVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CharthouseIrdaFirmwareVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RadioFirmware = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RadSightUserLogin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KioskUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WindowsAdminUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WifiSsid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PackingList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalSpreadsheetRowNumber = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildRecords_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BuildRecordAudit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    User = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuildRecordId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<int>(type: "int", nullable: false),
                    FieldChanged = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildRecordAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildRecordAudit_BuildRecords_BuildRecordId",
                        column: x => x.BuildRecordId,
                        principalTable: "BuildRecords",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BuildRecordAudit_Imports_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "Imports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BuildRecordSecrets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildRecordId = table.Column<int>(type: "int", nullable: false),
                    SecretType = table.Column<int>(type: "int", nullable: false),
                    SecretValueEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildRecordSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildRecordSecrets_BuildRecords_BuildRecordId",
                        column: x => x.BuildRecordId,
                        principalTable: "BuildRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecordAudit_BuildRecordId",
                table: "BuildRecordAudit",
                column: "BuildRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecordAudit_ImportBatchId",
                table: "BuildRecordAudit",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_CustomerId",
                table: "BuildRecords",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecordSecrets_BuildRecordId",
                table: "BuildRecordSecrets",
                column: "BuildRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildRecordAudit");

            migrationBuilder.DropTable(
                name: "BuildRecordSecrets");

            migrationBuilder.DropTable(
                name: "Imports");

            migrationBuilder.DropTable(
                name: "BuildRecords");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
