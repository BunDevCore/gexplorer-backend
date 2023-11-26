using System.Text.Json;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace GdanskExplorer.Controllers;

[Route("[controller]")]
public class DistrictController : ControllerBase
{
    private GExplorerContext _db;
    private IMapper _mapper;

    public DistrictController(GExplorerContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
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
                    Geometry = (Polygon)f.Geometry,
                    Area = f.Geometry.Area,
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

    [HttpGet("")]
    public IEnumerable<DistrictDto> GetAll()
    {
        return _mapper.ProjectTo<DistrictDto>(_db.Districts);
    }
}