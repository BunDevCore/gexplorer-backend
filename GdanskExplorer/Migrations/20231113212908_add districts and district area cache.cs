using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class adddistrictsanddistrictareacache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementGet_Achievement_AchievementId",
                table: "AchievementGet");

            migrationBuilder.DropForeignKey(
                name: "FK_AchievementGet_AspNetUsers_UserId",
                table: "AchievementGet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AchievementGet",
                table: "AchievementGet");

            migrationBuilder.RenameTable(
                name: "AchievementGet",
                newName: "AchievementGets");

            migrationBuilder.RenameIndex(
                name: "IX_AchievementGet_AchievementId",
                table: "AchievementGets",
                newName: "IX_AchievementGets_AchievementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AchievementGets",
                table: "AchievementGets",
                columns: new[] { "UserId", "AchievementId" });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Area = table.Column<Polygon>(type: "geometry", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistrictAreaCacheEntries",
                columns: table => new
                {
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Area = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistrictAreaCacheEntries", x => new { x.DistrictId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DistrictAreaCacheEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistrictAreaCacheEntries_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistrictAreaCacheEntries_UserId",
                table: "DistrictAreaCacheEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementGets_Achievement_AchievementId",
                table: "AchievementGets",
                column: "AchievementId",
                principalTable: "Achievement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementGets_AspNetUsers_UserId",
                table: "AchievementGets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementGets_Achievement_AchievementId",
                table: "AchievementGets");

            migrationBuilder.DropForeignKey(
                name: "FK_AchievementGets_AspNetUsers_UserId",
                table: "AchievementGets");

            migrationBuilder.DropTable(
                name: "DistrictAreaCacheEntries");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AchievementGets",
                table: "AchievementGets");

            migrationBuilder.RenameTable(
                name: "AchievementGets",
                newName: "AchievementGet");

            migrationBuilder.RenameIndex(
                name: "IX_AchievementGets_AchievementId",
                table: "AchievementGet",
                newName: "IX_AchievementGet_AchievementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AchievementGet",
                table: "AchievementGet",
                columns: new[] { "UserId", "AchievementId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementGet_Achievement_AchievementId",
                table: "AchievementGet",
                column: "AchievementId",
                principalTable: "Achievement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementGet_AspNetUsers_UserId",
                table: "AchievementGet",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
