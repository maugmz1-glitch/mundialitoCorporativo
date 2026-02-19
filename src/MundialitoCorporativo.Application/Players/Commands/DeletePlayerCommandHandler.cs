using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Players.Commands;

public class DeletePlayerCommandHandler : IRequestHandler<DeletePlayerCommand, Result<bool>>
{
    private readonly IAppDbContext _db;

    public DeletePlayerCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<bool>> Handle(DeletePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await _db.Players.FindAsync([request.Id], cancellationToken);
        if (player == null)
            return Result.Failure<bool>("Player not found.", ErrorCodes.NotFound);
        _db.Players.Remove(player);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
