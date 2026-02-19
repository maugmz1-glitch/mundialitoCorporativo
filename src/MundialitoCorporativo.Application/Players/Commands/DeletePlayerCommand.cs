using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Players.Commands;

public record DeletePlayerCommand(Guid Id) : IRequest<bool>;
