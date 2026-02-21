using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Matches.Queries;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Application.Matches.Commands;

public class AddMatchCardCommandHandler : IRequestHandler<AddMatchCardCommand, Result<MatchCardDto>>
{
    private readonly IAppDbContext _db;

    public AddMatchCardCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<MatchCardDto>> Handle(AddMatchCardCommand request, CancellationToken cancellationToken)
    {
        var match = await _db.Matches.FindAsync([request.MatchId], cancellationToken);
        if (match == null)
            return Result.Failure<MatchCardDto>("Partido no encontrado.", ErrorCodes.NotFound);
        var player = await _db.Players.FindAsync([request.PlayerId], cancellationToken);
        if (player == null)
            return Result.Failure<MatchCardDto>("Jugador no encontrado.", ErrorCodes.NotFound);
        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
            return Result.Failure<MatchCardDto>("El jugador no pertenece a ninguno de los equipos del partido.", ErrorCodes.Validation);
        if (request.CardType != (int)CardType.Yellow && request.CardType != (int)CardType.Red)
            return Result.Failure<MatchCardDto>("Tipo de tarjeta inválido (0=Amarilla, 1=Roja).", ErrorCodes.Validation);
        if (request.Minute < 0 || request.Minute > 999)
            return Result.Failure<MatchCardDto>("El minuto debe estar entre 0 y 999.", ErrorCodes.Validation);

        var card = new MatchCard
        {
            Id = Guid.NewGuid(),
            MatchId = request.MatchId,
            PlayerId = request.PlayerId,
            CardType = (CardType)request.CardType,
            Minute = request.Minute,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.MatchCards.Add(card);
        await _db.SaveChangesAsync(cancellationToken);

        var playerName = $"{player.FirstName} {player.LastName}";
        return Result.Success(new MatchCardDto(card.Id, card.PlayerId, playerName, request.CardType, card.Minute));
    }
}
