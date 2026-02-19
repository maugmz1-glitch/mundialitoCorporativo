using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Players.Queries;

namespace MundialitoCorporativo.Infrastructure.Persistence;

public class PlayerReadRepository : IPlayerReadRepository
{
    private readonly string _connectionString;

    public PlayerReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
    }

    public async Task<PlayerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        return await conn.QuerySingleOrDefaultAsync<PlayerDto>(
            "SELECT p.Id, p.TeamId, p.FirstName, p.LastName, p.JerseyNumber, p.Position, p.CreatedAtUtc FROM Players p WHERE p.Id = @Id",
            new { Id = id });
    }

    public async Task<PagedResult<PlayerListItemDto>> GetPagedAsync(GetPlayersQuery query, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        var name = query.Name?.Trim();
        var sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "LastName" : query.SortBy;
        var sortDir = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var allowedSort = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "FirstName", "LastName", "JerseyNumber", "TeamName", "CreatedAtUtc", "Id" };
        if (!allowedSort.Contains(sortBy)) sortBy = "LastName";
        var offset = (query.PageNumber - 1) * query.PageSize;
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var where = " FROM Players p INNER JOIN Teams t ON p.TeamId = t.Id WHERE 1=1 ";
        if (query.TeamId.HasValue) where += " AND p.TeamId = @TeamId ";
        if (!string.IsNullOrEmpty(name)) where += " AND (p.FirstName + ' ' + p.LastName LIKE @NameFilter OR p.LastName + ', ' + p.FirstName LIKE @NameFilter) ";
        var nameFilter = string.IsNullOrEmpty(name) ? null : $"%{name}%";

        var countSql = "SELECT COUNT(*)" + where;
        var totalRecords = await conn.ExecuteScalarAsync<int>(countSql, new { query.TeamId, NameFilter = nameFilter });

        var dataSql = $@"
SELECT p.Id, p.TeamId, p.FirstName, p.LastName, p.JerseyNumber, p.Position, t.Name AS TeamName, p.CreatedAtUtc
{where}
ORDER BY [{sortBy}] {sortDir}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        var data = (await conn.QueryAsync<PlayerListItemDto>(dataSql, new { query.TeamId, NameFilter = nameFilter, Offset = offset, PageSize = pageSize })).ToList();
        return new PagedResult<PlayerListItemDto> { Data = data, PageNumber = query.PageNumber, PageSize = pageSize, TotalRecords = totalRecords };
    }
}
