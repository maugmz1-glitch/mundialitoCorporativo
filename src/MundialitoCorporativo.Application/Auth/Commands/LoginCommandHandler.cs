using MediatR;
using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Application.Auth;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Application.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IAppDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return Result.Failure<LoginResponse>("Usuario y contraseña son obligatorios.", ErrorCodes.Unauthorized);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == request.Username.Trim(), cancellationToken);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
            return Result.Failure<LoginResponse>("Usuario o contraseña incorrectos.", ErrorCodes.Unauthorized);

        var token = _jwtTokenGenerator.GenerateToken(user.UserName);
        return Result.Success(new LoginResponse(token, user.UserName));
    }
}
