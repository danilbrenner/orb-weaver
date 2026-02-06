using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbWeaver.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitWithUpdateLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "messages_log",
                columns: table => new
                {
                    hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payload = table.Column<JsonDocument>(type: "jsonb", maxLength: 10000, nullable: false),
                    logged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages_log", x => x.hash);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "messages_log");
        }
    }
}
