using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class changegpstopologiestousegeography : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "GpsPolygon",
                table: "Trips",
                type: "geography",
                nullable: false,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry");

            migrationBuilder.AlterColumn<LineString>(
                name: "GpsLineString",
                table: "Trips",
                type: "geography",
                nullable: false,
                oldClrType: typeof(LineString),
                oldType: "geometry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "GpsPolygon",
                table: "Trips",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(MultiPolygon),
                oldType: "geography");

            migrationBuilder.AlterColumn<LineString>(
                name: "GpsLineString",
                table: "Trips",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(LineString),
                oldType: "geography");
        }
    }
}
