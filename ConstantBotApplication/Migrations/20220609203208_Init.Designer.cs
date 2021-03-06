// <auto-generated />
using ConstantBotApplication.Domain;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ConstantBotApplication.Migrations
{
    [DbContext(typeof(BotContext))]
    [Migration("20220609203208_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Fb:ValueGenerationStrategy", FbValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 31);

            modelBuilder.Entity("ConstantBotApplication.Domain.GuildSettings", b =>
                {
                    b.Property<decimal>("GuilId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("DECIMAL(18,2)")
                        .HasAnnotation("Fb:ValueGenerationStrategy", FbValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("MonitorChannelId")
                        .HasColumnType("DECIMAL(18,2)");

                    b.Property<bool>("MonitoringEnable")
                        .HasColumnType("BOOLEAN");

                    b.HasKey("GuilId");

                    b.ToTable("GuildSettings");
                });
#pragma warning restore 612, 618
        }
    }
}
