using System.Text.Json;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DotSpatial.Projections;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using GdanskExplorer.Topology;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    private readonly GExplorerContext _db;
    private readonly IMapper _mapper;
    private readonly IOptions<AreaCalculationOptions> _options;
    private readonly DotSpatialReprojector _reproject;


    public DistrictController(GExplorerContext db, IMapper mapper, IOptions<AreaCalculationOptions> options)
    {
        _db = db;
        _mapper = mapper;
        _options = options;
        _reproject = new DotSpatialReprojector(ProjectionInfo.FromEpsgCode(4326),
            ProjectionInfo.FromEpsgCode(_options.Value.CommonAreaSrid));
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
                {
                    var gpsGeometry = (Polygon)f.Geometry.Copy();
                    gpsGeometry.Apply(_reproject.Reversed());
                    
                    return new District
                    {
                        Geometry = (Polygon)f.Geometry,
                        Area = f.Geometry.Area,
                        GpsGeometry = gpsGeometry,
                        Id = new Guid(),
                        Name = (string)f.Attributes["DZIELNICY"]
                    };
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