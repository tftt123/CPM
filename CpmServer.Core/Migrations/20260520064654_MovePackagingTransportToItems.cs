using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class MovePackagingTransportToItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackagingCost",
                table: "QuoQuotation");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "QuoQuotation");

            migrationBuilder.AddColumn<decimal>(
                name: "PackagingCost",
                table: "QuoQuotationItem",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "QuoQuotationItem",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackagingCost",
                table: "QuoQuotationItem");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "QuoQuotationItem");

            migrationBuilder.AddColumn<decimal>(
                name: "PackagingCost",
                table: "QuoQuotation",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "QuoQuotation",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
