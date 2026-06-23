using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildBook.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "WindowsVersion",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RadSightVersion",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OANumber",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MachineName",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNumber",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerOrder",
                table: "BuildRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_CustomerOrder",
                table: "BuildRecords",
                column: "CustomerOrder");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_DateShipped",
                table: "BuildRecords",
                column: "DateShipped");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_InvoiceNumber",
                table: "BuildRecords",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_LastUpdatedAt",
                table: "BuildRecords",
                column: "LastUpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_MachineName",
                table: "BuildRecords",
                column: "MachineName");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_OANumber",
                table: "BuildRecords",
                column: "OANumber");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_ProductCode",
                table: "BuildRecords",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_ProductName",
                table: "BuildRecords",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_RadSightVersion",
                table: "BuildRecords",
                column: "RadSightVersion");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_SerialNumber",
                table: "BuildRecords",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecords_WindowsVersion",
                table: "BuildRecords",
                column: "WindowsVersion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Name",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_CustomerOrder",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_DateShipped",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_InvoiceNumber",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_LastUpdatedAt",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_MachineName",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_OANumber",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_ProductCode",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_ProductName",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_RadSightVersion",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_SerialNumber",
                table: "BuildRecords");

            migrationBuilder.DropIndex(
                name: "IX_BuildRecords_WindowsVersion",
                table: "BuildRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "WindowsVersion",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "RadSightVersion",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "OANumber",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MachineName",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNumber",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerOrder",
                table: "BuildRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
