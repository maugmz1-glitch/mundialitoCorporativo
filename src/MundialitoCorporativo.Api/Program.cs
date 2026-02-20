// ========== CONFIGURACIÓN PRINCIPAL DE LA API ==========
// Clean Architecture: Api solo orquesta; la lógica está en Application e Infrastructure.

using MundialitoCorporativo.Application;
using MundialitoCorporativo.Infrastructure;
using MundialitoCorporativo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Application: MediatR y handlers (CQRS). Infrastructure: DbContext, Dapper repos, IdempotencyStore.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// Respuestas JSON en camelCase para el frontend.
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:Origins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "http://localhost:3000" })
            .AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Espera inicial: solo en Docker (SQL tarda en aceptar conexiones desde otros contenedores). En local (localhost) 5s basta.
var connStr = builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "";
var isDocker = connStr.Contains("Server=db;", StringComparison.OrdinalIgnoreCase);
await Task.Delay(TimeSpan.FromSeconds(isDocker ? 35 : 5));
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
        var delay = TimeSpan.FromSeconds(10);
        app.Logger.LogWarning(ex, "Seed/DB no disponible, reintento en {Delay}s ({Attempt}/{Max})", delay.TotalSeconds, i + 1, maxRetries);
        await Task.Delay(delay);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Idempotencia: POST con cabecera Idempotency-Key devuelve la misma respuesta en reintentos sin duplicar.
app.UseMiddleware<MundialitoCorporativo.Api.Middleware.IdempotencyMiddleware>();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
