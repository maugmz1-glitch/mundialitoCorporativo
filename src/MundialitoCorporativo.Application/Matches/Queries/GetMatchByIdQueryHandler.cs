using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Matches.Queries;

public class GetMatchByIdQueryHandler : IRequestHandler<GetMatchByIdQuery, Result<MatchDto?>>
{
    private readonly IMatchReadRepository _readRepository;

    public GetMatchByIdQueryHandler(IMatchReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<MatchDto?>> Handle(GetMatchByIdQuery request, CancellationToken cancellationToken)
    {
        var match = await _readRepository.GetByIdAsync(request.Id, cancellationToken);
        return Result.Success(match);
    }
}
