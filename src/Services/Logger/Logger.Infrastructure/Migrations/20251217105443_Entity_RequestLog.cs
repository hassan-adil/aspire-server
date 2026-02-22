using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Entity_RequestLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    Tenants = table.Column<Guid[]>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'::uuid[]"),
                    IsGlobal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequestPath = table.Column<string>(type: "text", nullable: false),
                    HttpMethod = table.Column<string>(type: "text", nullable: false),
                    RequestJson = table.Column<string>(type: "text", nullable: true),
                    ResponseJson = table.Column<string>(type: "text", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_Tenants",
                table: "RequestLogs",
                column: "Tenants")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestLogs");
        }
    }
}
