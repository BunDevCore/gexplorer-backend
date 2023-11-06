using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using GdanskExplorer.Data;
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
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(IConfiguration config, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            return BadRequest();
        }

        if (!await _userManager.CheckPasswordAsync(user, dto.Password)) return BadRequest();
        
        var token = NewJwt(user);
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
            return BadRequest("Usernames can only consist of letters, numbers and .-_ and be between 4 and 30 characters");
        }

        var newUser = new User
        {
            // Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
        };

        
        var result = await _userManager.CreateAsync(newUser, dto.Password);
        
        var roleAddResult = await _userManager.AddToRoleAsync(newUser, "User");
        
        if (result.Succeeded && roleAddResult.Succeeded)
        {
            return new OkObjectResult(NewJwt(newUser));
        }

        return UnprocessableEntity(result.Errors.Select(e => e.Code)
            .Concat(roleAddResult.Errors.Select(e => e.Code)));
    }

    private string NewJwt(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTKey"]));
        var token = new JwtSecurityToken(issuer: "geledit-auth",
            audience: "geledit-api",
            expires: DateTime.UtcNow.AddHours(3),
            claims: claims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return serialized;
    }
}