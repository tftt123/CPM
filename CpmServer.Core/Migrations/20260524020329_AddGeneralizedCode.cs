using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralizedCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SysGeneralizedCode",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Domain = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LabelEn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TagType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Site = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysGeneralizedCode", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysGeneralizedCode_Domain_Code_Site",
                table: "SysGeneralizedCode",
                columns: new[] { "Domain", "Code", "Site" },
                unique: true,
                filter: "[Site] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SysGeneralizedCode_Domain_Site",
                table: "SysGeneralizedCode",
                columns: new[] { "Domain", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysGeneralizedCode_Site",
                table: "SysGeneralizedCode",
                column: "Site");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysGeneralizedCode");
        }
    }
}
