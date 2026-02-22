using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MundialitoCorporativo.Application.Interfaces;

namespace MundialitoCorporativo.Api.Services;

/// <summary>
/// Genera JWT para autenticación. Implementación de IJwtTokenGenerator (configuración en Api).
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string userName)
    {
        var secretKey = _config["Auth:SecretKey"] ?? "MundialitoCorporativo-SecretKey-Minimo32Caracteres!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(JwtRegisteredClaimNames.Sub, userName)
        };
        var token = new JwtSecurityToken(
            issuer: "MundialitoCorporativo",
            audience: "MundialitoCorporativo",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
