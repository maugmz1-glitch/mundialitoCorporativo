using MediatR;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Referees.Queries;

public class GetRefereesQueryHandler : IRequestHandler<GetRefereesQuery, Result<PagedResult<RefereeListItemDto>>>
{
    private readonly IRefereeReadRepository _readRepository;

    public GetRefereesQueryHandler(IRefereeReadRepository readRepository) => _readRepository = readRepository;

    public async Task<Result<PagedResult<RefereeListItemDto>>> Handle(GetRefereesQuery request, CancellationToken cancellationToken)
    {
        var paged = await _readRepository.GetPagedAsync(request, cancellationToken);
        return Result.Success(paged);
    }
}
