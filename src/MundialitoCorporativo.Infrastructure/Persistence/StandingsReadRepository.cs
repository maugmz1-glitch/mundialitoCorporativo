using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Standings.Queries;

namespace MundialitoCorporativo.Infrastructure.Persistence;

/// <summary>
/// Standings: order by Points DESC, GoalDifferential DESC, GoalsFor DESC (tournament rules).
/// Points: Win=3, Draw=1, Loss=0.
/// </summary>
public class StandingsReadRepository : IStandingsReadRepository
{
    private readonly string _connectionString;

    public StandingsReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
    }

    /// <summary>
    /// Operación importante: toda la tabla de posiciones en una sola consulta Dapper.
    /// CTE CompletedMatches: cada fila es un partido completado visto desde un equipo (GF, GA, IsWin, IsDraw, IsLoss).
    /// Agg: por equipo suma puntos (3/1/0), partidos jugados, goles. Orden: Points DESC, GD DESC, GF DESC.
    /// </summary>
    public async Task<IReadOnlyList<StandingRowDto>> GetStandingsAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        const string sql = @"
;WITH CompletedMatches AS (
    SELECT HomeTeamId AS TeamId, HomeScore AS GoalsFor, AwayScore AS GoalsAgainst,
           CASE WHEN HomeScore > AwayScore THEN 1 ELSE 0 END AS IsWin,
           CASE WHEN HomeScore = AwayScore THEN 1 ELSE 0 END AS IsDraw,
           CASE WHEN HomeScore < AwayScore THEN 1 ELSE 0 END AS IsLoss
    FROM Matches WHERE Status = 2 AND HomeScore IS NOT NULL AND AwayScore IS NOT NULL
    UNION ALL
    SELECT AwayTeamId, AwayScore, HomeScore,
           CASE WHEN AwayScore > HomeScore THEN 1 ELSE 0 END,
           CASE WHEN AwayScore = HomeScore THEN 1 ELSE 0 END,
           CASE WHEN AwayScore < HomeScore THEN 1 ELSE 0 END
    FROM Matches WHERE Status = 2 AND HomeScore IS NOT NULL AND AwayScore IS NOT NULL
),
Agg AS (
    SELECT t.Id AS TeamId, t.Name AS TeamName,
           ISNULL(SUM(cm.IsWin), 0) * 3 + ISNULL(SUM(cm.IsDraw), 0) AS Points,
           ISNULL(SUM(cm.IsWin), 0) AS Won, ISNULL(SUM(cm.IsDraw), 0) AS Drawn, ISNULL(SUM(cm.IsLoss), 0) AS Lost,
           ISNULL(SUM(cm.GoalsFor), 0) AS GoalsFor, ISNULL(SUM(cm.GoalsAgainst), 0) AS GoalsAgainst
    FROM Teams t
    LEFT JOIN CompletedMatches cm ON t.Id = cm.TeamId
    GROUP BY t.Id, t.Name
)
SELECT ROW_NUMBER() OVER (ORDER BY Points DESC, (GoalsFor - GoalsAgainst) DESC, GoalsFor DESC) AS Rank,
       TeamId, TeamName,
       Won + Drawn + Lost AS Played, Won, Drawn, Lost,
       GoalsFor, GoalsAgainst, (GoalsFor - GoalsAgainst) AS GoalDifferential, Points
FROM Agg
ORDER BY Rank";
        var rows = (await conn.QueryAsync<StandingRowDto>(sql)).ToList();
        return rows;
    }

    public async Task<IReadOnlyList<TopScorerDto>> GetTopScorersAsync(int? limit, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        var take = Math.Clamp(limit ?? 10, 1, 100);
        const string sql = @"
SELECT TOP (@Take) p.Id AS PlayerId,
       p.FirstName + ' ' + p.LastName AS PlayerName,
       t.Name AS TeamName,
       COUNT(g.Id) AS Goals
FROM Players p
INNER JOIN Teams t ON p.TeamId = t.Id
LEFT JOIN MatchGoals g ON g.ScorerId = p.Id AND g.IsOwnGoal = 0
GROUP BY p.Id, p.FirstName, p.LastName, t.Name
ORDER BY COUNT(g.Id) DESC";
        var rows = (await conn.QueryAsync<TopScorerDto>(sql, new { Take = take })).ToList();
        return rows;
    }
}
