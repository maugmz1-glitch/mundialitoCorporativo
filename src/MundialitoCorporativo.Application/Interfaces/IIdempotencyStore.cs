namespace MundialitoCorporativo.Application.Interfaces;

public record IdempotencyResponse(int StatusCode, string? Body, string? ContentType);

public interface IIdempotencyStore
{
    Task<IdempotencyResponse?> GetAsync(string idempotencyKey, string method, string path, CancellationToken cancellationToken = default);
    Task StoreAsync(string idempotencyKey, string method, string path, int statusCode, string? body, string? contentType, CancellationToken cancellationToken = default);
}
