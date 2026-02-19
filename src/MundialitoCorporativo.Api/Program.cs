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

// Primera ejecución: aplica migraciones EF y carga seed (4 equipos, 5 jugadores/equipo, 6 partidos).
await app.EnsureSeedAsync();

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
