using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstantBotApplication.Migrations
{
    public partial class AddedVolumeForPlayer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Volume",
                table: "GuildSettings",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Volume",
                table: "GuildSettings");
        }
    }
}
