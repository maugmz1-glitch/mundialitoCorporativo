using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Teams.Queries;

namespace MundialitoCorporativo.Application.Interfaces;

public interface ITeamReadRepository
{
    Task<TeamDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TeamListItemDto>> GetPagedAsync(GetTeamsQuery query, CancellationToken cancellationToken = default);
}
