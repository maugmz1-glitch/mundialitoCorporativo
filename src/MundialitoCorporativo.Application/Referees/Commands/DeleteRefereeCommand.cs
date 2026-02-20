using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Referees.Commands;

public record DeleteRefereeCommand(Guid Id) : IRequest<bool>;
