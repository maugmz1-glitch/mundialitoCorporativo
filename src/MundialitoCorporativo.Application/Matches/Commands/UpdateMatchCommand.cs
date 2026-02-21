using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Matches.Queries;

namespace MundialitoCorporativo.Application.Matches.Commands;

public record UpdateMatchCommand(Guid Id, Guid HomeTeamId, Guid AwayTeamId, Guid? RefereeId, DateTime ScheduledAtUtc, string? Venue, int Status) : IRequest<MatchDto>;
