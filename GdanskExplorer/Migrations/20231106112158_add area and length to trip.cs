using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class addareaandlengthtotrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Area",
                table: "Trips",
                type: "real",
                nullable: false,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry");

            migrationBuilder.AddColumn<float>(
                name: "Length",
                table: "Trips",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<MultiPolygon>(
                name: "Polygon",
                table: "Trips",
                type: "geometry",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Length",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Polygon",
                table: "Trips");

            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Area",
                table: "Trips",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
