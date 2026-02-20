using MediatR;
using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Referees.Queries;

public class GetRefereesQueryHandler : IRequestHandler<GetRefereesQuery, Result<PagedResult<RefereeListItemDto>>>
{
    private readonly IAppDbContext _db;

    public GetRefereesQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<PagedResult<RefereeListItemDto>>> Handle(GetRefereesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Referees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var term = request.Name.Trim().ToLower();
            query = query.Where(r => (r.FirstName + " " + r.LastName).ToLower().Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        var order = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
        var sorted = request.SortBy?.ToLowerInvariant() switch
        {
            "lastname" => order == "desc" ? query.OrderByDescending(r => r.LastName) : query.OrderBy(r => r.LastName),
            "createdatutc" => order == "desc" ? query.OrderByDescending(r => r.CreatedAtUtc) : query.OrderBy(r => r.CreatedAtUtc),
            _ => order == "desc" ? query.OrderByDescending(r => r.FirstName) : query.OrderBy(r => r.FirstName)
        };
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var pageNumber = Math.Max(1, request.PageNumber);
        var items = await sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RefereeListItemDto(r.Id, r.FirstName, r.LastName, r.LicenseNumber, r.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return Result.Success(new PagedResult<RefereeListItemDto> { Data = items, PageNumber = pageNumber, PageSize = pageSize, TotalRecords = total });
    }
}
