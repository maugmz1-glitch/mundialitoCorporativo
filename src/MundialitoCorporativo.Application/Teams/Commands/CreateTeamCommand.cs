using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Teams.Queries;

namespace MundialitoCorporativo.Application.Teams.Commands;

public record CreateTeamCommand(string Name, string? LogoUrl) : IRequest<TeamDto>;
