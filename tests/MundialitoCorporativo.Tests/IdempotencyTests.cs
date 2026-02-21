using Moq;
using Xunit;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Teams.Commands;
using MundialitoCorporativo.Application.Teams.Queries;
using MundialitoCorporativo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Infrastructure.Persistence;

namespace MundialitoCorporativo.Tests;

public class IdempotencyTests
{
    [Fact]
    public async Task IdempotencyStore_GetAsync_ReturnsNull_WhenNoRecord()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "Idempotency_Empty")
            .Options;
        await using var db = new AppDbContext(options);
        var store = new IdempotencyStore(db);
        var result = await store.GetAsync("key1", "POST", "/api/teams");
        Assert.Null(result);
    }

    [Fact]
    public async Task IdempotencyStore_StoreAndGet_ReturnsStoredResponse()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "Idempotency_Store_" + Guid.NewGuid())
            .Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            var store = new IdempotencyStore(db);
            await store.StoreAsync("key2", "POST", "/api/teams", 201, "{\"id\":\"x\"}");
        }
        await using (var db = new AppDbContext(options))
        {
            var store = new IdempotencyStore(db);
            var got = await store.GetAsync("key2", "POST", "/api/teams");
            Assert.NotNull(got);
            Assert.Equal(201, got.StatusCode);
            Assert.Equal("{\"id\":\"x\"}", got.Body);
        }
    }

    /// <summary>
    /// Validación de idempotencia: la misma clave devuelve la misma respuesta guardada (segunda llamada no re-ejecuta).
    /// </summary>
    [Fact]
    public async Task IdempotencyStore_SameKey_ReturnsSameResponse_Twice()
    {
        var dbName = "Idempotency_SameKey_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            var store = new IdempotencyStore(db);
            await store.StoreAsync("idem-key-1", "POST", "/api/teams", 201, "{\"id\":\"abc\",\"name\":\"Team\"}");
        }
        int firstStatusCode;
        string? firstBody;
        await using (var db1 = new AppDbContext(options))
        {
            var store1 = new IdempotencyStore(db1);
            var first = await store1.GetAsync("idem-key-1", "POST", "/api/teams");
            Assert.NotNull(first);
            firstStatusCode = first.StatusCode;
            firstBody = first.Body;
            Assert.Equal(201, firstStatusCode);
            Assert.Equal("{\"id\":\"abc\",\"name\":\"Team\"}", firstBody);
        }
        await using (var db2 = new AppDbContext(options))
        {
            var store2 = new IdempotencyStore(db2);
            var second = await store2.GetAsync("idem-key-1", "POST", "/api/teams");
            Assert.NotNull(second);
            Assert.Equal(firstStatusCode, second.StatusCode);
            Assert.Equal(firstBody, second.Body);
        }
    }

    [Fact]
    public async Task IdempotencyStore_Get_DifferentMethodOrPath_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "Idempotency_Diff_" + Guid.NewGuid())
            .Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            var store = new IdempotencyStore(db);
            await store.StoreAsync("k", "POST", "/api/teams", 201, "{}");
        }
        await using (var db = new AppDbContext(options))
        {
            var store = new IdempotencyStore(db);
            Assert.Null(await store.GetAsync("k", "PUT", "/api/teams"));
            Assert.Null(await store.GetAsync("k", "POST", "/api/players"));
        }
    }
}
