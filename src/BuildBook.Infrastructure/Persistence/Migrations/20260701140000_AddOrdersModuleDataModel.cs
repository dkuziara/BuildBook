using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildBook.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersModuleDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderImportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PlanId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExportDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ImportedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ImportedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowsRead = table.Column<int>(type: "int", nullable: false),
                    OrdersCreated = table.Column<int>(type: "int", nullable: false),
                    OrdersUpdated = table.Column<int>(type: "int", nullable: false),
                    OrdersSkipped = table.Column<int>(type: "int", nullable: false),
                    Warnings = table.Column<int>(type: "int", nullable: false),
                    Errors = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderImportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderImportBatches_ApplicationUsers_ImportedByUserId",
                        column: x => x.ImportedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrderTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    ImportedPriorityText = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    ImportedCompletedByText = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ImportedCreatedByText = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    PlannerTaskId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PlannerPlanId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PlannerBucketId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PlannerBucketName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PlannerSource = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PlannerStatus = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PlannerGoal = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ImportedLateFlag = table.Column<bool>(type: "bit", nullable: true),
                    CustomerReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CustomerPurchaseOrderNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    InternalOrderReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    QuoteNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SalesAdminOwnerUserId = table.Column<int>(type: "int", nullable: true),
                    ProductionOwnerUserId = table.Column<int>(type: "int", nullable: true),
                    NotesSummary = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    SupportTicketNo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ShippingRequired = table.Column<bool>(type: "bit", nullable: true),
                    ShippingMethod = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Courier = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CollectionRequired = table.Column<bool>(type: "bit", nullable: true),
                    CollectionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ShippedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ShippedByUserId = table.Column<int>(type: "int", nullable: true),
                    ShippingNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractReadyForInvoicing = table.Column<bool>(type: "bit", nullable: true),
                    ReadyForInvoicingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    InvoicedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InvoicedByUserId = table.Column<int>(type: "int", nullable: true),
                    InvoicingNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderRecords_ApplicationUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderRecords_ApplicationUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderRecords_ApplicationUsers_InvoicedByUserId",
                        column: x => x.InvoicedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderRecords_ApplicationUsers_LastUpdatedByUserId",
                        column: x => x.LastUpdatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderRecords_ApplicationUsers_ProductionOwnerUserId",
                        column: x => x.ProductionOwnerUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderRecords_ApplicationUsers_SalesAdminOwnerUserId",
                        column: x => x.SalesAdminOwnerUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderRecords_ApplicationUsers_ShippedByUserId",
                        column: x => x.ShippedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderRecords_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderImportWarnings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderImportBatchId = table.Column<int>(type: "int", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: true),
                    PlannerTaskId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    WarningType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderImportWarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderImportWarnings_OrderImportBatches_OrderImportBatchId",
                        column: x => x.OrderImportBatchId,
                        principalTable: "OrderImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderRecordId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: true),
                    ImportedUserText = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AssignmentType = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAssignments_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderAssignments_ApplicationUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderAssignments_OrderRecords_OrderRecordId",
                        column: x => x.OrderRecordId,
                        principalTable: "OrderRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderBuildRecordLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderRecordId = table.Column<int>(type: "int", nullable: false),
                    BuildRecordId = table.Column<int>(type: "int", nullable: false),
                    LinkType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LinkedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LinkedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderBuildRecordLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderBuildRecordLinks_ApplicationUsers_LinkedByUserId",
                        column: x => x.LinkedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderBuildRecordLinks_BuildRecords_BuildRecordId",
                        column: x => x.BuildRecordId,
                        principalTable: "BuildRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderBuildRecordLinks_OrderRecords_OrderRecordId",
                        column: x => x.OrderRecordId,
                        principalTable: "OrderRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderRecordId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ImportedCompletedText = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShowInBoardView = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderChecklistItems_ApplicationUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderChecklistItems_OrderRecords_OrderRecordId",
                        column: x => x.OrderRecordId,
                        principalTable: "OrderRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderLabels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderRecordId = table.Column<int>(type: "int", nullable: false),
                    LabelText = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLabels_OrderRecords_OrderRecordId",
                        column: x => x.OrderRecordId,
                        principalTable: "OrderRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderRecordId = table.Column<int>(type: "int", nullable: false),
                    NoteType = table.Column<int>(type: "int", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderNotes_ApplicationUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderNotes_ApplicationUsers_LastUpdatedByUserId",
                        column: x => x.LastUpdatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderNotes_OrderRecords_OrderRecordId",
                        column: x => x.OrderRecordId,
                        principalTable: "OrderRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderRecordId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_ApplicationUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_OrderRecords_OrderRecordId",
                        column: x => x.OrderRecordId,
                        principalTable: "OrderRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignments_ApplicationUserId",
                table: "OrderAssignments",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignments_AssignedAt",
                table: "OrderAssignments",
                column: "AssignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignments_AssignedByUserId",
                table: "OrderAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignments_OrderRecordId",
                table: "OrderAssignments",
                column: "OrderRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBuildRecordLinks_BuildRecordId",
                table: "OrderBuildRecordLinks",
                column: "BuildRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBuildRecordLinks_LinkedByUserId",
                table: "OrderBuildRecordLinks",
                column: "LinkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBuildRecordLinks_OrderRecordId",
                table: "OrderBuildRecordLinks",
                column: "OrderRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBuildRecordLinks_OrderRecordId_BuildRecordId",
                table: "OrderBuildRecordLinks",
                columns: new[] { "OrderRecordId", "BuildRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderChecklistItems_CompletedByUserId",
                table: "OrderChecklistItems",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChecklistItems_DisplayOrder",
                table: "OrderChecklistItems",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChecklistItems_IsCompleted",
                table: "OrderChecklistItems",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChecklistItems_OrderRecordId",
                table: "OrderChecklistItems",
                column: "OrderRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChecklistItems_ShowInBoardView",
                table: "OrderChecklistItems",
                column: "ShowInBoardView");

            migrationBuilder.CreateIndex(
                name: "IX_OrderImportBatches_ImportedAt",
                table: "OrderImportBatches",
                column: "ImportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderImportBatches_ImportedByUserId",
                table: "OrderImportBatches",
                column: "ImportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderImportBatches_PlanId",
                table: "OrderImportBatches",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderImportWarnings_OrderImportBatchId",
                table: "OrderImportWarnings",
                column: "OrderImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderImportWarnings_PlannerTaskId",
                table: "OrderImportWarnings",
                column: "PlannerTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderImportWarnings_Severity",
                table: "OrderImportWarnings",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLabels_LabelText",
                table: "OrderLabels",
                column: "LabelText");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLabels_OrderRecordId",
                table: "OrderLabels",
                column: "OrderRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotes_CreatedAt",
                table: "OrderNotes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotes_CreatedByUserId",
                table: "OrderNotes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotes_LastUpdatedByUserId",
                table: "OrderNotes",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotes_NoteType",
                table: "OrderNotes",
                column: "NoteType");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotes_OrderRecordId",
                table: "OrderNotes",
                column: "OrderRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_CompletedByUserId",
                table: "OrderRecords",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_CreatedByUserId",
                table: "OrderRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_CustomerId",
                table: "OrderRecords",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_DueDate",
                table: "OrderRecords",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_InvoicedByUserId",
                table: "OrderRecords",
                column: "InvoicedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_InvoiceNumber",
                table: "OrderRecords",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_LastUpdatedAt",
                table: "OrderRecords",
                column: "LastUpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_LastUpdatedByUserId",
                table: "OrderRecords",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_OrderNumber",
                table: "OrderRecords",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_PlannerTaskId",
                table: "OrderRecords",
                column: "PlannerTaskId",
                unique: true,
                filter: "[PlannerTaskId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_Priority",
                table: "OrderRecords",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_ProductionOwnerUserId",
                table: "OrderRecords",
                column: "ProductionOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_SalesAdminOwnerUserId",
                table: "OrderRecords",
                column: "SalesAdminOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_ShippedByUserId",
                table: "OrderRecords",
                column: "ShippedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_StartDate",
                table: "OrderRecords",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_Status",
                table: "OrderRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecords_SupportTicketNo",
                table: "OrderRecords",
                column: "SupportTicketNo");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_ChangedAt",
                table: "OrderStatusHistory",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_ChangedByUserId",
                table: "OrderStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OrderRecordId",
                table: "OrderStatusHistory",
                column: "OrderRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderAssignments");

            migrationBuilder.DropTable(
                name: "OrderBuildRecordLinks");

            migrationBuilder.DropTable(
                name: "OrderChecklistItems");

            migrationBuilder.DropTable(
                name: "OrderImportWarnings");

            migrationBuilder.DropTable(
                name: "OrderLabels");

            migrationBuilder.DropTable(
                name: "OrderNotes");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "OrderImportBatches");

            migrationBuilder.DropTable(
                name: "OrderRecords");
        }
    }
}
