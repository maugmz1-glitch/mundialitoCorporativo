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
                if (await db.Teams.AnyAsync()) return;
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

        await db.SaveChangesAsync();
    }
}
