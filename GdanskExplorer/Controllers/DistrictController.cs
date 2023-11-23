using System.Text.Json;
using GdanskExplorer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace GdanskExplorer.Controllers;

[Route("[controller]")]
public class DistrictController : ControllerBase
{
    private GExplorerContext _db;

    public DistrictController(GExplorerContext db)
    {
        _db = db;
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    // [SwaggerSchema("Accepts a GeoJSON feature collection as the body. All geometries must be polygonal and have a DZIELNICY attribute.")]
    public async Task<IActionResult> ImportDistricts([FromBody] JsonElement data)
    {
        var bodyString = data.GetRawText();
        // var bodyString = await new StreamReader(HttpContext.Request.BodyReader.AsStream()).ReadToEndAsync();
        var reader = new GeoJsonReader();
        try
        {
            var fc = reader.Read<FeatureCollection>(bodyString);
            var dbDistricts = fc.Select(f =>
                new District
                {
                    Area = (Polygon)f.Geometry,
                    Id = new Guid(),
                    Name = (string)f.Attributes["DZIELNICY"]
                }
            );
            await _db.Districts.ExecuteDeleteAsync();
            await _db.Districts.AddRangeAsync(dbDistricts);
            await _db.SaveChangesAsync();
        }
        catch (JsonReaderException)
        {
            return BadRequest("bad GeoJSON body!");
        }

        return Ok();
    }
}