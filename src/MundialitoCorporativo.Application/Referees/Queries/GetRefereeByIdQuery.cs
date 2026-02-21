using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Referees.Queries;

public record GetRefereeByIdQuery(Guid Id) : IRequest<RefereeDto?>;
