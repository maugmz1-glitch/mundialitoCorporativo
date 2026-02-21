using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Referees.Queries;

namespace MundialitoCorporativo.Infrastructure.Persistence;

public class RefereeReadRepository : IRefereeReadRepository
{
    private readonly string _connectionString;

    public RefereeReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
    }

    public async Task<RefereeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        return await conn.QuerySingleOrDefaultAsync<RefereeDto>(
            "SELECT Id, FirstName, LastName, LicenseNumber, CreatedAtUtc FROM Referees WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<PagedResult<RefereeListItemDto>> GetPagedAsync(GetRefereesQuery query, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        var name = query.Name?.Trim();
        var sortByRaw = query.SortBy?.Trim();
        var sortBy = sortByRaw?.ToLowerInvariant() switch
        {
            "lastname" => "LastName",
            "createdatutc" => "CreatedAtUtc",
            "id" => "Id",
            _ => "FirstName"
        };
        var sortDir = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var offset = (query.PageNumber - 1) * query.PageSize;
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var where = " WHERE 1=1 ";
        if (!string.IsNullOrEmpty(name))
        {
            where += " AND (LOWER(FirstName + ' ' + LastName) LIKE @NameFilter OR LOWER(LastName + ' ' + FirstName) LIKE @NameFilter) ";
        }
        var nameFilter = string.IsNullOrEmpty(name) ? null : $"%{name.ToLowerInvariant()}%";

        var countSql = "SELECT COUNT(*) FROM Referees" + where;
        var totalRecords = await conn.ExecuteScalarAsync<int>(countSql, new { NameFilter = nameFilter });

        var dataSql = $@"
SELECT Id, FirstName, LastName, LicenseNumber, CreatedAtUtc
FROM Referees
{where}
ORDER BY [{sortBy}] {sortDir}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        var data = (await conn.QueryAsync<RefereeListItemDto>(dataSql, new { NameFilter = nameFilter, Offset = offset, PageSize = pageSize })).ToList();
        return new PagedResult<RefereeListItemDto> { Data = data, PageNumber = query.PageNumber, PageSize = pageSize, TotalRecords = totalRecords };
    }
}
