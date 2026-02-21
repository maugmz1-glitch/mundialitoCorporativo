namespace MundialitoCorporativo.Application.Referees.Queries;

public record RefereeDto(Guid Id, string FirstName, string LastName, string? LicenseNumber, DateTime CreatedAtUtc);
public record RefereeListItemDto(Guid Id, string FirstName, string LastName, string? LicenseNumber, DateTime CreatedAtUtc);
