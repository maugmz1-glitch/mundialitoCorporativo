using MediatR;
using MundialitoCorporativo.Application.Auth;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Auth.Commands;

public record RegisterCommand(string UserName, string? Email, string Password) : IRequest<Result<RegisterResponse>>;
