using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Players.Queries;

public class GetPlayersQueryHandler : IRequestHandler<GetPlayersQuery, Result<PagedResult<PlayerListItemDto>>>
{
    private readonly IPlayerReadRepository _readRepository;

    public GetPlayersQueryHandler(IPlayerReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<PagedResult<PlayerListItemDto>>> Handle(GetPlayersQuery request, CancellationToken cancellationToken)
    {
        var paged = await _readRepository.GetPagedAsync(request, cancellationToken);
        return Result.Success(paged);
    }
}
