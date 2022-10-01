﻿// <auto-generated />
using ConstantBotApplication.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConstantBotApplication.Migrations
{
    [DbContext(typeof(BotContext))]
    partial class BotContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ConstantBotApplication.Domain.Guild", b =>
                {
                    b.Property<decimal>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("ChannelMonitoring")
                        .HasColumnType("boolean");

                    b.Property<decimal?>("MonitorChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("ReactionsMonitoring")
                        .HasColumnType("boolean");

                    b.Property<decimal?>("ReportChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("RolesMonitoring")
                        .HasColumnType("boolean");

                    b.Property<bool>("UserMonitoring")
                        .HasColumnType("boolean");

                    b.Property<bool>("VoiceMonitoring")
                        .HasColumnType("boolean");

                    b.Property<int?>("Volume")
                        .HasColumnType("integer");

                    b.HasKey("GuildId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("ConstantBotApplication.Domain.SocialAttachments", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Action")
                        .HasColumnType("integer");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("SocialAttachments");
                });

            modelBuilder.Entity("ConstantBotApplication.Domain.SocialCounter", b =>
                {
                    b.Property<decimal>("GiverId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("TakerId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Action")
                        .HasColumnType("integer");

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.HasKey("GiverId", "TakerId", "Action");

                    b.ToTable("SocialCounters");
                });
#pragma warning restore 612, 618
        }
    }
}
