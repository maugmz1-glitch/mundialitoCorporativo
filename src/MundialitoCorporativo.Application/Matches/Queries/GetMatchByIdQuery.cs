using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Application.Matches.Queries;

public record GetMatchByIdQuery(Guid Id) : IRequest<MatchDto?>;
