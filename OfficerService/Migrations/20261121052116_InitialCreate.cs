using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfficerService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Officers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Officers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficerId = table.Column<int>(type: "int", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Standard = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Findings = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceRecords_Officers_OfficerId",
                        column: x => x.OfficerId,
                        principalTable: "Officers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VerificationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceRequestId = table.Column<int>(type: "int", nullable: false),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    FarmerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedOfficerId = table.Column<int>(type: "int", nullable: true),
                    DecisionRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CallbackStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CallbackAttempts = table.Column<int>(type: "int", nullable: false),
                    LastCallbackAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCallbackError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationRequests_Officers_AssignedOfficerId",
                        column: x => x.AssignedOfficerId,
                        principalTable: "Officers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VerificationRequestId = table.Column<int>(type: "int", nullable: false),
                    SourceQuotationId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Suitability = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SuitabilityNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotations_VerificationRequests_VerificationRequestId",
                        column: x => x.VerificationRequestId,
                        principalTable: "VerificationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficerId = table.Column<int>(type: "int", nullable: false),
                    VerificationRequestId = table.Column<int>(type: "int", nullable: true),
                    TargetFarmerId = table.Column<int>(type: "int", nullable: true),
                    TargetSupplierId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_Officers_OfficerId",
                        column: x => x.OfficerId,
                        principalTable: "Officers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recommendations_VerificationRequests_VerificationRequestId",
                        column: x => x.VerificationRequestId,
                        principalTable: "VerificationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VerificationRequestId = table.Column<int>(type: "int", nullable: false),
                    QuotationId = table.Column<int>(type: "int", nullable: true),
                    Criterion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsCompliant = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CheckedByOfficerId = table.Column<int>(type: "int", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceChecks_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplianceChecks_VerificationRequests_VerificationRequestId",
                        column: x => x.VerificationRequestId,
                        principalTable: "VerificationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_QuotationId",
                table: "ComplianceChecks",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_VerificationRequestId",
                table: "ComplianceChecks",
                column: "VerificationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRecords_OfficerId",
                table: "ComplianceRecords",
                column: "OfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRecords_SubjectType_SubjectId",
                table: "ComplianceRecords",
                columns: new[] { "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_Officers_Email",
                table: "Officers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Officers_EmployeeNumber",
                table: "Officers",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_SourceQuotationId",
                table: "Quotations",
                column: "SourceQuotationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_VerificationRequestId_SupplierId",
                table: "Quotations",
                columns: new[] { "VerificationRequestId", "SupplierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_OfficerId",
                table: "Recommendations",
                column: "OfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_VerificationRequestId",
                table: "Recommendations",
                column: "VerificationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequests_AssignedOfficerId_Status",
                table: "VerificationRequests",
                columns: new[] { "AssignedOfficerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequests_ReferenceNumber",
                table: "VerificationRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequests_SourceRequestId",
                table: "VerificationRequests",
                column: "SourceRequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplianceChecks");

            migrationBuilder.DropTable(
                name: "ComplianceRecords");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "VerificationRequests");

            migrationBuilder.DropTable(
                name: "Officers");
        }
    }
}
