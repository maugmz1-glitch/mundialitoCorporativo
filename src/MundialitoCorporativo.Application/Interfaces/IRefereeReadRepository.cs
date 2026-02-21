using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Referees.Queries;

namespace MundialitoCorporativo.Application.Interfaces;

public interface IRefereeReadRepository
{
    Task<RefereeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<RefereeListItemDto>> GetPagedAsync(GetRefereesQuery query, CancellationToken cancellationToken = default);
}
