using Microsoft.AspNetCore.Mvc;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Domain.Common;

namespace MundialitoCorporativo.Api;

/// <summary>
/// Mapeo consistente Result → HTTP: 400 Bad Request, 404 Not Found, 409 Conflict.
/// Todos los controladores usan este método para no usar excepciones en el flujo de negocio.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Usar solo cuando result.IsSuccess es false. Devuelve 400, 404 o 409 según result.ErrorCode.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.ErrorCode switch
        {
            ErrorCodes.NotFound => new NotFoundObjectResult(new { message = result.Message, code = result.ErrorCode }),
            ErrorCodes.Conflict or ErrorCodes.Duplicate => new ConflictObjectResult(new { message = result.Message, code = result.ErrorCode }),
            ErrorCodes.Unauthorized => new UnauthorizedObjectResult(new { message = result.Message, code = result.ErrorCode }),
            _ => new BadRequestObjectResult(new { message = result.Message, code = result.ErrorCode })
        };
    }
}
