using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Infrastructure.Persistence;

public static class SeedData
{
    /// <summary>
    /// Crea la base de datos en SQL Server si no existe (conectando a master), para evitar
    /// que MigrateAsync se ejecute contra una conexión inesperada en Docker.
    /// </summary>
    private static async Task EnsureDatabaseExistsAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
        await using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        // Nombre escapado para T-SQL: ] -> ]]
        var escaped = databaseName.Replace("]", "]]");
        var sql = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{databaseName.Replace("'", "''")}') EXEC('CREATE DATABASE [{escaped}]');";
        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public static async Task EnsureSeedAsync(this IHost host)
    {
        var config = host.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var connectionString = config["ConnectionStrings:DefaultConnection"];
        var databaseName = "Mundialito";
        if (!string.IsNullOrEmpty(connectionString))
        {
            var csBuilder = new SqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(csBuilder.InitialCatalog)) databaseName = csBuilder.InitialCatalog;
            await EnsureDatabaseExistsAsync(connectionString, databaseName);
        }

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Si EF no encontró migraciones en el ensamblado (p. ej. al ejecutar desde Api) o la tabla no existe, crear esquema desde el modelo.
        for (var i = 0; i < 2; i++)
        {
            try
            {
                if (await db.Teams.AnyAsync())
                {
                    await EnsureSampleRefereesGoalsAndCardsAsync(db);
                    return;
                }
                break;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208)
            {
                if (i == 0)
                {
                    // Si EF no aplicó migraciones (ej. "No migrations were found"), la BD puede tener solo __EFMigrationsHistory.
                    // EnsureCreated() no crea tablas si ya existe alguna; eliminamos solo esa tabla para permitir EnsureCreated.
                    await db.Database.ExecuteSqlRawAsync(
                        "IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Teams') DROP TABLE IF EXISTS [__EFMigrationsHistory];");
                    await db.Database.EnsureCreatedAsync();
                    if (await db.Teams.AnyAsync()) return;
                }
                else throw;
            }
        }

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

        // Árbitros de ejemplo
        var r1 = new Referee { Id = Guid.NewGuid(), FirstName = "Carlos", LastName = "Ramos", LicenseNumber = "REF-001", CreatedAtUtc = DateTime.UtcNow };
        var r2 = new Referee { Id = Guid.NewGuid(), FirstName = "María", LastName = "García", LicenseNumber = "REF-002", CreatedAtUtc = DateTime.UtcNow };
        var r3 = new Referee { Id = Guid.NewGuid(), FirstName = "Luis", LastName = "Martínez", LicenseNumber = "REF-003", CreatedAtUtc = DateTime.UtcNow };
        var r4 = new Referee { Id = Guid.NewGuid(), FirstName = "Ana", LastName = "López", LicenseNumber = "REF-004", CreatedAtUtc = DateTime.UtcNow };
        db.Referees.AddRange(r1, r2, r3, r4);
        m1.RefereeId = r1.Id;
        m2.RefereeId = r2.Id;
        m3.RefereeId = r3.Id;
        m4.RefereeId = r4.Id;

        await db.SaveChangesAsync();

