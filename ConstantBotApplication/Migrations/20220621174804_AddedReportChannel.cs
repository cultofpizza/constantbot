using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstantBotApplication.Migrations
{
    public partial class AddedReportChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "MonitoringEnable",
                table: "GuildSettings",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonitorChannelId",
                table: "GuildSettings",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuilId",
                table: "GuildSettings",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "ReportChannelId",
                table: "GuildSettings",
                type: "numeric(20,0)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportChannelId",
                table: "GuildSettings");

            migrationBuilder.AlterColumn<bool>(
                name: "MonitoringEnable",
                table: "GuildSettings",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonitorChannelId",
                table: "GuildSettings",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuilId",
                table: "GuildSettings",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }
    }
}
