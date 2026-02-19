using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Common;

public interface IRequest<TResponse> : MediatR.IRequest<Result<TResponse>> { }
