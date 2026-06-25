using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildBook.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildRecordSecretTypeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildRecordSecrets_BuildRecordId",
                table: "BuildRecordSecrets");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecordSecrets_BuildRecordId_SecretType",
                table: "BuildRecordSecrets",
                columns: new[] { "BuildRecordId", "SecretType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildRecordSecrets_BuildRecordId_SecretType",
                table: "BuildRecordSecrets");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRecordSecrets_BuildRecordId",
                table: "BuildRecordSecrets",
                column: "BuildRecordId");
        }
    }
}
