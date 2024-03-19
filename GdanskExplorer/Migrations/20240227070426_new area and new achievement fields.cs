using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class newareaandnewachievementfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "NewArea",
                table: "Trips",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "AchievedOnTripId",
                table: "AchievementGets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AchievementGets_AchievedOnTripId",
                table: "AchievementGets",
                column: "AchievedOnTripId");

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementGets_Trips_AchievedOnTripId",
                table: "AchievementGets",
                column: "AchievedOnTripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementGets_Trips_AchievedOnTripId",
                table: "AchievementGets");

            migrationBuilder.DropIndex(
                name: "IX_AchievementGets_AchievedOnTripId",
                table: "AchievementGets");

            migrationBuilder.DropColumn(
                name: "NewArea",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "AchievedOnTripId",
                table: "AchievementGets");
        }
    }
}
