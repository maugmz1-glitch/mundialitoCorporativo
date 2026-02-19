namespace MundialitoCorporativo.Application.Players.Queries;

public record PlayerDto(Guid Id, Guid TeamId, string FirstName, string LastName, string? JerseyNumber, string? Position, DateTime CreatedAtUtc);
public record PlayerListItemDto(Guid Id, Guid TeamId, string FirstName, string LastName, string? JerseyNumber, string? Position, string TeamName, DateTime CreatedAtUtc);
