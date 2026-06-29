using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildBook.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRmaModuleFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RmaRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BuildRecordId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SupportTicketNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SupportTicketUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    OriginalOrderNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OriginalOrderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OriginalInvoiceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FaultSummary = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    InitialFaultDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FaultDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaultCategory = table.Column<int>(type: "int", nullable: true),
                    FaultSubcategory = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ReportedSymptoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InitialDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RootCause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RootCauseCategory = table.Column<int>(type: "int", nullable: true),
                    RepairActionTaken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarrantyStatus = table.Column<int>(type: "int", nullable: true),
                    WarrantyExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ChargeableRepair = table.Column<bool>(type: "bit", nullable: true),
                    CustomerApprovalRequired = table.Column<bool>(type: "bit", nullable: true),
                    CustomerApprovalReceived = table.Column<bool>(type: "bit", nullable: true),
                    CustomerApprovalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    QuoteNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RepairInvoiceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EstimatedRepairCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualRepairCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateItemReceived = table.Column<DateOnly>(type: "date", nullable: true),
                    ReceivedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TargetCompletionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OnHoldReason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EscalationRequired = table.Column<bool>(type: "bit", nullable: true),
                    EscalatedTo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EscalationNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepairCompletedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RepairCompletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TestRequired = table.Column<bool>(type: "bit", nullable: true),
                    TestPlanUsed = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TestResult = table.Column<int>(type: "int", nullable: true),
                    TestedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TestDate = table.Column<DateOnly>(type: "date", nullable: true),
                    QaRequired = table.Column<bool>(type: "bit", nullable: true),
                    QaResult = table.Column<int>(type: "int", nullable: true),
                    QaCheckedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    QaDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReleaseApproved = table.Column<bool>(type: "bit", nullable: true),
                    ReleaseApprovedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReleaseApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReturnMethod = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Courier = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CollectionArranged = table.Column<bool>(type: "bit", nullable: true),
                    CollectionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ShippedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ShippedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReturnAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProofOfDeliveryReceived = table.Column<bool>(type: "bit", nullable: true),
                    ProofOfDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Outcome = table.Column<int>(type: "int", nullable: true),
                    ClosureNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerFacingSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RmaRecords_BuildRecords_BuildRecordId",
                        column: x => x.BuildRecordId,
                        principalTable: "BuildRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RmaRecords_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RmaAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaRecordId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StoredFilePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AttachmentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RmaAttachments_RmaRecords_RmaRecordId",
                        column: x => x.RmaRecordId,
                        principalTable: "RmaRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RmaAudit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaRecordId = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    User = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FieldChanged = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RmaAudit_RmaRecords_RmaRecordId",
                        column: x => x.RmaRecordId,
                        principalTable: "RmaRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RmaChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaRecordId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ShowInBoardView = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RmaChecklistItems_RmaRecords_RmaRecordId",
                        column: x => x.RmaRecordId,
                        principalTable: "RmaRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RmaCommunications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaRecordId = table.Column<int>(type: "int", nullable: false),
                    CommunicationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ContactMethod = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FollowUpRequired = table.Column<bool>(type: "bit", nullable: false),
                    FollowUpDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaCommunications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RmaCommunications_RmaRecords_RmaRecordId",
                        column: x => x.RmaRecordId,
                        principalTable: "RmaRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RmaNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaRecordId = table.Column<int>(type: "int", nullable: false),
                    NoteType = table.Column<int>(type: "int", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RmaNotes_RmaRecords_RmaRecordId",
                        column: x => x.RmaRecordId,
                        principalTable: "RmaRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RmaParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaRecordId = table.Column<int>(type: "int", nullable: false),
                    PartName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Supplier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RmaParts_RmaRecords_RmaRecordId",
                        column: x => x.RmaRecordId,
                        principalTable: "RmaRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RmaStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RmaRecordId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RmaStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RmaStatusHistory_RmaRecords_RmaRecordId",
                        column: x => x.RmaRecordId,
                        principalTable: "RmaRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RmaAttachments_RmaRecordId",
                table: "RmaAttachments",
                column: "RmaRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_RmaAttachments_UploadedAt",
                table: "RmaAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RmaAudit_OccurredAt",
                table: "RmaAudit",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_RmaAudit_RmaRecordId",
                table: "RmaAudit",
                column: "RmaRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_RmaChecklistItems_RmaRecordId_DisplayOrder",
                table: "RmaChecklistItems",
                columns: new[] { "RmaRecordId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RmaCommunications_CommunicationDate",
                table: "RmaCommunications",
                column: "CommunicationDate");

            migrationBuilder.CreateIndex(
                name: "IX_RmaCommunications_RmaRecordId",
                table: "RmaCommunications",
                column: "RmaRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_RmaNotes_NoteType",
                table: "RmaNotes",
                column: "NoteType");

            migrationBuilder.CreateIndex(
                name: "IX_RmaNotes_RmaRecordId",
                table: "RmaNotes",
                column: "RmaRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_RmaParts_RmaRecordId",
                table: "RmaParts",
                column: "RmaRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_AssignedTo",
                table: "RmaRecords",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_BuildRecordId",
                table: "RmaRecords",
                column: "BuildRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_CustomerId",
                table: "RmaRecords",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_DueDate",
                table: "RmaRecords",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_LastUpdatedAt",
                table: "RmaRecords",
                column: "LastUpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_Priority",
                table: "RmaRecords",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_ProductCode",
                table: "RmaRecords",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_ProductName",
                table: "RmaRecords",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_RmaNumber",
                table: "RmaRecords",
                column: "RmaNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_SerialNumber",
                table: "RmaRecords",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_RmaRecords_Status",
                table: "RmaRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RmaStatusHistory_ChangedAt",
                table: "RmaStatusHistory",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RmaStatusHistory_RmaRecordId",
                table: "RmaStatusHistory",
                column: "RmaRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RmaAttachments");

            migrationBuilder.DropTable(
                name: "RmaAudit");

            migrationBuilder.DropTable(
                name: "RmaChecklistItems");

            migrationBuilder.DropTable(
                name: "RmaCommunications");

            migrationBuilder.DropTable(
                name: "RmaNotes");

            migrationBuilder.DropTable(
                name: "RmaParts");

            migrationBuilder.DropTable(
                name: "RmaStatusHistory");

            migrationBuilder.DropTable(
                name: "RmaRecords");
        }
    }
}
