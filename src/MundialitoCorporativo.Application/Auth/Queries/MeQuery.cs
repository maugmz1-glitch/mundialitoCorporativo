using MediatR;
using MundialitoCorporativo.Application.Auth;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Auth.Queries;

public record MeQuery(string? UserName) : IRequest<Result<MeResponse?>>;
