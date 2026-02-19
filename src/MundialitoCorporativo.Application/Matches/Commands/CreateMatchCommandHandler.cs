using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Matches.Queries;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Application.Matches.Commands;

public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, Result<MatchDto>>
{
    private readonly IAppDbContext _db;
    private readonly IMatchReadRepository _readRepository;

    public CreateMatchCommandHandler(IAppDbContext db, IMatchReadRepository readRepository)
    {
        _db = db;
        _readRepository = readRepository;
    }

    public async Task<Result<MatchDto>> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
    {
        if (request.HomeTeamId == request.AwayTeamId)
            return Result.Failure<MatchDto>("Home and away team must be different.", ErrorCodes.Validation);
        if (await _db.Teams.FindAsync([request.HomeTeamId], cancellationToken) == null)
            return Result.Failure<MatchDto>("Home team not found.", ErrorCodes.NotFound);
        if (await _db.Teams.FindAsync([request.AwayTeamId], cancellationToken) == null)
            return Result.Failure<MatchDto>("Away team not found.", ErrorCodes.NotFound);
        var match = new Match
        {
            Id = Guid.NewGuid(),
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            ScheduledAtUtc = request.ScheduledAtUtc,
            Venue = request.Venue?.Trim(),
            Status = MatchStatus.Scheduled,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await _readRepository.GetByIdAsync(match.Id, cancellationToken);
        return Result.Success(dto!);
    }
}
