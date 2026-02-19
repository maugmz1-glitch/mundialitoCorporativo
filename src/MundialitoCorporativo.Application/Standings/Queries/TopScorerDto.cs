namespace MundialitoCorporativo.Application.Standings.Queries;

public record TopScorerDto(Guid PlayerId, string PlayerName, string TeamName, int Goals);
