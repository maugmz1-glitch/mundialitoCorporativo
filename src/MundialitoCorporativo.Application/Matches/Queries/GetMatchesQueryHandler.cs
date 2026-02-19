using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Matches.Queries;

public class GetMatchesQueryHandler : IRequestHandler<GetMatchesQuery, Result<PagedResult<MatchListItemDto>>>
{
    private readonly IMatchReadRepository _readRepository;

    public GetMatchesQueryHandler(IMatchReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<PagedResult<MatchListItemDto>>> Handle(GetMatchesQuery request, CancellationToken cancellationToken)
    {
        var paged = await _readRepository.GetPagedAsync(request, cancellationToken);
        return Result.Success(paged);
    }
}
