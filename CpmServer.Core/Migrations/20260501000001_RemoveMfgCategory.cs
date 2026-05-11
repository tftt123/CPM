using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMfgCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_MfgProcess_Category",
                table: "MfgProcess");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_MfgProcess_CategoryId",
                table: "MfgProcess");

            // Add Category column
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MfgProcess",
                type: "nvarchar(max)",
                nullable: true);

            // Migrate data: map CategoryId to Category name
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.Category = c.CategoryName
                FROM MfgProcess p
                INNER JOIN MfgCategory c ON p.CategoryId = c.Id
            ");

            // Drop CategoryId column
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "MfgProcess");

            // Drop MfgCategory table
            migrationBuilder.DropTable(
                name: "MfgCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration is not reversible in a meaningful way
            throw new NotSupportedException("This migration cannot be reverted.");
        }
    }
}
