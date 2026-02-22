namespace MundialitoCorporativo.Application.Interfaces;

/// <summary>
/// Hash y verificación de contraseñas (PBKDF2). Implementado en Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    (string Hash, string Salt) HashPassword(string password);
    bool VerifyPassword(string password, string storedHash, string storedSalt);
}
