using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Indices_DatabaseOperationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DatabaseOperationLogs_NewSnapshot",
                table: "DatabaseOperationLogs",
                column: "NewSnapshot")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseOperationLogs_OldSnapshot",
                table: "DatabaseOperationLogs",
                column: "OldSnapshot")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DatabaseOperationLogs_NewSnapshot",
                table: "DatabaseOperationLogs");

            migrationBuilder.DropIndex(
                name: "IX_DatabaseOperationLogs_OldSnapshot",
                table: "DatabaseOperationLogs");
        }
    }
}
