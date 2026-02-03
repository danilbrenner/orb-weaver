using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbWeaver.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlertStateAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    alert_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    filter = table.Column<string>(type: "jsonpath", nullable: false),
                    expression = table.Column<string>(type: "jsonpath", nullable: false),
                    last_handled_ts = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alerts", x => x.alert_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");
        }
    }
}
