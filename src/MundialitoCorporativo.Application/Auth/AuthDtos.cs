namespace MundialitoCorporativo.Application.Auth;

public record RegisterResponse(string UserName, string Message);
public record LoginResponse(string Token, string UserName);
public record MeResponse(string UserName);
