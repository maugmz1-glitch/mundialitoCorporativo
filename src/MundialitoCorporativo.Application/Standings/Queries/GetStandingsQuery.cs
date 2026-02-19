using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Standings.Queries;

public record GetStandingsQuery : IRequest<IReadOnlyList<StandingRowDto>>;
