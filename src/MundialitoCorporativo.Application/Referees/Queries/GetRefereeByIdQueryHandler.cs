using MediatR;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Referees.Queries;

public class GetRefereeByIdQueryHandler : IRequestHandler<GetRefereeByIdQuery, Result<RefereeDto?>>
{
    private readonly IRefereeReadRepository _readRepository;

    public GetRefereeByIdQueryHandler(IRefereeReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<RefereeDto?>> Handle(GetRefereeByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _readRepository.GetByIdAsync(request.Id, cancellationToken);
        return Result.Success<RefereeDto?>(r);
    }
}
