using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleTimeStatusAndChangeRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PmProjectTraceStepActualCycleTime",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PmStepCycleTimeChangeRequest",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepId = table.Column<long>(type: "bigint", nullable: false),
                    TraceId = table.Column<long>(type: "bigint", nullable: false),
                    SubmitterId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmitterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    ApprovalInstanceId = table.Column<long>(type: "bigint", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmStepCycleTimeChangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PmStepCycleTimeChangeRequest_PmProjectTraceStep_StepId",
                        column: x => x.StepId,
                        principalTable: "PmProjectTraceStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PmStepCycleTimeChangeDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    TargetRecordId = table.Column<long>(type: "bigint", nullable: true),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualCycleTime = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmStepCycleTimeChangeDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PmStepCycleTimeChangeDetail_PmStepCycleTimeChangeRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PmStepCycleTimeChangeRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PmStepCycleTimeChangeDetail_RequestId",
                table: "PmStepCycleTimeChangeDetail",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PmStepCycleTimeChangeRequest_StepId",
                table: "PmStepCycleTimeChangeRequest",
                column: "StepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PmStepCycleTimeChangeDetail");

            migrationBuilder.DropTable(
                name: "PmStepCycleTimeChangeRequest");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PmProjectTraceStepActualCycleTime");
        }
    }
}
