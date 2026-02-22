using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Entity_DatabaseOperationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DatabaseOperationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    Tenants = table.Column<Guid[]>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'::uuid[]"),
                    IsGlobal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    RequestPath = table.Column<string>(type: "text", nullable: false),
                    HttpMethod = table.Column<string>(type: "text", nullable: false),
                    TraceIdentifier = table.Column<string>(type: "text", nullable: true),
                    OldSnapshot = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    NewSnapshot = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseOperationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseOperationLogs_Tenants",
                table: "DatabaseOperationLogs",
                column: "Tenants")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatabaseOperationLogs");
        }
    }
}
