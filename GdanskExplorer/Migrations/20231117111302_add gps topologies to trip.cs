using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class addgpstopologiestotrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadDate",
                table: "Trips",
                newName: "UploadTime");

            migrationBuilder.AddColumn<LineString>(
                name: "GpsLineString",
                table: "Trips",
                type: "geometry",
                nullable: false);

            migrationBuilder.AddColumn<MultiPolygon>(
                name: "GpsPolygon",
                table: "Trips",
                type: "geometry",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GpsLineString",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "GpsPolygon",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "UploadTime",
                table: "Trips",
                newName: "UploadDate");
        }
    }
}
