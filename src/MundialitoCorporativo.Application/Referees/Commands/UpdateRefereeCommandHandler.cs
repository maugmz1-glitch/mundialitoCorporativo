using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Referees.Queries;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Referees.Commands;

public class UpdateRefereeCommandHandler : IRequestHandler<UpdateRefereeCommand, Result<RefereeDto>>
{
    private readonly IAppDbContext _db;

    public UpdateRefereeCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<RefereeDto>> Handle(UpdateRefereeCommand request, CancellationToken cancellationToken)
    {
        var referee = await _db.Referees.FindAsync([request.Id], cancellationToken);
        if (referee == null)
            return Result.Failure<RefereeDto>("Árbitro no encontrado.", ErrorCodes.NotFound);
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Result.Failure<RefereeDto>("El nombre es obligatorio.", ErrorCodes.Validation);
        if (string.IsNullOrWhiteSpace(request.LastName))
            return Result.Failure<RefereeDto>("El apellido es obligatorio.", ErrorCodes.Validation);

        referee.FirstName = request.FirstName.Trim();
        referee.LastName = request.LastName.Trim();
        referee.LicenseNumber = request.LicenseNumber?.Trim();
        referee.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(new RefereeDto(referee.Id, referee.FirstName, referee.LastName, referee.LicenseNumber, referee.CreatedAtUtc));
    }
}
