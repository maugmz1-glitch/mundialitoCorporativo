using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Teams.Queries;

namespace MundialitoCorporativo.Infrastructure.Persistence;

public class TeamReadRepository : ITeamReadRepository
{
    private readonly string _connectionString;

    public TeamReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
    }

    public async Task<TeamDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        return await conn.QuerySingleOrDefaultAsync<TeamDto>(
            "SELECT Id, Name, LogoUrl, CreatedAtUtc FROM Teams WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<PagedResult<TeamListItemDto>> GetPagedAsync(GetTeamsQuery query, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        var name = query.Name?.Trim();
        var sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "Name" : query.SortBy;
        var sortDir = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var allowedSort = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name", "CreatedAtUtc", "Id" };
        if (!allowedSort.Contains(sortBy)) sortBy = "Name";
        var offset = (query.PageNumber - 1) * query.PageSize;
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var where = " WHERE 1=1 ";
        if (!string.IsNullOrEmpty(name)) where += " AND Name LIKE @NameFilter ";
        var nameFilter = string.IsNullOrEmpty(name) ? null : $"%{name}%";

        var countSql = "SELECT COUNT(*) FROM Teams" + where;
        var totalRecords = await conn.ExecuteScalarAsync<int>(countSql, new { NameFilter = nameFilter });

        var dataSql = $@"
SELECT Id, Name, LogoUrl, CreatedAtUtc
FROM Teams
{where}
ORDER BY [{sortBy}] {sortDir}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        var data = (await conn.QueryAsync<TeamListItemDto>(dataSql, new { NameFilter = nameFilter, Offset = offset, PageSize = pageSize })).ToList();
        return new PagedResult<TeamListItemDto> { Data = data, PageNumber = query.PageNumber, PageSize = pageSize, TotalRecords = totalRecords };
    }
}
