using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Referees.Queries;

public class GetRefereesQuery : ListQueryBase, IRequest<PagedResult<RefereeListItemDto>>
{
    public string? Name { get; set; }
}
