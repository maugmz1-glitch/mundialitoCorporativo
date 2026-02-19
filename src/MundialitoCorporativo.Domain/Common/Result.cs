namespace MundialitoCorporativo.Domain.Common;

/// <summary>
/// Patrón Result: resultado de una operación sin usar excepciones para el flujo de negocio.
/// Los handlers devuelven Result.Success(data) o Result.Failure(message, errorCode);
/// la API traduce ErrorCode a HTTP (NotFound→404, Conflict→409, Validation→400).
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string Message { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, T? data, string message, string? errorCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T data) => new(true, data, string.Empty, null);
    public static Result<T> Failure(string message, string? errorCode = null) => new(false, default, message, errorCode);
}

public static class Result
{
    public static Result<T> Success<T>(T data) => Result<T>.Success(data);
    public static Result<T> Failure<T>(string message, string? errorCode = null) => Result<T>.Failure(message, errorCode);
}
