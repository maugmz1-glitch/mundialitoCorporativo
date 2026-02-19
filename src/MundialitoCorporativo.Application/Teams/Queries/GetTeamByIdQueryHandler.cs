using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Teams.Queries;

public class GetTeamByIdQueryHandler : IRequestHandler<GetTeamByIdQuery, Result<TeamDto?>>
{
    private readonly ITeamReadRepository _readRepository;

    public GetTeamByIdQueryHandler(ITeamReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<TeamDto?>> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var team = await _readRepository.GetByIdAsync(request.Id, cancellationToken);
        return Result.Success(team);
    }
}
