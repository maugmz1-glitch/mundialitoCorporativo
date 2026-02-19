namespace MundialitoCorporativo.Application.Teams.Queries;

public record TeamDto(Guid Id, string Name, string? LogoUrl, DateTime CreatedAtUtc);
public record TeamListItemDto(Guid Id, string Name, string? LogoUrl, DateTime CreatedAtUtc);
