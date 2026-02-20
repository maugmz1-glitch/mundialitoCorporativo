using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Players.Queries;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Application.Players.Commands;

public class CreatePlayerCommandHandler : IRequestHandler<CreatePlayerCommand, Result<PlayerDto>>
{
    private readonly IAppDbContext _db;

    public CreatePlayerCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<PlayerDto>> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Result.Failure<PlayerDto>("First name is required.", ErrorCodes.Validation);
        if (string.IsNullOrWhiteSpace(request.LastName))
            return Result.Failure<PlayerDto>("Last name is required.", ErrorCodes.Validation);

        if (await _db.Teams.FindAsync([request.TeamId], cancellationToken) == null)
            return Result.Failure<PlayerDto>("Team not found.", ErrorCodes.NotFound);

        var player = new Player
        {
            Id = Guid.NewGuid(),
            TeamId = request.TeamId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            JerseyNumber = request.JerseyNumber?.Trim(),
            Position = request.Position?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Players.Add(player);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(new PlayerDto(player.Id, player.TeamId, player.FirstName, player.LastName, player.JerseyNumber, player.Position, player.CreatedAtUtc));
    }
}
