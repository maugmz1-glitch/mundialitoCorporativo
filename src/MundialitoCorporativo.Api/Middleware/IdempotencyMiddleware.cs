using MundialitoCorporativo.Application.Interfaces;

namespace MundialitoCorporativo.Api.Middleware;

/// <summary>
/// Middleware de idempotencia para POST: si el cliente reenvía la misma petición con el mismo
/// Idempotency-Key, devolvemos la respuesta guardada sin ejecutar de nuevo el handler (evita duplicados).
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    public const string IdempotencyKeyHeader = "Idempotency-Key";

    public IdempotencyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IIdempotencyStore store)
    {
        var key = context.Request.Headers[IdempotencyKeyHeader].FirstOrDefault();
        // Solo aplicamos idempotencia a POST con cabecera presente.
        if (string.IsNullOrWhiteSpace(key) || context.Request.Method != "POST")
        {
            await _next(context);
            return;
        }
        key = key.Trim();
        var path = (context.Request.Path.Value ?? "/") + (context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty);
        var method = context.Request.Method;

        // ¿Ya tenemos una respuesta guardada para esta clave + método + ruta?
        var existing = await store.GetAsync(key, method, path, context.RequestAborted);
        if (existing != null)
        {
            // Replay: devolvemos la misma respuesta (mismo status, body y Content-Type si está disponible).
            context.Response.StatusCode = existing.StatusCode;
            context.Response.ContentType = existing.ContentType ?? "application/json";
            if (!string.IsNullOrEmpty(existing.Body))
                await context.Response.WriteAsync(existing.Body);
            return;
        }

        // Primera vez: capturamos la respuesta en un MemoryStream para guardarla después.
        var originalBodyStream = context.Response.Body;
        await using var newBodyStream = new MemoryStream();
        context.Response.Body = newBodyStream;

        await _next(context);

        // Copiamos la respuesta al cliente y la guardamos en el store para futuros reintentos.
        newBodyStream.Position = 0;
        var responseBody = await new StreamReader(newBodyStream).ReadToEndAsync();
        newBodyStream.Position = 0;
        await newBodyStream.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;

        // Solo guardamos respuestas exitosas (2xx) para evitar cachear errores transitorios.
        var status = context.Response.StatusCode;
        if (status >= 200 && status < 300)
        {
            await store.StoreAsync(key, method, path, status, responseBody, context.Response.ContentType, context.RequestAborted);
        }
    }
}
