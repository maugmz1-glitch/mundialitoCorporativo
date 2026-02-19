using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Matches.Queries;

namespace MundialitoCorporativo.Application.Interfaces;

public interface IMatchReadRepository
{
    Task<MatchDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<MatchListItemDto>> GetPagedAsync(GetMatchesQuery query, CancellationToken cancellationToken = default);
}
