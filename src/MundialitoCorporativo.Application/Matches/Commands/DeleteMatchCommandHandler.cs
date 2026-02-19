using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Matches.Commands;

public class DeleteMatchCommandHandler : IRequestHandler<DeleteMatchCommand, Result<bool>>
{
    private readonly IAppDbContext _db;

    public DeleteMatchCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<bool>> Handle(DeleteMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _db.Matches.FindAsync([request.Id], cancellationToken);
        if (match == null)
            return Result.Failure<bool>("Match not found.", ErrorCodes.NotFound);
        _db.Matches.Remove(match);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
