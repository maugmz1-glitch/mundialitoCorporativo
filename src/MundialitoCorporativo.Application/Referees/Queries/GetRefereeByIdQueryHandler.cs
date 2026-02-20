using MediatR;
using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Referees.Queries;

public class GetRefereeByIdQueryHandler : IRequestHandler<GetRefereeByIdQuery, Result<RefereeDto?>>
{
    private readonly IAppDbContext _db;

    public GetRefereeByIdQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<RefereeDto?>> Handle(GetRefereeByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _db.Referees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (r == null) return Result.Success<RefereeDto?>(null);
        return Result.Success(new RefereeDto(r.Id, r.FirstName, r.LastName, r.LicenseNumber, r.CreatedAtUtc));
    }
}
