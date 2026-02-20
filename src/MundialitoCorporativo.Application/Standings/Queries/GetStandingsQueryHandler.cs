using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Standings.Queries;

public class GetStandingsQueryHandler : IRequestHandler<GetStandingsQuery, Result<IReadOnlyList<StandingRowDto>>>
{
    private readonly IStandingsReadRepository _readRepository;

    public GetStandingsQueryHandler(IStandingsReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<IReadOnlyList<StandingRowDto>>> Handle(GetStandingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var standings = await _readRepository.GetStandingsAsync(cancellationToken);
            return Result.Success(standings);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<StandingRowDto>>(ex.Message, ErrorCodes.Validation);
        }
    }
}
