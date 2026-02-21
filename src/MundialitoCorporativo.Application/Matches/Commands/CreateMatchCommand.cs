using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Matches.Queries;

namespace MundialitoCorporativo.Application.Matches.Commands;

public record CreateMatchCommand(Guid HomeTeamId, Guid AwayTeamId, Guid? RefereeId, DateTime ScheduledAtUtc, string? Venue) : IRequest<MatchDto>;
