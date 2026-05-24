using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteToPM_Email_Alert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "SysEmailTemplate",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "SysEmailLog",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "SysEmailConfig",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "SysAlertRecipient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "PmStepCycleTimeChangeRequest",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "PmStepCycleTimeChangeDetail",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "PmProjectTraceStepActualCycleTime",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "PmProjectTraceStep",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "PmProjectTrace",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysEmailTemplate_Site",
                table: "SysEmailTemplate",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_SysEmailTemplate_TemplateCode_Site",
                table: "SysEmailTemplate",
                columns: new[] { "TemplateCode", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysEmailLog_Site",
                table: "SysEmailLog",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_SysEmailConfig_Site",
                table: "SysEmailConfig",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_SysAlertRecipient_Site",
                table: "SysAlertRecipient",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_PmStepCycleTimeChangeRequest_Site",
                table: "PmStepCycleTimeChangeRequest",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_PmStepCycleTimeChangeDetail_Site",
                table: "PmStepCycleTimeChangeDetail",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTraceStepActualCycleTime_Site",
                table: "PmProjectTraceStepActualCycleTime",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTraceStep_Site",
                table: "PmProjectTraceStep",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTrace_Site",
                table: "PmProjectTrace",
                column: "Site");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysEmailTemplate_Site",
                table: "SysEmailTemplate");

            migrationBuilder.DropIndex(
                name: "IX_SysEmailTemplate_TemplateCode_Site",
                table: "SysEmailTemplate");

            migrationBuilder.DropIndex(
                name: "IX_SysEmailLog_Site",
                table: "SysEmailLog");

            migrationBuilder.DropIndex(
                name: "IX_SysEmailConfig_Site",
                table: "SysEmailConfig");

            migrationBuilder.DropIndex(
                name: "IX_SysAlertRecipient_Site",
                table: "SysAlertRecipient");

            migrationBuilder.DropIndex(
                name: "IX_PmStepCycleTimeChangeRequest_Site",
                table: "PmStepCycleTimeChangeRequest");

            migrationBuilder.DropIndex(
                name: "IX_PmStepCycleTimeChangeDetail_Site",
                table: "PmStepCycleTimeChangeDetail");

            migrationBuilder.DropIndex(
                name: "IX_PmProjectTraceStepActualCycleTime_Site",
                table: "PmProjectTraceStepActualCycleTime");

            migrationBuilder.DropIndex(
                name: "IX_PmProjectTraceStep_Site",
                table: "PmProjectTraceStep");

            migrationBuilder.DropIndex(
                name: "IX_PmProjectTrace_Site",
                table: "PmProjectTrace");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "SysEmailTemplate");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "SysEmailLog");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "SysEmailConfig");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "SysAlertRecipient");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "PmStepCycleTimeChangeRequest");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "PmStepCycleTimeChangeDetail");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "PmProjectTraceStepActualCycleTime");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "PmProjectTraceStep");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "PmProjectTrace");
        }
    }
}
