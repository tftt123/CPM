using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SysApprovalCondition",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepId = table.Column<long>(type: "bigint", nullable: false),
                    ConditionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetStepId = table.Column<long>(type: "bigint", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysApprovalCondition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysApprovalCondition_SysApprovalStep_StepId",
                        column: x => x.StepId,
                        principalTable: "SysApprovalStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysApprovalCondition_StepId",
                table: "SysApprovalCondition",
                column: "StepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SysApprovalCondition");
        }
    }
}
