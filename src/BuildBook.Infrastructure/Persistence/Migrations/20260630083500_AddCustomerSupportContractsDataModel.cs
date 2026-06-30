using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildBook.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSupportContractsDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildRecords_Customers_CustomerId",
                table: "BuildRecords");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Customers",
                newName: "SupportNotes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AccountCode",
                table: "Customers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountyRegion",
                table: "Customers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainEmail",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainPhone",
                table: "Customers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postcode",
                table: "Customers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactEmail",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactName",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactPhone",
                table: "Customers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SupportContractEndDate",
                table: "Customers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupportContractLevelId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SupportContractStartDate",
                table: "Customers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportContractStatus",
                table: "Customers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "No Contract");

            migrationBuilder.AddColumn<string>(
                name: "TownCity",
                table: "Customers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupportContractLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    TargetResponseTimeValue = table.Column<int>(type: "int", nullable: true),
                    TargetResponseTimeUnit = table.Column<int>(type: "int", nullable: true),
                    DefaultRmaPriority = table.Column<int>(type: "int", nullable: true),
                    RmaPriorityWeight = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportContractLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsActive",
                table: "Customers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SupportContractLevelId",
                table: "Customers",
                column: "SupportContractLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SupportContractStatus",
                table: "Customers",
                column: "SupportContractStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SupportContractLevels_DisplayOrder",
                table: "SupportContractLevels",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SupportContractLevels_IsActive",
                table: "SupportContractLevels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupportContractLevels_Name",
                table: "SupportContractLevels",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BuildRecords_Customers_CustomerId",
                table: "BuildRecords",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_SupportContractLevels_SupportContractLevelId",
                table: "Customers",
                column: "SupportContractLevelId",
                principalTable: "SupportContractLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildRecords_Customers_CustomerId",
                table: "BuildRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_SupportContractLevels_SupportContractLevelId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "SupportContractLevels");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IsActive",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SupportContractLevelId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SupportContractStatus",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccountCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CountyRegion",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MainEmail",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MainPhone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PrimaryContactEmail",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PrimaryContactName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PrimaryContactPhone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SupportContractEndDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SupportContractLevelId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SupportContractStartDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SupportContractStatus",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TownCity",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "SupportNotes",
                table: "Customers",
                newName: "Notes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddForeignKey(
                name: "FK_BuildRecords_Customers_CustomerId",
                table: "BuildRecords",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}
