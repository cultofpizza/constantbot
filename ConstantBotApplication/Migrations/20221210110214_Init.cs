using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConstantBotApplication.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MonitorChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    ReportChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Volume = table.Column<int>(type: "integer", nullable: true),
                    UserMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    VoiceMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    ReactionsMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    ChannelMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    RolesMonitoring = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "SocialAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialCounters",
                columns: table => new
                {
                    GiverId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TakerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialCounters", x => new { x.GiverId, x.TakerId, x.Action });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "SocialAttachments");

            migrationBuilder.DropTable(
                name: "SocialCounters");
        }
    }
}
