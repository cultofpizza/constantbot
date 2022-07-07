﻿// <auto-generated />
using ConstantBotApplication.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConstantBotApplication.Migrations
{
    [DbContext(typeof(BotContext))]
    [Migration("20220706185037_AddedVolumeForPlayer")]
    partial class AddedVolumeForPlayer
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ConstantBotApplication.Domain.GuildSettings", b =>
                {
                    b.Property<decimal>("GuilId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("MonitorChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("MonitoringEnable")
                        .HasColumnType("boolean");

                    b.Property<decimal?>("ReportChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("Volume")
                        .HasColumnType("integer");

                    b.HasKey("GuilId");

                    b.ToTable("GuildSettings");
                });
#pragma warning restore 612, 618
        }
    }
}
