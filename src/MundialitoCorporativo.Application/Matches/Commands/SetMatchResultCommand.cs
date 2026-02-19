using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Matches.Queries;

namespace MundialitoCorporativo.Application.Matches.Commands;

public record SetMatchResultCommand(Guid MatchId, int HomeScore, int AwayScore) : IRequest<MatchDto>;
