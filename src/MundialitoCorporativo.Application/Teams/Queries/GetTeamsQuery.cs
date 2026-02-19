using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Teams.Queries;

public class GetTeamsQuery : ListQueryBase, IRequest<PagedResult<TeamListItemDto>>
{
    public string? Name { get; set; }
}
