using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Infrastructure.Persistence;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly IAppDbContext _db;

    public IdempotencyStore(IAppDbContext db) => _db = db;

    public async Task<IdempotencyResponse?> GetAsync(string idempotencyKey, string method, string path, CancellationToken cancellationToken = default)
    {
        var record = await _db.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey && r.RequestMethod == method && r.RequestPath == path, cancellationToken);
        return record == null ? null : new IdempotencyResponse(record.ResponseStatusCode, record.ResponseBody, record.ResponseContentType);
    }

    public async Task StoreAsync(string idempotencyKey, string method, string path, int statusCode, string? body, string? contentType, CancellationToken cancellationToken = default)
    {
        var record = new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            RequestMethod = method,
            RequestPath = path,
            ResponseStatusCode = statusCode,
            ResponseBody = body?.Length > 8000 ? body[..8000] : body,
            ResponseContentType = contentType?.Length > 200 ? contentType[..200] : contentType,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.IdempotencyRecords.Add(record);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // Puede ocurrir por condición de carrera (unique index). Si ya existe, no fallamos.
            var existing = await _db.IdempotencyRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey && r.RequestMethod == method && r.RequestPath == path, cancellationToken);
            if (existing != null)
                return;
            throw;
        }
    }
}
