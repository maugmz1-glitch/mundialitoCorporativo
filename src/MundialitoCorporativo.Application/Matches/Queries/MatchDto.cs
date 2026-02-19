namespace MundialitoCorporativo.Application.Matches.Queries;

public record MatchDto(
    Guid Id, Guid HomeTeamId, Guid AwayTeamId,
    DateTime ScheduledAtUtc, string? Venue, int Status,
    int? HomeScore, int? AwayScore,
    string HomeTeamName, string AwayTeamName,
    DateTime CreatedAtUtc);

public record MatchListItemDto(
    Guid Id, Guid HomeTeamId, Guid AwayTeamId,
    DateTime ScheduledAtUtc, string? Venue, int Status,
    int? HomeScore, int? AwayScore,
    string HomeTeamName, string AwayTeamName,
    DateTime CreatedAtUtc);
