using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Standings.Queries;

public record GetTopScorersQuery(int? Limit = 10) : IRequest<IReadOnlyList<TopScorerDto>>;
