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
        return record == null ? null : new IdempotencyResponse(record.ResponseStatusCode, record.ResponseBody);
    }

    public async Task StoreAsync(string idempotencyKey, string method, string path, int statusCode, string? body, CancellationToken cancellationToken = default)
    {
        _db.IdempotencyRecords.Add(new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            RequestMethod = method,
            RequestPath = path,
            ResponseStatusCode = statusCode,
            ResponseBody = body?.Length > 8000 ? body[..8000] : body,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
