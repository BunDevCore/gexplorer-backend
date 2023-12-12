using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class addachievementtodbcontextproperly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementGets_Achievement_AchievementId",
                table: "AchievementGets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Achievement",
                table: "Achievement");

            migrationBuilder.RenameTable(
                name: "Achievement",
                newName: "Achievements");

            migrationBuilder.AddColumn<Geometry>(
                name: "Target",
                table: "Achievements",
                type: "geometry",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Achievements",
                table: "Achievements",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementGets_Achievements_AchievementId",
                table: "AchievementGets",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementGets_Achievements_AchievementId",
                table: "AchievementGets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Achievements",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "Achievements");

            migrationBuilder.RenameTable(
                name: "Achievements",
                newName: "Achievement");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Achievement",
                table: "Achievement",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementGets_Achievement_AchievementId",
                table: "AchievementGets",
                column: "AchievementId",
                principalTable: "Achievement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
