using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    public partial class AddProcessFieldsToQuotationItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessType",
                table: "QuoQuotationItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentType",
                table: "QuoQuotationItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Equipment",
                table: "QuoQuotationItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CycleTime",
                table: "QuoQuotationItem",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "QuoQuotationItem",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "QuoQuotationItem",
                type: "decimal(18,4)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessType",
                table: "QuoQuotationItem");

            migrationBuilder.DropColumn(
                name: "EquipmentType",
                table: "QuoQuotationItem");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "QuoQuotationItem");

            migrationBuilder.DropColumn(
                name: "CycleTime",
                table: "QuoQuotationItem");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "QuoQuotationItem");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "QuoQuotationItem");
        }
    }
}
