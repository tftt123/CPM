using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleTypeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SysModuleTypeConfig",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysModuleTypeConfig", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysModuleTypeConfig_ModuleType",
                table: "SysModuleTypeConfig",
                column: "ModuleType",
                unique: true);

            // Seed data
            migrationBuilder.InsertData(
                table: "SysModuleTypeConfig",
                columns: new[] { "ModuleType", "ModuleName", "Description", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { "Quotation", "报价单", "报价单审批", true, new DateTime(2026, 5, 12) },
                    { "Sample", "样品", "样品审批", true, new DateTime(2026, 5, 12) },
                    { "Procurement", "采购", "采购审批", true, new DateTime(2026, 5, 12) },
                    { "PmStepCycleTime", "实际节拍维护", "实际节拍变更审批", true, new DateTime(2026, 5, 12) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysModuleTypeConfig");
        }
    }
}
