using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Players.Queries;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Players.Commands;

public class UpdatePlayerCommandHandler : IRequestHandler<UpdatePlayerCommand, Result<PlayerDto>>
{
    private readonly IAppDbContext _db;

    public UpdatePlayerCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<PlayerDto>> Handle(UpdatePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await _db.Players.FindAsync([request.Id], cancellationToken);
        if (player == null)
            return Result.Failure<PlayerDto>("Player not found.", ErrorCodes.NotFound);
        if (await _db.Teams.FindAsync([request.TeamId], cancellationToken) == null)
            return Result.Failure<PlayerDto>("Team not found.", ErrorCodes.NotFound);
        player.TeamId = request.TeamId;
        player.FirstName = request.FirstName.Trim();
        player.LastName = request.LastName.Trim();
        player.JerseyNumber = request.JerseyNumber?.Trim();
        player.Position = request.Position?.Trim();
        player.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(new PlayerDto(player.Id, player.TeamId, player.FirstName, player.LastName, player.JerseyNumber, player.Position, player.CreatedAtUtc));
    }
}
