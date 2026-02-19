using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Teams.Queries;

namespace MundialitoCorporativo.Application.Teams.Commands;

public record UpdateTeamCommand(Guid Id, string Name, string? LogoUrl) : IRequest<TeamDto>;
