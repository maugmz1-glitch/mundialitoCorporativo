using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config) => _config = config;

    /// <summary>
    /// Login básico: valida usuario/contraseña desde configuración y devuelve un JWT.
    /// Configuración: Auth:Username, Auth:Password, Auth:SecretKey (para firmar el JWT).
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var username = _config["Auth:Username"] ?? "admin";
        var password = _config["Auth:Password"] ?? "Mundialito2024!";
        var secretKey = _config["Auth:SecretKey"] ?? "MundialitoCorporativo-SecretKey-Minimo32Caracteres!";

        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password) ||
            request.Username != username || request.Password != password)
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.Name, request.Username), new Claim(JwtRegisteredClaimNames.Sub, request.Username) };
        var token = new JwtSecurityToken(
            issuer: "MundialitoCorporativo",
            audience: "MundialitoCorporativo",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse(tokenString, request.Username));
    }

    /// <summary>
    /// Devuelve el usuario actual si el JWT es válido (cabecera Authorization: Bearer &lt;token&gt;).
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeResponse), 200)]
    [ProducesResponseType(401)]
    public IActionResult Me()
    {
        var name = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null;
        if (string.IsNullOrEmpty(name)) return Unauthorized();
        return Ok(new MeResponse(name));
    }
}

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string UserName);
public record MeResponse(string UserName);
