using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task EnsureSeedAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        if (await db.Teams.AnyAsync()) return;

        var t1 = new Team { Id = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "Team Alpha", CreatedAtUtc = DateTime.UtcNow };
        var t2 = new Team { Id = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Team Beta", CreatedAtUtc = DateTime.UtcNow };
        var t3 = new Team { Id = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Team Gamma", CreatedAtUtc = DateTime.UtcNow };
        var t4 = new Team { Id = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = "Team Delta", CreatedAtUtc = DateTime.UtcNow };
        db.Teams.AddRange(t1, t2, t3, t4);

        var players = new List<Player>();
        for (var i = 0; i < 5; i++)
        {
            players.Add(new Player { Id = Guid.NewGuid(), TeamId = t1.Id, FirstName = $"Alpha", LastName = $"Player{i + 1}", JerseyNumber = $"{i + 1}", Position = "Forward", CreatedAtUtc = DateTime.UtcNow });
            players.Add(new Player { Id = Guid.NewGuid(), TeamId = t2.Id, FirstName = $"Beta", LastName = $"Player{i + 1}", JerseyNumber = $"{i + 1}", Position = "Midfielder", CreatedAtUtc = DateTime.UtcNow });
            players.Add(new Player { Id = Guid.NewGuid(), TeamId = t3.Id, FirstName = $"Gamma", LastName = $"Player{i + 1}", JerseyNumber = $"{i + 1}", Position = "Defender", CreatedAtUtc = DateTime.UtcNow });
            players.Add(new Player { Id = Guid.NewGuid(), TeamId = t4.Id, FirstName = $"Delta", LastName = $"Player{i + 1}", JerseyNumber = $"{i + 1}", Position = "Goalkeeper", CreatedAtUtc = DateTime.UtcNow });
        }
        db.Players.AddRange(players);

        var baseDate = DateTime.UtcNow.Date;
        var m1 = new Match { Id = Guid.NewGuid(), HomeTeamId = t1.Id, AwayTeamId = t2.Id, ScheduledAtUtc = baseDate.AddDays(1), Venue = "Stadium A", Status = MatchStatus.Completed, HomeScore = 2, AwayScore = 1, CreatedAtUtc = DateTime.UtcNow };
        var m2 = new Match { Id = Guid.NewGuid(), HomeTeamId = t3.Id, AwayTeamId = t4.Id, ScheduledAtUtc = baseDate.AddDays(2), Venue = "Stadium B", Status = MatchStatus.Completed, HomeScore = 0, AwayScore = 0, CreatedAtUtc = DateTime.UtcNow };
        var m3 = new Match { Id = Guid.NewGuid(), HomeTeamId = t1.Id, AwayTeamId = t3.Id, ScheduledAtUtc = baseDate.AddDays(3), Venue = "Stadium A", Status = MatchStatus.Completed, HomeScore = 3, AwayScore = 2, CreatedAtUtc = DateTime.UtcNow };
        var m4 = new Match { Id = Guid.NewGuid(), HomeTeamId = t2.Id, AwayTeamId = t4.Id, ScheduledAtUtc = baseDate.AddDays(4), Venue = "Stadium B", Status = MatchStatus.Scheduled, CreatedAtUtc = DateTime.UtcNow };
        var m5 = new Match { Id = Guid.NewGuid(), HomeTeamId = t2.Id, AwayTeamId = t3.Id, ScheduledAtUtc = baseDate.AddDays(5), Venue = "Stadium A", Status = MatchStatus.Scheduled, CreatedAtUtc = DateTime.UtcNow };
        var m6 = new Match { Id = Guid.NewGuid(), HomeTeamId = t4.Id, AwayTeamId = t1.Id, ScheduledAtUtc = baseDate.AddDays(6), Venue = "Stadium B", Status = MatchStatus.Scheduled, CreatedAtUtc = DateTime.UtcNow };
        db.Matches.AddRange(m1, m2, m3, m4, m5, m6);

        await db.SaveChangesAsync();
    }
}
