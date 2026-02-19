using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Players.Queries;

public record GetPlayerByIdQuery(Guid Id) : IRequest<PlayerDto?>;
