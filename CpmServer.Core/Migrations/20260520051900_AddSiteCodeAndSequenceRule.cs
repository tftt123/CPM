using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteCodeAndSequenceRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SiteCode",
                table: "SysSiteSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SysSequenceRule",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Prefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentSequence = table.Column<long>(type: "bigint", nullable: false),
                    SequenceLength = table.Column<int>(type: "int", nullable: false),
                    ResetRule = table.Column<int>(type: "int", nullable: false),
                    LastResetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastGeneratedNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Site = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysSequenceRule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysSequenceRule_ModuleType_Site",
                table: "SysSequenceRule",
                columns: new[] { "ModuleType", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysSequenceRule");

            migrationBuilder.DropColumn(
                name: "SiteCode",
                table: "SysSiteSettings");
        }
    }
}
