using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Application.Referees.Queries;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Application.Referees.Commands;

public class CreateRefereeCommandHandler : IRequestHandler<CreateRefereeCommand, Result<RefereeDto>>
{
    private readonly IAppDbContext _db;

    public CreateRefereeCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<RefereeDto>> Handle(CreateRefereeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Result.Failure<RefereeDto>("El nombre es obligatorio.", ErrorCodes.Validation);
        if (string.IsNullOrWhiteSpace(request.LastName))
            return Result.Failure<RefereeDto>("El apellido es obligatorio.", ErrorCodes.Validation);

        var referee = new Referee
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            LicenseNumber = request.LicenseNumber?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Referees.Add(referee);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(new RefereeDto(referee.Id, referee.FirstName, referee.LastName, referee.LicenseNumber, referee.CreatedAtUtc));
    }
}
