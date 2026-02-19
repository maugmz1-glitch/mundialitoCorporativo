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
    public void IdempotencyStore_GetAsync_ReturnsNull_WhenNoRecord()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "Idempotency_Empty")
            .Options;
        using var db = new AppDbContext(options);
        var store = new IdempotencyStore(db);
        var result = store.GetAsync("key1", "POST", "/api/teams").GetAwaiter().GetResult();
        Assert.Null(result);
    }

    [Fact]
    public void IdempotencyStore_StoreAndGet_ReturnsStoredResponse()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "Idempotency_Store_" + Guid.NewGuid())
            .Options;
        using (var db = new AppDbContext(options))
        {
            db.Database.EnsureCreated();
            var store = new IdempotencyStore(db);
            store.StoreAsync("key2", "POST", "/api/teams", 201, "{\"id\":\"x\"}").GetAwaiter().GetResult();
        }
        using (var db = new AppDbContext(options))
        {
            var store = new IdempotencyStore(db);
            var got = store.GetAsync("key2", "POST", "/api/teams").GetAwaiter().GetResult();
            Assert.NotNull(got);
            Assert.Equal(201, got.StatusCode);
            Assert.Equal("{\"id\":\"x\"}", got.Body);
        }
    }
}
