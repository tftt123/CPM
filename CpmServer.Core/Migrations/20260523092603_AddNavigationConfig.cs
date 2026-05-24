using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SysNavigationConfig",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NavCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModuleLabelEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NavLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NavLabelEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoutePath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Site = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysNavigationConfig", x => x.Id);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysNavigationConfig");
        }
    }
}
