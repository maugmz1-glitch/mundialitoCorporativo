using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Teams.Commands;

public class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand, Result<bool>>
{
    private readonly IAppDbContext _db;

    public DeleteTeamCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<bool>> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _db.Teams.FindAsync([request.Id], cancellationToken);
        if (team == null)
            return Result.Failure<bool>("Team not found.", ErrorCodes.NotFound);
        _db.Teams.Remove(team);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
