using GdanskExplorer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace GdanskExplorer;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class DistrictController : ControllerBase
{
    private GExplorerContext _db;

    public DistrictController(GExplorerContext db)
    {
        _db = db;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportDistricts()
    {
        var bodyString = await new StreamReader(HttpContext.Request.BodyReader.AsStream()).ReadToEndAsync();
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
        }
        catch (JsonReaderException)
        {
            return BadRequest("bad GeoJSON body!");
        }

        return Ok();
    }
}