using System.Net;
using System.Text.Json;

namespace MundialitoCorporativo.Api.Middleware;

/// <summary>
/// Captura excepciones no controladas y devuelve 500 con mensaje en JSON para que el frontend pueda mostrarlo.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var message = _env.IsDevelopment() ? ex.Message : "Error interno del servidor. Revisa los logs.";
            var body = JsonSerializer.Serialize(new { message }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(body);
        }
    }
}
