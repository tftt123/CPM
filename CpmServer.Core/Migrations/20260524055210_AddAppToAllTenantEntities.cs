using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAppToAllTenantEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysSequenceRule_ModuleType_Site] ON [SysSequenceRule];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysNavigationConfig_NavCode_Site] ON [SysNavigationConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysNavigationConfig_Site] ON [SysNavigationConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysModuleTypeConfig_ModuleType] ON [SysModuleTypeConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysI18nMessage_MessageKey_Site] ON [SysI18nMessage];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysFieldControl_ModuleCode_PageCode_FieldCode_Site] ON [SysFieldControl];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysEmailTemplate_Site] ON [SysEmailTemplate];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysEmailTemplate_TemplateCode_Site] ON [SysEmailTemplate];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysEmailLog_Site] ON [SysEmailLog];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysEmailConfig_Site] ON [SysEmailConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysApprovalTemplate_TemplateCode] ON [SysApprovalTemplate];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysAlertRecipient_Site] ON [SysAlertRecipient];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_QuoQuotation_QuotationNo] ON [QuoQuotation];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_QuoOpportunity_OpportunityNo] ON [QuoOpportunity];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmStepCycleTimeChangeRequest_Site] ON [PmStepCycleTimeChangeRequest];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmStepCycleTimeChangeDetail_Site] ON [PmStepCycleTimeChangeDetail];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmProjectTraceStepActualCycleTime_Site] ON [PmProjectTraceStepActualCycleTime];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmProjectTraceStep_Site] ON [PmProjectTraceStep];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmProjectTrace_Site] ON [PmProjectTrace];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_MfgSubCategory_Site] ON [MfgSubCategory];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_MfgProcess_Site] ON [MfgProcess];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_MfgEquipment_Site] ON [MfgEquipment];");

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysUser",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysSequenceRule",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysRole",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysNavigationConfig",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysModuleTypeConfig",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "SysModuleTypeConfig",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysI18nMessage",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysFileRecord",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysFieldControl",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysEmailTemplate",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysEmailLog",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysEmailConfig",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysDept",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Site",
                table: "SysApprovalTemplate",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysApprovalTemplate",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysApprovalStep",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysApprovalRule",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "SysApprovalRule",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysApprovalRecord",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysApprovalInstanceTask",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "SysApprovalInstanceTask",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysApprovalInstance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysApprovalCondition",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "SysApprovalCondition",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysAlertRecipient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "QuoQuotationItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Site",
                table: "QuoQuotation",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "QuoQuotation",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Site",
                table: "QuoOpportunity",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "QuoOpportunity",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "PmStepCycleTimeChangeRequest",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "PmStepCycleTimeChangeDetail",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "PmProjectTraceStepActualCycleTime",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "PmProjectTraceStep",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "PmProjectTrace",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "MfgSubCategory",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "MfgProcess",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "MfgEquipment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "CrmProduct",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "CrmCustomer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysSequenceRule_ModuleType_App_Site",
                table: "SysSequenceRule",
                columns: new[] { "ModuleType", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysNavigationConfig_App_Site",
                table: "SysNavigationConfig",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysNavigationConfig_NavCode_App_Site",
                table: "SysNavigationConfig",
                columns: new[] { "NavCode", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysModuleTypeConfig_App_Site",
                table: "SysModuleTypeConfig",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysModuleTypeConfig_ModuleType_App_Site",
                table: "SysModuleTypeConfig",
                columns: new[] { "ModuleType", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysI18nMessage_MessageKey_App_Site",
                table: "SysI18nMessage",
                columns: new[] { "MessageKey", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysFieldControl_ModuleCode_PageCode_FieldCode_App_Site",
                table: "SysFieldControl",
                columns: new[] { "ModuleCode", "PageCode", "FieldCode", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysEmailTemplate_App_Site",
                table: "SysEmailTemplate",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysEmailTemplate_TemplateCode_App_Site",
                table: "SysEmailTemplate",
                columns: new[] { "TemplateCode", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysEmailLog_App_Site",
                table: "SysEmailLog",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysEmailConfig_App_Site",
                table: "SysEmailConfig",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysApprovalTemplate_TemplateCode_App_Site",
                table: "SysApprovalTemplate",
                columns: new[] { "TemplateCode", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysApprovalRule_App_Site",
                table: "SysApprovalRule",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysApprovalInstanceTask_App_Site",
                table: "SysApprovalInstanceTask",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysApprovalCondition_App_Site",
                table: "SysApprovalCondition",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysAlertRecipient_App_Site",
                table: "SysAlertRecipient",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoQuotation_QuotationNo_App_Site",
                table: "QuoQuotation",
                columns: new[] { "QuotationNo", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuoOpportunity_OpportunityNo_App_Site",
                table: "QuoOpportunity",
                columns: new[] { "OpportunityNo", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PmStepCycleTimeChangeRequest_App_Site",
                table: "PmStepCycleTimeChangeRequest",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_PmStepCycleTimeChangeDetail_App_Site",
                table: "PmStepCycleTimeChangeDetail",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTraceStepActualCycleTime_App_Site",
                table: "PmProjectTraceStepActualCycleTime",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTraceStep_App_Site",
                table: "PmProjectTraceStep",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_PmProjectTrace_App_Site",
                table: "PmProjectTrace",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_MfgSubCategory_App_Site",
                table: "MfgSubCategory",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_MfgProcess_App_Site",
                table: "MfgProcess",
                columns: new[] { "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_MfgEquipment_App_Site",
                table: "MfgEquipment",
                columns: new[] { "App", "Site" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysSequenceRule_ModuleType_App_Site] ON [SysSequenceRule];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysNavigationConfig_App_Site] ON [SysNavigationConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysNavigationConfig_NavCode_App_Site] ON [SysNavigationConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysModuleTypeConfig_App_Site] ON [SysModuleTypeConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysModuleTypeConfig_ModuleType_App_Site] ON [SysModuleTypeConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysI18nMessage_MessageKey_App_Site] ON [SysI18nMessage];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysFieldControl_ModuleCode_PageCode_FieldCode_App_Site] ON [SysFieldControl];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysEmailTemplate_App_Site] ON [SysEmailTemplate];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysEmailTemplate_TemplateCode_App_Site] ON [SysEmailTemplate];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysEmailLog_App_Site] ON [SysEmailLog];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysEmailConfig_App_Site] ON [SysEmailConfig];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysApprovalTemplate_TemplateCode_App_Site] ON [SysApprovalTemplate];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysApprovalRule_App_Site] ON [SysApprovalRule];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysApprovalInstanceTask_App_Site] ON [SysApprovalInstanceTask];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysApprovalCondition_App_Site] ON [SysApprovalCondition];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_SysAlertRecipient_App_Site] ON [SysAlertRecipient];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_QuoQuotation_QuotationNo_App_Site] ON [QuoQuotation];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_QuoOpportunity_OpportunityNo_App_Site] ON [QuoOpportunity];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmStepCycleTimeChangeRequest_App_Site] ON [PmStepCycleTimeChangeRequest];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmStepCycleTimeChangeDetail_App_Site] ON [PmStepCycleTimeChangeDetail];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmProjectTraceStepActualCycleTime_App_Site] ON [PmProjectTraceStepActualCycleTime];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmProjectTraceStep_App_Site] ON [PmProjectTraceStep];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_PmProjectTrace_App_Site] ON [PmProjectTrace];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_MfgSubCategory_App_Site] ON [MfgSubCategory];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_MfgProcess_App_Site] ON [MfgProcess];");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_MfgEquipment_App_Site] ON [MfgEquipment];");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysUser");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysSequenceRule");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysRole");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysNavigationConfig");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysModuleTypeConfig");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "SysModuleTypeConfig");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysI18nMessage");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysFileRecord");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysFieldControl");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysEmailTemplate");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysEmailLog");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysEmailConfig");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysDept");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysApprovalTemplate");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysApprovalStep");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysApprovalRule");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "SysApprovalRule");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysApprovalRecord");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysApprovalInstanceTask");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "SysApprovalInstanceTask");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysApprovalInstance");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysApprovalCondition");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "SysApprovalCondition");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysAlertRecipient");

            migrationBuilder.DropColumn(
                name: "App",
                table: "QuoQuotationItem");

            migrationBuilder.DropColumn(
                name: "App",
                table: "QuoQuotation");

            migrationBuilder.DropColumn(
                name: "App",
                table: "QuoOpportunity");

            migrationBuilder.DropColumn(
                name: "App",
                table: "PmStepCycleTimeChangeRequest");

            migrationBuilder.DropColumn(
                name: "App",
                table: "PmStepCycleTimeChangeDetail");

            migrationBuilder.DropColumn(
                name: "App",
                table: "PmProjectTraceStepActualCycleTime");

            migrationBuilder.DropColumn(
                name: "App",
                table: "PmProjectTraceStep");

            migrationBuilder.DropColumn(
                name: "App",
                table: "PmProjectTrace");

            migrationBuilder.DropColumn(
                name: "App",
                table: "MfgSubCategory");

            migrationBuilder.DropColumn(
                name: "App",
                table: "MfgProcess");

            migrationBuilder.DropColumn(
                name: "App",
                table: "MfgEquipment");

            migrationBuilder.DropColumn(
                name: "App",
                table: "CrmProduct");

            migrationBuilder.DropColumn(
                name: "App",
                table: "CrmCustomer");

            migrationBuilder.AlterColumn<string>(
                name: "Site",
                table: "SysApprovalTemplate",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Site",
                table: "QuoQuotation",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Site",
                table: "QuoOpportunity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysSequenceRule_ModuleType_Site",
                table: "SysSequenceRule",
                columns: new[] { "ModuleType", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysNavigationConfig_NavCode_Site",
                table: "SysNavigationConfig",
                columns: new[] { "NavCode", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysNavigationConfig_Site",
                table: "SysNavigationConfig",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_SysModuleTypeConfig_ModuleType",
                table: "SysModuleTypeConfig",
                column: "ModuleType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysI18nMessage_MessageKey_Site",
                table: "SysI18nMessage",
                columns: new[] { "MessageKey", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysFieldControl_ModuleCode_PageCode_FieldCode_Site",
                table: "SysFieldControl",
                columns: new[] { "ModuleCode", "PageCode", "FieldCode", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");

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
                name: "IX_SysApprovalTemplate_TemplateCode",
                table: "SysApprovalTemplate",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysAlertRecipient_Site",
                table: "SysAlertRecipient",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_QuoQuotation_QuotationNo",
                table: "QuoQuotation",
                column: "QuotationNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoOpportunity_OpportunityNo",
                table: "QuoOpportunity",
                column: "OpportunityNo",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_MfgSubCategory_Site",
                table: "MfgSubCategory",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_MfgProcess_Site",
                table: "MfgProcess",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_MfgEquipment_Site",
                table: "MfgEquipment",
                column: "Site");
        }
    }
}
