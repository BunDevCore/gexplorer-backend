using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class addgpsareatodistrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Polygon>(
                name: "GpsGeometry",
                table: "Districts",
                type: "geometry",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GpsGeometry",
                table: "Districts");
        }
    }
}
