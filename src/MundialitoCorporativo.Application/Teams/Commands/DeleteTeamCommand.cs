using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Teams.Commands;

public record DeleteTeamCommand(Guid Id) : IRequest<bool>;
