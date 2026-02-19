using MediatR;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Standings.Queries;

public class GetTopScorersQueryHandler : IRequestHandler<GetTopScorersQuery, Result<IReadOnlyList<TopScorerDto>>>
{
    private readonly IStandingsReadRepository _readRepository;

    public GetTopScorersQueryHandler(IStandingsReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<IReadOnlyList<TopScorerDto>>> Handle(GetTopScorersQuery request, CancellationToken cancellationToken)
    {
        var scorers = await _readRepository.GetTopScorersAsync(request.Limit, cancellationToken);
        return Result.Success(scorers);
    }
}
