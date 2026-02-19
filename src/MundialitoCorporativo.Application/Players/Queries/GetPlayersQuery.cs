using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Players.Queries;

public class GetPlayersQuery : ListQueryBase, IRequest<PagedResult<PlayerListItemDto>>
{
    public Guid? TeamId { get; set; }
    public string? Name { get; set; }
}
