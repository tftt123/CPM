using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpmServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration - all tables already exist in database.
            // Created manually to align EF model snapshot with existing schema.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op for baseline
        }
    }
}
