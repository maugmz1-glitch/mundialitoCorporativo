// ========== CONFIGURACIÓN PRINCIPAL DE LA API ==========
// Clean Architecture: Api solo orquesta; la lógica está en Application e Infrastructure.

using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using MundialitoCorporativo.Api;
using MundialitoCorporativo.Api.Services;
using MundialitoCorporativo.Application;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Infrastructure;
using MundialitoCorporativo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Application: MediatR y handlers (CQRS). Infrastructure: DbContext, Dapper repos, IdempotencyStore.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// Respuestas JSON en camelCase para el frontend.
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
// Versionado de API: rutas api/v1/... (permite evolucionar sin romper clientes).
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mundialito Corporativo API",
        Version = "v1",
        Description = "API para gestión de torneos: equipos, jugadores, partidos, árbitros y posiciones. Autenticación JWT (Auth/Login)."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token JWT obtenido de POST /api/auth/login. Ejemplo: Bearer <token>"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:Origins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "http://localhost:3000" })
            .AllowAnyMethod().AllowAnyHeader();
    });
});

// Login básico: JWT. Configuración: Auth:Username, Auth:Password, Auth:SecretKey (mín. 32 caracteres).
var secretKey = builder.Configuration["Auth:SecretKey"] ?? "MundialitoCorporativo-SecretKey-Minimo32Caracteres!";
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
            ValidIssuer = "MundialitoCorporativo",
            ValidAudience = "MundialitoCorporativo",
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Compresión de respuestas (gzip/brotli) para reducir tiempo de transferencia.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// Espera inicial reducida: local 1s, Docker 15s (SQL tarda en aceptar conexiones). Reintentos cubren si la DB no está lista.
var connStr = builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "";
var isDocker = connStr.Contains("Server=db;", StringComparison.OrdinalIgnoreCase);
await Task.Delay(TimeSpan.FromSeconds(isDocker ? 15 : 1));
const int maxRetries = 12;
for (var i = 0; i < maxRetries; i++)
{
    try
    {
        await app.EnsureSeedAsync();
        break;
    }
    catch (Exception ex) when (i < maxRetries - 1)
    {
        var delay = TimeSpan.FromSeconds(isDocker ? 10 : 3);
        app.Logger.LogWarning(ex, "Seed/DB no disponible, reintento en {Delay}s ({Attempt}/{Max})", delay.TotalSeconds, i + 1, maxRetries);
        await Task.Delay(delay);
    }
}

try
{
    await app.EnsureDemoUserAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "No se pudo crear el usuario demo; la API seguirá en ejecución.");
}

// Compresión antes de otros middlewares para que las respuestas salgan comprimidas.
app.UseResponseCompression();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mundialito Corporativo API v1");
    c.RoutePrefix = "swagger";
});

// Errores no controlados: devolver 500 en JSON para que el frontend muestre el mensaje.
app.UseMiddleware<MundialitoCorporativo.Api.Middleware.ExceptionHandlingMiddleware>();
// Idempotencia: POST con cabecera Idempotency-Key devuelve la misma respuesta en reintentos sin duplicar.
app.UseMiddleware<MundialitoCorporativo.Api.Middleware.IdempotencyMiddleware>();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
