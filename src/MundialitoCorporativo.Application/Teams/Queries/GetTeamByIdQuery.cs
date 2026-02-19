using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Teams.Queries;

public record GetTeamByIdQuery(Guid Id) : IRequest<TeamDto?>;
