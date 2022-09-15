using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstantBotApplication.Migrations
{
    public partial class FixedPrimaryKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SocialCounters",
                table: "SocialCounters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SocialCounters",
                table: "SocialCounters",
                columns: new[] { "GiverId", "TakerId", "Action" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SocialCounters",
                table: "SocialCounters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SocialCounters",
                table: "SocialCounters",
                columns: new[] { "GiverId", "TakerId" });
        }
    }
}
