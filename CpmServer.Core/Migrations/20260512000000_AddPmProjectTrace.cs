using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPmProjectTrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PmProjectTrace",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlannedQty = table.Column<int>(type: "int", nullable: true),
                    ProjectStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayWeeks = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmProjectTrace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PmProjectTraceStep",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectTraceId = table.Column<long>(type: "bigint", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    ProcessName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonInCharge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CycleTime = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SettingDays = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanDurationDays = table.Column<int>(type: "int", nullable: true),
                    PlanStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualForecastStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDurationDays = table.Column<int>(type: "int", nullable: true),
                    ActualPlanDurationDays = table.Column<int>(type: "int", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmProjectTraceStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PmProjectTraceStep_PmProjectTrace_ProjectTraceId",
                        column: x => x.ProjectTraceId,
                        principalTable: "PmProjectTrace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTrace_CustomerId",
                table: "PmProjectTrace",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTrace_QuotationId",
                table: "PmProjectTrace",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTraceStep_ProjectTraceId",
                table: "PmProjectTraceStep",
                column: "ProjectTraceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PmProjectTraceStep");

            migrationBuilder.DropTable(
                name: "PmProjectTrace");
        }
    }
}
