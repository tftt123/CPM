using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAppToGeneralizedCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysGeneralizedCode_Domain_Code_Site",
                table: "SysGeneralizedCode");

            migrationBuilder.DropIndex(
                name: "IX_SysGeneralizedCode_Domain_Site",
                table: "SysGeneralizedCode");

            migrationBuilder.AddColumn<string>(
                name: "App",
                table: "SysGeneralizedCode",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysGeneralizedCode_App",
                table: "SysGeneralizedCode",
                column: "App");

            migrationBuilder.CreateIndex(
                name: "IX_SysGeneralizedCode_Domain_App_Site",
                table: "SysGeneralizedCode",
                columns: new[] { "Domain", "App", "Site" });

            migrationBuilder.CreateIndex(
                name: "IX_SysGeneralizedCode_Domain_Code_App_Site",
                table: "SysGeneralizedCode",
                columns: new[] { "Domain", "Code", "App", "Site" },
                unique: true,
                filter: "[App] IS NOT NULL AND [Site] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysGeneralizedCode_App",
                table: "SysGeneralizedCode");

            migrationBuilder.DropIndex(
                name: "IX_SysGeneralizedCode_Domain_App_Site",
                table: "SysGeneralizedCode");

            migrationBuilder.DropIndex(
                name: "IX_SysGeneralizedCode_Domain_Code_App_Site",
                table: "SysGeneralizedCode");

            migrationBuilder.DropColumn(
                name: "App",
                table: "SysGeneralizedCode");

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
        }
    }
}
