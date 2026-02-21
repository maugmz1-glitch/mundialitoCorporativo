using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Referees.Commands;

public class DeleteRefereeCommandHandler : IRequestHandler<DeleteRefereeCommand, Result<bool>>
{
    private readonly IAppDbContext _db;

    public DeleteRefereeCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<bool>> Handle(DeleteRefereeCommand request, CancellationToken cancellationToken)
    {
        var referee = await _db.Referees.FindAsync([request.Id], cancellationToken);
        if (referee == null)
            return Result.Failure<bool>("Árbitro no encontrado.", ErrorCodes.NotFound);
        _db.Referees.Remove(referee);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
