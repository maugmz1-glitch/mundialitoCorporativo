using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Players.Queries;

public class GetPlayerByIdQueryHandler : IRequestHandler<GetPlayerByIdQuery, Result<PlayerDto?>>
{
    private readonly IPlayerReadRepository _readRepository;

    public GetPlayerByIdQueryHandler(IPlayerReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<PlayerDto?>> Handle(GetPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        var player = await _readRepository.GetByIdAsync(request.Id, cancellationToken);
        return Result.Success(player);
    }
}
