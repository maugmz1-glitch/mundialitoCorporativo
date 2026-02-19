using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Matches.Queries;

public class GetMatchesQuery : ListQueryBase, IRequest<PagedResult<MatchListItemDto>>
{
    public Guid? TeamId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? Status { get; set; }
}
