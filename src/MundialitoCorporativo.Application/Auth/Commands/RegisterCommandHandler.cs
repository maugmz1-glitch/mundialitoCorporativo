using MediatR;
using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Domain.Entities;
using MundialitoCorporativo.Application.Auth;

namespace MundialitoCorporativo.Application.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IAppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userName = request.UserName?.Trim();
        var email = request.Email?.Trim();
        var password = request.Password;

        if (string.IsNullOrEmpty(userName) || userName.Length < 3)
            return Result.Failure<RegisterResponse>("El nombre de usuario es obligatorio y debe tener al menos 3 caracteres.", ErrorCodes.Validation);
        if (string.IsNullOrEmpty(password) || password.Length < 6)
            return Result.Failure<RegisterResponse>("La contraseña debe tener al menos 6 caracteres.", ErrorCodes.Validation);

        var exists = await _db.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
        if (exists)
            return Result.Failure<RegisterResponse>("Ya existe un usuario con ese nombre. Elige otro.", ErrorCodes.Conflict);

        if (!string.IsNullOrEmpty(email))
        {
            var emailExists = await _db.Users.AnyAsync(u => u.Email == email, cancellationToken);
            if (emailExists)
                return Result.Failure<RegisterResponse>("Ya existe una cuenta con ese correo.", ErrorCodes.Conflict);
        }

        var (hash, salt) = _passwordHasher.HashPassword(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = string.IsNullOrEmpty(email) ? null : email,
            PasswordHash = hash,
            Salt = salt,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("truncat", StringComparison.OrdinalIgnoreCase))
                return Result.Failure<RegisterResponse>("Error al guardar la cuenta. Actualice la base de datos con las migraciones más recientes (ejecute la aplicación o aplique las migraciones).", ErrorCodes.Validation);
            return Result.Failure<RegisterResponse>("No se pudo crear la cuenta. Intente de nuevo.", ErrorCodes.Conflict);
        }

        return Result.Success(new RegisterResponse(user.UserName, "Cuenta creada. Ya puedes iniciar sesión."));
    }
}
