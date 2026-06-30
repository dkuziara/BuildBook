using BuildBook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildBook.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(BuildBookDbContext))]
    [Migration("20260630113000_AddRmaPlannerMigrationTraceability")]
    public partial class AddRmaPlannerMigrationTraceability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MigrationSource",
                table: "RmaRecords",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalPlannerNotes",
                table: "RmaRecords",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalPlannerTaskTitle",
                table: "RmaRecords",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MigrationSource",
                table: "RmaRecords");

            migrationBuilder.DropColumn(
                name: "OriginalPlannerNotes",
                table: "RmaRecords");

            migrationBuilder.DropColumn(
                name: "OriginalPlannerTaskTitle",
                table: "RmaRecords");
        }

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
        }
    }
}
