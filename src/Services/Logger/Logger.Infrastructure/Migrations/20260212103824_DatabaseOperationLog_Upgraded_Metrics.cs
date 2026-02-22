using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseOperationLog_Upgraded_Metrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Action",
                table: "DatabaseOperationLogs",
                newName: "Operation");

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "DatabaseOperationLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "DatabaseOperationLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsError",
                table: "DatabaseOperationLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RowsAffected",
                table: "DatabaseOperationLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SqlQuery",
                table: "DatabaseOperationLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "DatabaseOperationLogs");

            migrationBuilder.DropColumn(
                name: "IsError",
                table: "DatabaseOperationLogs");

            migrationBuilder.DropColumn(
                name: "RowsAffected",
                table: "DatabaseOperationLogs");

            migrationBuilder.DropColumn(
                name: "SqlQuery",
                table: "DatabaseOperationLogs");

            migrationBuilder.RenameColumn(
                name: "Operation",
                table: "DatabaseOperationLogs",
                newName: "Action");

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "DatabaseOperationLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
