using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using DotSpatial.Projections;
using GdanskExplorer;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using GdanskExplorer.Topology;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new GeometryJsonConverter());
            options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        }
);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "gexplorer-auth", Version = "v1" });
    c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter into field the word 'Bearer' followed by space and JWT",
            Name = "Authorization", Type = SecuritySchemeType.ApiKey
        });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "jwt",
                Name = "Bearer",
                In = ParameterLocation.Header,

            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<GExplorerContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("prodDb"),
        x => x.UseNetTopologySuite()));

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins(builder.Configuration.GetValue<string[]>("CorsOrigins") ??
                 new []{"http://localhost:3000", "https://localhost:3000"})));

builder.Services.AddAuthorization();
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<GExplorerContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer("Bearer", options =>
{
    options.SaveToken = true;
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = false,
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidIssuer = "gexplorer-auth",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTKey"] ??
                                                                           throw new InvalidDataException("jwt key not configured")))
    };
});

// area calc configuration related stuff
builder.Services.Configure<AreaCalculationOptions>(
    builder.Configuration.GetSection(AreaCalculationOptions.SectionName));

builder.Services.AddSingleton<GpxAreaExtractor>(isp => new GpxAreaExtractor(
    isp.GetRequiredService<IOptions<AreaCalculationOptions>>().Value, isp.GetRequiredService<ILogger<GpxAreaExtractor>>()));

builder.Services.AddAutoMapper(typeof(GExplorerAutoMapperProfile));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
    var roles = new[] { "Admin", "User" };
 
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }
}

app.Run();