using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Players.Queries;

namespace MundialitoCorporativo.Application.Interfaces;

public interface IPlayerReadRepository
{
    Task<PlayerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<PlayerListItemDto>> GetPagedAsync(GetPlayersQuery query, CancellationToken cancellationToken = default);
}
