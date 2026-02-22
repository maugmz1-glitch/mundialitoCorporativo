using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MundialitoCorporativo.Api;
using MundialitoCorporativo.Application.Auth;
using MundialitoCorporativo.Application.Auth.Commands;
using MundialitoCorporativo.Application.Auth.Queries;

namespace MundialitoCorporativo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crear cuenta: registra un nuevo usuario (nombre de usuario, email opcional, contraseña).
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterCommand(request.UserName, request.Email, request.Password), cancellationToken);
        if (result.IsSuccess)
            return StatusCode(201, result.Data);
        return result.ToActionResult();
    }

    /// <summary>
    /// Iniciar sesión: valida usuario y contraseña contra la base de datos y devuelve un JWT.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(request.Username, request.Password), cancellationToken);
        if (result.IsSuccess)
            return Ok(result.Data);
        return result.ToActionResult();
    }

    /// <summary>
    /// Devuelve el usuario actual si el JWT es válido (cabecera Authorization: Bearer &lt;token&gt;).
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var name = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null;
        var result = await _mediator.Send(new MeQuery(name), cancellationToken);
        if (result.IsSuccess)
            return Ok(result.Data);
        return result.ToActionResult();
    }
}

public record RegisterRequest(string UserName, string? Email, string Password);
public record LoginRequest(string Username, string Password);
