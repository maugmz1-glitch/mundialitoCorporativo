using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Matches.Queries;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Application.Matches.Commands;

public class SetMatchResultCommandHandler : IRequestHandler<SetMatchResultCommand, Result<MatchDto>>
{
    private readonly IAppDbContext _db;
    private readonly IMatchReadRepository _readRepository;

    public SetMatchResultCommandHandler(IAppDbContext db, IMatchReadRepository readRepository)
    {
        _db = db;
        _readRepository = readRepository;
    }

    public async Task<Result<MatchDto>> Handle(SetMatchResultCommand request, CancellationToken cancellationToken)
    {
        var match = await _db.Matches.FindAsync([request.MatchId], cancellationToken);
        if (match == null)
            return Result.Failure<MatchDto>("Match not found.", ErrorCodes.NotFound);
        if (match.Status != MatchStatus.Scheduled && match.Status != MatchStatus.InProgress)
            return Result.Failure<MatchDto>("Match cannot be updated with result in current status.", ErrorCodes.Validation);
        if (request.HomeScore < 0 || request.AwayScore < 0)
            return Result.Failure<MatchDto>("Scores cannot be negative.", ErrorCodes.Validation);
        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.Status = MatchStatus.Completed;
        match.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await _readRepository.GetByIdAsync(match.Id, cancellationToken);
        return Result.Success(dto!);
    }
}
