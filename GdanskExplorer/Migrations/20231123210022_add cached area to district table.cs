using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GdanskExplorer.Migrations
{
    /// <inheritdoc />
    public partial class addcachedareatodistricttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Districts");
            
            migrationBuilder.AddColumn<double>(
                name: "Area",
                table: "Districts",
                type: "double precision",
                nullable: false, defaultValue: 0.0);

            migrationBuilder.AddColumn<Polygon>(
                name: "Geometry",
                table: "Districts",
                type: "geometry",
                nullable: false, defaultValue: Polygon.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Geometry",
                table: "Districts");
            
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Districts");
            
            migrationBuilder.AddColumn<double>(
                name: "Area",
                table: "Districts",
                type: "double precision",
                nullable: false, defaultValue: 0.0);
        }
    }
}
