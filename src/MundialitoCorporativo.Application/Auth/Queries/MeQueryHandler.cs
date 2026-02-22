using MediatR;
using MundialitoCorporativo.Application.Auth;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Auth.Queries;

public class MeQueryHandler : IRequestHandler<MeQuery, Result<MeResponse?>>
{
    public Task<Result<MeResponse?>> Handle(MeQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserName))
            return Task.FromResult(Result.Failure<MeResponse?>("No autorizado.", ErrorCodes.Unauthorized));
        return Task.FromResult(Result.Success<MeResponse?>(new MeResponse(request.UserName)));
    }
}