        // Goles por jugador: resultado profesional con goleador destacado (Alpha Player1 lidera)
        // Índices: 0-4 Alpha, 5-9 Beta, 10-14 Gamma, 15-19 Delta
        var utc = DateTime.UtcNow;
        // m1: Alpha 2-1 Beta. Alpha: Player1 (2 goles); Beta: Player1 (1)
        db.MatchGoals.AddRange(
            new MatchGoal { Id = Guid.NewGuid(), MatchId = m1.Id, ScorerId = players[0].Id, Minute = 18, IsOwnGoal = false, CreatedAtUtc = utc },
            new MatchGoal { Id = Guid.NewGuid(), MatchId = m1.Id, ScorerId = players[0].Id, Minute = 64, IsOwnGoal = false, CreatedAtUtc = utc },
            new MatchGoal { Id = Guid.NewGuid(), MatchId = m1.Id, ScorerId = players[5].Id, Minute = 52, IsOwnGoal = false, CreatedAtUtc = utc }
        );
        // m2: 0-0 sin goles
        // m3: Alpha 3-2 Gamma. Alpha: Player1 (1), Player2 (1), Player4 (1); Gamma: Player1 (2)
        db.MatchGoals.AddRange(
            new MatchGoal { Id = Guid.NewGuid(), MatchId = m3.Id, ScorerId = players[0].Id, Minute = 9, IsOwnGoal = false, CreatedAtUtc = utc },
            new MatchGoal { Id = Guid.NewGuid(), MatchId = m3.Id, ScorerId = players[1].Id, Minute = 38, IsOwnGoal = false, CreatedAtUtc = utc },
            new MatchGoal { Id = Guid.NewGuid(), MatchId = m3.Id, ScorerId = players[3].Id, Minute = 72, IsOwnGoal = false, CreatedAtUtc = utc },
            new MatchGoal { Id = Guid.NewGuid(), MatchId = m3.Id, ScorerId = players[10].Id, Minute = 44, IsOwnGoal = false, CreatedAtUtc = utc },
            new MatchGoal { Id = Guid.NewGuid(), MatchId = m3.Id, ScorerId = players[10].Id, Minute = 81, IsOwnGoal = false, CreatedAtUtc = utc }
        );

