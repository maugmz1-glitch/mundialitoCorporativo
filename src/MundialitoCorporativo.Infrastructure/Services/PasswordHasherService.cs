using System.Security.Cryptography;
using MundialitoCorporativo.Application.Interfaces;

namespace MundialitoCorporativo.Infrastructure.Services;

/// <summary>
/// Hash de contraseñas con PBKDF2 (salt aleatorio por usuario). Implementación de IPasswordHasher.
/// </summary>
public sealed class PasswordHasherService : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public (string Hash, string Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, HashSize, HashAlgorithmName.SHA256, Iterations);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var salt = Convert.FromBase64String(storedSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, HashSize, HashAlgorithmName.SHA256, Iterations);
        return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(storedHash));
    }
}
