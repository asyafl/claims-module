using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    private static readonly Dictionary<string, (string Password, string Role, Guid UserId)> Users = new()
    {
        ["handler@claims.io"] = ("Handler123!", "handler", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001")),
        ["supervisor@claims.io"] = ("Supervisor123!", "supervisor", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002")),
        ["manager@claims.io"] = ("Manager123!", "manager", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003")),
    };

    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult GetToken([FromBody] LoginRequest request)
    {
        if (!Users.TryGetValue(request.Email, out var user) || user.Password != request.Password)
            return Unauthorized(new { message = "Invalid credentials" });

        var secret = configuration["Jwt:Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("name", request.Email),
            new Claim("role", user.Role),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            email = request.Email,
            role = user.Role,
            userId = user.UserId,
            expiresAt = DateTime.UtcNow.AddHours(8)
        });
    }
}

public record LoginRequest(string Email, string Password);
