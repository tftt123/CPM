using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddMfgSiteAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MfgCategory
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MfgCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "MfgCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MfgCategory_Site",
                table: "MfgCategory",
                column: "Site");

            // MfgProcess
            migrationBuilder.AddColumn<decimal>(
                name: "CostRate",
                table: "MfgProcess",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MfgProcess",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "MfgProcess",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StdTimeMin",
                table: "MfgProcess",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MfgProcess_Site",
                table: "MfgProcess",
                column: "Site");

            // MfgSubCategory
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MfgSubCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "MfgSubCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToleranceGrade",
                table: "MfgSubCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MfgSubCategory_Site",
                table: "MfgSubCategory",
                column: "Site");

            // MfgEquipment
            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "MfgEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "MfgEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "MfgEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Spec",
                table: "MfgEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MfgEquipment_Site",
                table: "MfgEquipment",
                column: "Site");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MfgCategory_Site",
                table: "MfgCategory");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MfgCategory");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "MfgCategory");

            migrationBuilder.DropIndex(
                name: "IX_MfgProcess_Site",
                table: "MfgProcess");

            migrationBuilder.DropColumn(
                name: "CostRate",
                table: "MfgProcess");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MfgProcess");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "MfgProcess");

            migrationBuilder.DropColumn(
                name: "StdTimeMin",
                table: "MfgProcess");

            migrationBuilder.DropIndex(
                name: "IX_MfgSubCategory_Site",
                table: "MfgSubCategory");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MfgSubCategory");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "MfgSubCategory");

            migrationBuilder.DropColumn(
                name: "ToleranceGrade",
                table: "MfgSubCategory");

            migrationBuilder.DropIndex(
                name: "IX_MfgEquipment_Site",
                table: "MfgEquipment");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "MfgEquipment");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "MfgEquipment");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "MfgEquipment");

            migrationBuilder.DropColumn(
                name: "Spec",
                table: "MfgEquipment");
        }
    }
}
