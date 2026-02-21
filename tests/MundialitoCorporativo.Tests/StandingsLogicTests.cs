using Xunit;
using MundialitoCorporativo.Application.Standings.Queries;
using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Tests;

/// <summary>
/// Lógica de tabla de posiciones: puntos (3/1/0), diferencia de gol, ordenamiento.
/// </summary>
public class StandingsLogicTests
{
    [Fact]
    public void Points_Win3_Draw1_Loss0()
    {
        int wins = 2;
        int draws = 1;
        int losses = 0;
        int points = wins * 3 + draws * 1 + losses * 0;
        Assert.Equal(7, points);
    }

    [Theory]
    [InlineData(2, 0, 0, 6)]
    [InlineData(1, 1, 0, 4)]
    [InlineData(0, 2, 1, 2)]
    [InlineData(0, 0, 3, 0)]
    public void Points_Calculation(int won, int drawn, int lost, int expectedPoints)
    {
        int points = won * 3 + drawn * 1 + lost * 0;
        Assert.Equal(expectedPoints, points);
    }

    [Fact]
    public void GoalDifferential_GoalsForMinusGoalsAgainst()
    {
        int gf = 10;
        int ga = 4;
        int gd = gf - ga;
        Assert.Equal(6, gd);
    }

    [Theory]
    [InlineData(5, 2, 3)]
    [InlineData(0, 3, -3)]
    [InlineData(4, 4, 0)]
    public void GoalDifferential_Values(int goalsFor, int goalsAgainst, int expectedGd)
    {
        Assert.Equal(expectedGd, goalsFor - goalsAgainst);
    }

    [Fact]
    public void Ordering_ByPointsThenGoalDifferentialThenGoalsFor()
    {
        var rows = new[]
        {
            new StandingRowDto(1, Guid.NewGuid(), "A", 3, 1, 0, 2, 5, 3, 2, 3, 0, 0),
            new StandingRowDto(2, Guid.NewGuid(), "B", 3, 1, 1, 1, 4, 4, 0, 4, 0, 0),
            new StandingRowDto(3, Guid.NewGuid(), "C", 3, 2, 0, 1, 6, 2, 4, 6, 0, 0),
        };
        var ordered = rows.OrderByDescending(x => x.Points)
            .ThenByDescending(x => x.GoalDifferential)
            .ThenByDescending(x => x.GoalsFor)
            .ToList();
        Assert.Equal(6, ordered[0].Points);
        Assert.Equal(4, ordered[0].GoalDifferential);
        Assert.Equal(6, ordered[0].GoalsFor);
        Assert.Equal("C", ordered[0].TeamName);
        Assert.Equal(4, ordered[1].Points);
        Assert.Equal(3, ordered[2].Points);
    }

    [Fact]
    public void Ordering_TieBreak_SamePoints_ThenGoalDifferential()
    {
        var teamA = new StandingRowDto(1, Guid.NewGuid(), "A", 2, 1, 0, 1, 4, 2, 2, 3, 0, 0);
        var teamB = new StandingRowDto(2, Guid.NewGuid(), "B", 2, 1, 0, 1, 3, 1, 2, 3, 0, 0);
        var ordered = new[] { teamB, teamA }
            .OrderByDescending(x => x.Points)
            .ThenByDescending(x => x.GoalDifferential)
            .ThenByDescending(x => x.GoalsFor)
            .ToList();
        Assert.Equal(teamA.TeamId, ordered[0].TeamId);
        Assert.Equal(2, ordered[0].GoalDifferential);
        Assert.Equal(4, ordered[0].GoalsFor);
    }

    [Fact]
    public void PagedResult_TotalPages_RoundsUp()
    {
        int totalRecords = 25;
        int pageSize = 10;
        int expectedPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        Assert.Equal(3, expectedPages);
    }
}
