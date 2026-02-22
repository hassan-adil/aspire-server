using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Runtime_Field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RuntimeMs",
                table: "RequestLogs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RuntimeMs",
                table: "ExceptionLogs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RuntimeMs",
                table: "DatabaseOperationLogs",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RuntimeMs",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "RuntimeMs",
                table: "ExceptionLogs");

            migrationBuilder.DropColumn(
                name: "RuntimeMs",
                table: "DatabaseOperationLogs");
        }
    }
}