        // Tarjetas de ejemplo (amarillas y rojas en partidos jugados)
        var now = DateTime.UtcNow;
        db.MatchCards.AddRange(
            new MatchCard { Id = Guid.NewGuid(), MatchId = m1.Id, PlayerId = players[2].Id, CardType = CardType.Yellow, Minute = 35, CreatedAtUtc = now },
            new MatchCard { Id = Guid.NewGuid(), MatchId = m1.Id, PlayerId = players[6].Id, CardType = CardType.Yellow, Minute = 68, CreatedAtUtc = now },
            new MatchCard { Id = Guid.NewGuid(), MatchId = m1.Id, PlayerId = players[7].Id, CardType = CardType.Red, Minute = 89, CreatedAtUtc = now },
            new MatchCard { Id = Guid.NewGuid(), MatchId = m2.Id, PlayerId = players[10].Id, CardType = CardType.Yellow, Minute = 22, CreatedAtUtc = now },
            new MatchCard { Id = Guid.NewGuid(), MatchId = m2.Id, PlayerId = players[15].Id, CardType = CardType.Yellow, Minute = 55, CreatedAtUtc = now },
            new MatchCard { Id = Guid.NewGuid(), MatchId = m3.Id, PlayerId = players[1].Id, CardType = CardType.Yellow, Minute = 40, CreatedAtUtc = now },
            new MatchCard { Id = Guid.NewGuid(), MatchId = m3.Id, PlayerId = players[12].Id, CardType = CardType.Yellow, Minute = 60, CreatedAtUtc = now },
            new MatchCard { Id = Guid.NewGuid(), MatchId = m3.Id, PlayerId = players[12].Id, CardType = CardType.Red, Minute = 61, CreatedAtUtc = now }
        );

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Si faltan árbitros, goles o tarjetas, agrega datos de ejemplo (para BDs ya creadas).
    /// </summary>
    private static async Task EnsureSampleRefereesGoalsAndCardsAsync(AppDbContext db)
    {
        if (!await db.Referees.AnyAsync())
        {
            var r1 = new Referee { Id = Guid.NewGuid(), FirstName = "Carlos", LastName = "Ramos", LicenseNumber = "REF-001", CreatedAtUtc = DateTime.UtcNow };
            var r2 = new Referee { Id = Guid.NewGuid(), FirstName = "María", LastName = "García", LicenseNumber = "REF-002", CreatedAtUtc = DateTime.UtcNow };
            var r3 = new Referee { Id = Guid.NewGuid(), FirstName = "Luis", LastName = "Martínez", LicenseNumber = "REF-003", CreatedAtUtc = DateTime.UtcNow };
            var r4 = new Referee { Id = Guid.NewGuid(), FirstName = "Ana", LastName = "López", LicenseNumber = "REF-004", CreatedAtUtc = DateTime.UtcNow };
            db.Referees.AddRange(r1, r2, r3, r4);
            await db.SaveChangesAsync();

            var matchesToAssign = await db.Matches.OrderBy(m => m.ScheduledAtUtc).Take(4).ToListAsync();
            var refs = new[] { r1, r2, r3, r4 };
            for (var i = 0; i < Math.Min(matchesToAssign.Count, refs.Length); i++)
            {
                matchesToAssign[i].RefereeId = refs[i].Id;
            }
            await db.SaveChangesAsync();
        }

        if (!await db.MatchGoals.AnyAsync())
        {
            var completed = await db.Matches.Where(m => m.Status == MatchStatus.Completed && m.HomeScore != null && m.AwayScore != null)
                .Include(m => m.HomeTeam).Include(m => m.AwayTeam).ToListAsync();
            var utc = DateTime.UtcNow;
            foreach (var m in completed)
            {
                var homePlayers = await db.Players.Where(p => p.TeamId == m.HomeTeamId).OrderBy(p => p.JerseyNumber).Take(5).ToListAsync();
                var awayPlayers = await db.Players.Where(p => p.TeamId == m.AwayTeamId).OrderBy(p => p.JerseyNumber).Take(5).ToListAsync();
                var h = m.HomeScore ?? 0;
                var a = m.AwayScore ?? 0;
                // Distribución profesional: goleador (primer jugador) marca más, resto repartido; minutos variados
                var homeMinutes = new[] { 8, 23, 45, 64, 78 };
                var awayMinutes = new[] { 15, 32, 51, 69, 88 };
                for (var g = 0; g < h; g++)
                {
                    var scorerIdx = g < 2 && homePlayers.Count > 0 ? 0 : Math.Min(g, homePlayers.Count - 1);
                    db.MatchGoals.Add(new MatchGoal { Id = Guid.NewGuid(), MatchId = m.Id, ScorerId = homePlayers[scorerIdx].Id, Minute = homeMinutes[g % homeMinutes.Length], IsOwnGoal = false, CreatedAtUtc = utc });
                }
                for (var g = 0; g < a; g++)
                {
                    var scorerIdx = g < 2 && awayPlayers.Count > 0 ? 0 : Math.Min(g, awayPlayers.Count - 1);
                    db.MatchGoals.Add(new MatchGoal { Id = Guid.NewGuid(), MatchId = m.Id, ScorerId = awayPlayers[scorerIdx].Id, Minute = awayMinutes[g % awayMinutes.Length], IsOwnGoal = false, CreatedAtUtc = utc });
                }
            }
            await db.SaveChangesAsync();
        }

        if (!await db.MatchCards.AnyAsync())
        {
            var matchesWithPlayers = await db.Matches.Where(m => m.Status == MatchStatus.Completed).Take(3).ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var m in matchesWithPlayers)
            {
                var players = await db.Players.Where(p => p.TeamId == m.HomeTeamId || p.TeamId == m.AwayTeamId).Take(4).ToListAsync();
                if (players.Count >= 2)
                {
                    db.MatchCards.Add(new MatchCard { Id = Guid.NewGuid(), MatchId = m.Id, PlayerId = players[0].Id, CardType = CardType.Yellow, Minute = 25, CreatedAtUtc = now });
                    db.MatchCards.Add(new MatchCard { Id = Guid.NewGuid(), MatchId = m.Id, PlayerId = players[1].Id, CardType = CardType.Yellow, Minute = 70, CreatedAtUtc = now });
                    if (players.Count >= 3)
                        db.MatchCards.Add(new MatchCard { Id = Guid.NewGuid(), MatchId = m.Id, PlayerId = players[2].Id, CardType = CardType.Red, Minute = 85, CreatedAtUtc = now });
                }
            }
            await db.SaveChangesAsync();
        }
    }
}
