using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPmProjectTraceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PmStepCycleTimeChangeRequest_StepId",
                table: "PmStepCycleTimeChangeRequest");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "PmProjectTrace",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "PmProjectTrace",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PmStepCycleTimeChangeRequest_StepId_ApprovalStatus",
                table: "PmStepCycleTimeChangeRequest",
                columns: new[] { "StepId", "ApprovalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTrace_CustomerName_ProductCode_Status_CreatedAt",
                table: "PmProjectTrace",
                columns: new[] { "CustomerName", "ProductCode", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PmStepCycleTimeChangeRequest_StepId_ApprovalStatus",
                table: "PmStepCycleTimeChangeRequest");

            migrationBuilder.DropIndex(
                name: "IX_PmProjectTrace_CustomerName_ProductCode_Status_CreatedAt",
                table: "PmProjectTrace");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "PmProjectTrace",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "PmProjectTrace",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_PmStepCycleTimeChangeRequest_StepId",
                table: "PmStepCycleTimeChangeRequest",
                column: "StepId");
        }
    }
}
