using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbWeaver.Data.Migrations
{
    /// <inheritdoc />
    public partial class MessageLoggingEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp",
                table: "messages_log",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "alerts",
                type: "xid",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "ix_messages_log_payload",
                table: "messages_log",
                column: "payload")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_messages_log_timestamp",
                table: "messages_log",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_messages_log_payload",
                table: "messages_log");

            migrationBuilder.DropIndex(
                name: "ix_messages_log_timestamp",
                table: "messages_log");

            migrationBuilder.DropColumn(
                name: "timestamp",
                table: "messages_log");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "alerts");
        }
    }
}
