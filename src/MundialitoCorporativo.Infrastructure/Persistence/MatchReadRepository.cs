using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Matches.Queries;

namespace MundialitoCorporativo.Infrastructure.Persistence;

public class MatchReadRepository : IMatchReadRepository
{
    private readonly string _connectionString;

    public MatchReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
    }

    public async Task<MatchDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        const string matchSql = @"
SELECT m.Id, m.HomeTeamId, m.AwayTeamId, m.RefereeId, r.FirstName + ' ' + r.LastName AS RefereeName,
       m.ScheduledAtUtc, m.Venue, CAST(m.Status AS INT) AS Status,
       m.HomeScore, m.AwayScore, ht.Name AS HomeTeamName, at.Name AS AwayTeamName, m.CreatedAtUtc
FROM Matches m
INNER JOIN Teams ht ON m.HomeTeamId = ht.Id
INNER JOIN Teams at ON m.AwayTeamId = at.Id
LEFT JOIN Referees r ON m.RefereeId = r.Id
WHERE m.Id = @Id";
        var matchRow = await conn.QuerySingleOrDefaultAsync<MatchDtoRow>(matchSql, new { Id = id });
        if (matchRow == null) return null;
        const string cardsSql = @"
SELECT c.Id, c.PlayerId, p.FirstName + ' ' + p.LastName AS PlayerName, CAST(c.CardType AS INT) AS CardType, c.Minute
FROM MatchCards c
INNER JOIN Players p ON c.PlayerId = p.Id
WHERE c.MatchId = @MatchId
ORDER BY c.Minute, c.CreatedAtUtc";
        var cards = (await conn.QueryAsync<MatchCardDto>(cardsSql, new { MatchId = id })).ToList();
        return new MatchDto(
            matchRow.Id, matchRow.HomeTeamId, matchRow.AwayTeamId, matchRow.RefereeId, matchRow.RefereeName,
            matchRow.ScheduledAtUtc, matchRow.Venue, matchRow.Status,
            matchRow.HomeScore, matchRow.AwayScore,
            matchRow.HomeTeamName, matchRow.AwayTeamName,
            cards, matchRow.CreatedAtUtc);
    }

    private sealed class MatchDtoRow
    {
        public Guid Id { get; init; }
        public Guid HomeTeamId { get; init; }
        public Guid AwayTeamId { get; init; }
        public Guid? RefereeId { get; init; }
        public string? RefereeName { get; init; }
        public DateTime ScheduledAtUtc { get; init; }
        public string? Venue { get; init; }
        public int Status { get; init; }
        public int? HomeScore { get; init; }
        public int? AwayScore { get; init; }
        public string HomeTeamName { get; init; } = "";
        public string AwayTeamName { get; init; } = "";
        public DateTime CreatedAtUtc { get; init; }
    }

    public async Task<PagedResult<MatchListItemDto>> GetPagedAsync(GetMatchesQuery query, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        var sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "ScheduledAtUtc" : query.SortBy;
        var sortDir = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var allowedSort = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ScheduledAtUtc", "HomeTeamName", "AwayTeamName", "Status", "HomeScore", "AwayScore", "Id" };
        if (!allowedSort.Contains(sortBy)) sortBy = "ScheduledAtUtc";
        var offset = (query.PageNumber - 1) * query.PageSize;
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var where = @"
FROM Matches m
INNER JOIN Teams ht ON m.HomeTeamId = ht.Id
INNER JOIN Teams at ON m.AwayTeamId = at.Id
LEFT JOIN Referees r ON m.RefereeId = r.Id
WHERE 1=1 ";
        if (query.TeamId.HasValue) where += " AND (m.HomeTeamId = @TeamId OR m.AwayTeamId = @TeamId) ";
        if (query.DateFrom.HasValue) where += " AND m.ScheduledAtUtc >= @DateFrom ";
        if (query.DateTo.HasValue) where += " AND m.ScheduledAtUtc <= @DateTo ";
        if (query.Status.HasValue) where += " AND m.Status = @Status ";

        var countSql = "SELECT COUNT(*)" + where;
        var totalRecords = await conn.ExecuteScalarAsync<int>(countSql, new { query.TeamId, query.DateFrom, query.DateTo, query.Status });

        var dataSql = $@"
SELECT m.Id, m.HomeTeamId, m.AwayTeamId, m.RefereeId, r.FirstName + ' ' + r.LastName AS RefereeName,
       m.ScheduledAtUtc, m.Venue, CAST(m.Status AS INT) AS Status,
       m.HomeScore, m.AwayScore, ht.Name AS HomeTeamName, at.Name AS AwayTeamName, m.CreatedAtUtc
{where}
ORDER BY [{sortBy}] {sortDir}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        var data = (await conn.QueryAsync<MatchListItemDto>(dataSql, new { query.TeamId, query.DateFrom, query.DateTo, query.Status, Offset = offset, PageSize = pageSize })).ToList();
        return new PagedResult<MatchListItemDto> { Data = data, PageNumber = query.PageNumber, PageSize = pageSize, TotalRecords = totalRecords };
    }
}
