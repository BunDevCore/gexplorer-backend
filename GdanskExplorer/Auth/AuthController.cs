using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using GdanskExplorer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GdanskExplorer.Auth;

[ApiController]
[Route("[controller]")]
public partial class AuthController : ControllerBase
{
    [GeneratedRegex("^[a-zA-Z0-9.-_]{4,30}$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex UsernameRegex();

    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly GExplorerContext _db;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<AuthController> _log;

    public AuthController(IConfiguration config, UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, ILogger<AuthController> log, GExplorerContext db)
    {
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
        _log = log;
        _db = db;
    }

    [HttpGet("check")]
    [Authorize]
    public string AuthCheck()
    {
        return ":)";
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _log.LogInformation("new login attempt for {User}", dto.UserName);
        if (!ModelState.IsValid)
        {
            _log.LogDebug("bad model state");
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            _log.LogDebug("user is nonexistent");
            return BadRequest();
        }

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _log.LogInformation("failed login attempt for {User}", user.UserName);
            user.AccessFailedCount += 1;
            return BadRequest();
        }

        var token = await NewJwt(user);
        return Ok(token);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!UsernameRegex().IsMatch(dto.UserName))
        {
            return BadRequest(
                "Usernames can only consist of letters, numbers and .-_ and be between 4 and 30 characters");
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            SecurityStamp = new Guid().ToString()
        };


        var result = await _userManager.CreateAsync(newUser, dto.Password);
        
        if (!result.Succeeded)
        {
            return UnprocessableEntity(result.Errors.Select(e => e.Code));
        }

        var roleAddResult = await _userManager.AddToRoleAsync(newUser, "User");

        if (!roleAddResult.Succeeded)
        {
            return UnprocessableEntity(roleAddResult.Errors.Select(e => e.Code));
        }
        return Ok(await NewJwt(newUser));

        
    }

    private async Task<string> NewJwt(User user)
    {
        var claims = await GetClaims(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTKey"]));
        var token = new JwtSecurityToken(issuer: "geledit-auth",
            audience: "geledit-api",
            expires: DateTime.UtcNow.AddHours(3),
            claims: claims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return serialized;
    }

    private async Task<IEnumerable<Claim>> GetClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);

        claims.AddRange(userClaims);
        
        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
            var role = await _roleManager.FindByNameAsync(userRole);
            if (role == null) continue;
            
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            claims.AddRange(roleClaims);
        }


        return claims;
    }
}