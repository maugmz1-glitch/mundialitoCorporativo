namespace MundialitoCorporativo.Application.Standings.Queries;

public record StandingRowDto(
    int Rank,
    Guid TeamId,
    string TeamName,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifferential,
    int Points,
    int YellowCards,
    int RedCards);
