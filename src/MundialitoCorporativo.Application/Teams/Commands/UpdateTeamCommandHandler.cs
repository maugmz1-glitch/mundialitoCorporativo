using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Teams.Queries;
using MundialitoCorporativo.Domain.Common;
namespace MundialitoCorporativo.Application.Teams.Commands;

public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, Result<TeamDto>>
{
    private readonly IAppDbContext _db;

    public UpdateTeamCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<TeamDto>> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _db.Teams.FindAsync([request.Id], cancellationToken);
        if (team == null)
            return Result.Failure<TeamDto>("Team not found.", ErrorCodes.NotFound);
        team.Name = request.Name.Trim();
        team.LogoUrl = request.LogoUrl?.Trim();
        team.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(new TeamDto(team.Id, team.Name, team.LogoUrl, team.CreatedAtUtc));
    }
}
