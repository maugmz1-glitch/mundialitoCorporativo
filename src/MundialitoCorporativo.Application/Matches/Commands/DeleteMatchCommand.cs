using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Matches.Commands;

public record DeleteMatchCommand(Guid Id) : IRequest<bool>;
