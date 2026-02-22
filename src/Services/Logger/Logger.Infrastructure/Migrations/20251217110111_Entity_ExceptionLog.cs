using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Entity_ExceptionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExceptionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    Tenants = table.Column<Guid[]>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'::uuid[]"),
                    IsGlobal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequestPath = table.Column<string>(type: "text", nullable: false),
                    HttpMethod = table.Column<string>(type: "text", nullable: false),
                    RequestJson = table.Column<string>(type: "text", nullable: true),
                    TraceIdentifier = table.Column<string>(type: "text", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionLogs_Tenants",
                table: "ExceptionLogs",
                column: "Tenants")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExceptionLogs");
        }
    }
}
