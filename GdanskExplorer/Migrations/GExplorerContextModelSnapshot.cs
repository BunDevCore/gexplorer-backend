﻿// <auto-generated />
using System;
using GdanskExplorer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GdanskExplorer.Migrations
{
    [DbContext(typeof(GExplorerContext))]
    partial class GExplorerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "postgis");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GdanskExplorer.Data.Achievement", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Achievement");
                });

            modelBuilder.Entity("GdanskExplorer.Data.AchievementGet", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("AchievementId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("TimeAchieved")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("UserId", "AchievementId");

                    b.HasIndex("AchievementId");

                    b.ToTable("AchievementGet");
                });

            modelBuilder.Entity("GdanskExplorer.Data.Trip", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<MultiPolygon>("Area")
                        .IsRequired()
                        .HasColumnType("geometry");

                    b.Property<DateTime>("UploadDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Trips");
                });

            modelBuilder.Entity("GdanskExplorer.Data.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("DateJoined")
                        .HasColumnType("timestamp with time zone");

                    b.Property<MultiPolygon>("OverallArea")
                        .IsRequired()
                        .HasColumnType("geometry");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("GdanskExplorer.Data.AchievementGet", b =>
                {
                    b.HasOne("GdanskExplorer.Data.Achievement", null)
                        .WithMany()
                        .HasForeignKey("AchievementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GdanskExplorer.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("GdanskExplorer.Data.Trip", b =>
                {
                    b.HasOne("GdanskExplorer.Data.User", "User")
                        .WithMany("Trips")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("GdanskExplorer.Data.User", b =>
                {
                    b.Navigation("Trips");
                });
#pragma warning restore 612, 618
        }
    }
}
