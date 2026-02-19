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
        const string sql = @"
SELECT m.Id, m.HomeTeamId, m.AwayTeamId, m.ScheduledAtUtc, m.Venue, CAST(m.Status AS INT) AS Status,
       m.HomeScore, m.AwayScore, ht.Name AS HomeTeamName, at.Name AS AwayTeamName, m.CreatedAtUtc
FROM Matches m
INNER JOIN Teams ht ON m.HomeTeamId = ht.Id
INNER JOIN Teams at ON m.AwayTeamId = at.Id
WHERE m.Id = @Id";
        return await conn.QuerySingleOrDefaultAsync<MatchDto>(sql, new { Id = id });
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
WHERE 1=1 ";
        if (query.TeamId.HasValue) where += " AND (m.HomeTeamId = @TeamId OR m.AwayTeamId = @TeamId) ";
        if (query.DateFrom.HasValue) where += " AND m.ScheduledAtUtc >= @DateFrom ";
        if (query.DateTo.HasValue) where += " AND m.ScheduledAtUtc <= @DateTo ";
        if (query.Status.HasValue) where += " AND m.Status = @Status ";

        var countSql = "SELECT COUNT(*)" + where;
        var totalRecords = await conn.ExecuteScalarAsync<int>(countSql, new { query.TeamId, query.DateFrom, query.DateTo, query.Status });

        var dataSql = $@"
SELECT m.Id, m.HomeTeamId, m.AwayTeamId, m.ScheduledAtUtc, m.Venue, CAST(m.Status AS INT) AS Status,
       m.HomeScore, m.AwayScore, ht.Name AS HomeTeamName, at.Name AS AwayTeamName, m.CreatedAtUtc
{where}
ORDER BY [{sortBy}] {sortDir}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        var data = (await conn.QueryAsync<MatchListItemDto>(dataSql, new { query.TeamId, query.DateFrom, query.DateTo, query.Status, Offset = offset, PageSize = pageSize })).ToList();
        return new PagedResult<MatchListItemDto> { Data = data, PageNumber = query.PageNumber, PageSize = pageSize, TotalRecords = totalRecords };
    }
}
