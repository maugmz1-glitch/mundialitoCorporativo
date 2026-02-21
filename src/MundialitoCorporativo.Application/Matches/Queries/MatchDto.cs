namespace MundialitoCorporativo.Application.Matches.Queries;

public record MatchCardDto(Guid Id, Guid PlayerId, string PlayerName, int CardType, int Minute);

public record MatchDto(
    Guid Id, Guid HomeTeamId, Guid AwayTeamId, Guid? RefereeId, string? RefereeName,
    DateTime ScheduledAtUtc, string? Venue, int Status,
    int? HomeScore, int? AwayScore,
    string HomeTeamName, string AwayTeamName,
    IReadOnlyList<MatchCardDto> Cards,
    DateTime CreatedAtUtc);

public record MatchListItemDto(
    Guid Id, Guid HomeTeamId, Guid AwayTeamId, Guid? RefereeId, string? RefereeName,
    DateTime ScheduledAtUtc, string? Venue, int Status,
    int? HomeScore, int? AwayScore,
    string HomeTeamName, string AwayTeamName,
    DateTime CreatedAtUtc);
