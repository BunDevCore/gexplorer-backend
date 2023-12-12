using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class changeachievementpktypetostring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_AchievementGets_Achievement_AchievementId", "AchievementGets");
            migrationBuilder.AlterColumn<string>(
                name: "AchievementId",
                table: "AchievementGets",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Achievement",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
            migrationBuilder.AddForeignKey("FK_AchievementGets_Achievement_AchievementId", "AchievementGets",
                principalTable: "Achievement", principalColumn: "Id", column: "AchievementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AchievementId",
                table: "AchievementGets",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Achievement",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
