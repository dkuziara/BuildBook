using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildBook.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderProductCodeLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "OrderRecords",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_ProductCode",
                table: "OrderRecords",
                column: "ProductCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderRecords_ProductCode",
                table: "OrderRecords");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "OrderRecords");
        }
    }
}
