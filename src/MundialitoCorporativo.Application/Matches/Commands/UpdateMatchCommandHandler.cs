using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Matches.Queries;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Application.Matches.Commands;

public class UpdateMatchCommandHandler : IRequestHandler<UpdateMatchCommand, Result<MatchDto>>
{
    private readonly IAppDbContext _db;
    private readonly IMatchReadRepository _readRepository;

    public UpdateMatchCommandHandler(IAppDbContext db, IMatchReadRepository readRepository)
    {
        _db = db;
        _readRepository = readRepository;
    }

    public async Task<Result<MatchDto>> Handle(UpdateMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _db.Matches.FindAsync([request.Id], cancellationToken);
        if (match == null)
            return Result.Failure<MatchDto>("Match not found.", ErrorCodes.NotFound);
        if (request.HomeTeamId == request.AwayTeamId)
            return Result.Failure<MatchDto>("Home and away team must be different.", ErrorCodes.Validation);
        if (await _db.Teams.FindAsync([request.HomeTeamId], cancellationToken) == null)
            return Result.Failure<MatchDto>("Home team not found.", ErrorCodes.NotFound);
        if (await _db.Teams.FindAsync([request.AwayTeamId], cancellationToken) == null)
            return Result.Failure<MatchDto>("Away team not found.", ErrorCodes.NotFound);
        if (request.RefereeId.HasValue && await _db.Referees.FindAsync([request.RefereeId.Value], cancellationToken) == null)
            return Result.Failure<MatchDto>("Referee not found.", ErrorCodes.NotFound);
        match.HomeTeamId = request.HomeTeamId;
        match.AwayTeamId = request.AwayTeamId;
        match.RefereeId = request.RefereeId;
        match.ScheduledAtUtc = request.ScheduledAtUtc;
        match.Venue = request.Venue?.Trim();
        match.Status = (MatchStatus)request.Status;
        match.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await _readRepository.GetByIdAsync(match.Id, cancellationToken);
        return Result.Success(dto!);
    }
}
