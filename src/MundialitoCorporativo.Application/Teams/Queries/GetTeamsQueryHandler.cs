using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Teams.Queries;

public class GetTeamsQueryHandler : IRequestHandler<GetTeamsQuery, Result<PagedResult<TeamListItemDto>>>
{
    private readonly ITeamReadRepository _readRepository;

    public GetTeamsQueryHandler(ITeamReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<PagedResult<TeamListItemDto>>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _readRepository.GetPagedAsync(request, cancellationToken);
        return Result.Success(paged);
    }
}
