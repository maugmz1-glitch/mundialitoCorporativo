using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Players.Queries;

namespace MundialitoCorporativo.Application.Players.Commands;

public record UpdatePlayerCommand(Guid Id, Guid TeamId, string FirstName, string LastName, string? JerseyNumber, string? Position) : IRequest<PlayerDto>;
