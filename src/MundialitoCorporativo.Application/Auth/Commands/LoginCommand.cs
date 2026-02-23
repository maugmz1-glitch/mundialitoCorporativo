using MediatR;
using MundialitoCorporativo.Application.Auth;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Auth.Commands;

public record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponse>>;
