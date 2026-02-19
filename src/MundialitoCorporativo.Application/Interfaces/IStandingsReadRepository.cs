using MundialitoCorporativo.Application.Standings.Queries;

namespace MundialitoCorporativo.Application.Interfaces;

public interface IStandingsReadRepository
{
    Task<IReadOnlyList<StandingRowDto>> GetStandingsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopScorerDto>> GetTopScorersAsync(int? limit, CancellationToken cancellationToken = default);
}
