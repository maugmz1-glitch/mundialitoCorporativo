namespace MundialitoCorporativo.Application.Interfaces;

/// <summary>
/// Generación de JWT para autenticación. Implementado en Api (usa IConfiguration y paquetes JWT).
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(string userName);
}
