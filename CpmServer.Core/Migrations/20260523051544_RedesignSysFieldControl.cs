using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class RedesignSysFieldControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysFieldControl_ModuleCode_FieldCode_Site",
                table: "SysFieldControl");

            migrationBuilder.DropColumn(
                name: "FieldName",
                table: "SysFieldControl");

            migrationBuilder.DropColumn(
                name: "FieldNameEn",
                table: "SysFieldControl");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "SysFieldControl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PageCode",
                table: "SysFieldControl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SysFieldControl_ModuleCode_PageCode_FieldCode_Site",
                table: "SysFieldControl",
                columns: new[] { "ModuleCode", "PageCode", "FieldCode", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysFieldControl_ModuleCode_PageCode_FieldCode_Site",
                table: "SysFieldControl");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "SysFieldControl");

            migrationBuilder.DropColumn(
                name: "PageCode",
                table: "SysFieldControl");

            migrationBuilder.AddColumn<string>(
                name: "FieldName",
                table: "SysFieldControl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FieldNameEn",
                table: "SysFieldControl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SysFieldControl_ModuleCode_FieldCode_Site",
                table: "SysFieldControl",
                columns: new[] { "ModuleCode", "FieldCode", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");
        }
    }
}
