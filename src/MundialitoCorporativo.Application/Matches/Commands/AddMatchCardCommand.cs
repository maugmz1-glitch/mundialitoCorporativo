using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Matches.Queries;

namespace MundialitoCorporativo.Application.Matches.Commands;

public record AddMatchCardCommand(Guid MatchId, Guid PlayerId, int CardType, int Minute) : IRequest<MatchCardDto>;
