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
}
