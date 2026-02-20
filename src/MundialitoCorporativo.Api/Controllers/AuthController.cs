using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MundialitoCorporativo.Api.Services;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IAppDbContext _db;

    public AuthController(IConfiguration config, IAppDbContext db)
    {
        _config = config;
        _db = db;
    }

    /// <summary>
    /// Crear cuenta: registra un nuevo usuario (nombre de usuario, email opcional, contraseña).
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var userName = request.UserName?.Trim();
        var email = request.Email?.Trim();
        var password = request.Password;

        if (string.IsNullOrEmpty(userName) || userName.Length < 3)
            return BadRequest(new { message = "El nombre de usuario es obligatorio y debe tener al menos 3 caracteres." });
        if (string.IsNullOrEmpty(password) || password.Length < 6)
            return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres." });

        var exists = await _db.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
        if (exists)
            return BadRequest(new { message = "Ya existe un usuario con ese nombre. Elige otro." });

        if (!string.IsNullOrEmpty(email))
        {
            var emailExists = await _db.Users.AnyAsync(u => u.Email == email, cancellationToken);
            if (emailExists)
                return BadRequest(new { message = "Ya existe una cuenta con ese correo." });
        }

        var (hash, salt) = PasswordHasher.HashPassword(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = string.IsNullOrEmpty(email) ? null : email,
            PasswordHash = hash,
            Salt = salt,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("truncat", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Error al guardar la cuenta. Actualice la base de datos con las migraciones más recientes (ejecute la aplicación o aplique las migraciones)." });
            return BadRequest(new { message = "No se pudo crear la cuenta. Intente de nuevo." });
        }

        return StatusCode(201, new RegisterResponse(user.UserName, "Cuenta creada. Ya puedes iniciar sesión."));
    }

    /// <summary>
    /// Iniciar sesión: valida usuario y contraseña contra la base de datos y devuelve un JWT.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return Unauthorized(new { message = "Usuario y contraseña son obligatorios." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == request.Username.Trim(), cancellationToken);
        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

        var secretKey = _config["Auth:SecretKey"] ?? "MundialitoCorporativo-SecretKey-Minimo32Caracteres!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.Name, user.UserName), new Claim(JwtRegisteredClaimNames.Sub, user.UserName) };
        var token = new JwtSecurityToken(
            issuer: "MundialitoCorporativo",
            audience: "MundialitoCorporativo",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse(tokenString, user.UserName));
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

public record RegisterRequest(string UserName, string? Email, string Password);
public record RegisterResponse(string UserName, string Message);
public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string UserName);
public record MeResponse(string UserName);
