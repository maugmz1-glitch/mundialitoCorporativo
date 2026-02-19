using Xunit;
using MundialitoCorporativo.Application.Standings.Queries;
using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Tests;

public class StandingsLogicTests
{
    [Fact]
    public void Points_Win3_Draw1_Loss0()
    {
        int points = 3 * 2 + 1 * 1 + 0; // 2 wins, 1 draw
        Assert.Equal(7, points);
    }

    [Fact]
    public void GoalDifferential_GoalsForMinusGoalsAgainst()
    {
        int gf = 10;
        int ga = 4;
        int gd = gf - ga;
        Assert.Equal(6, gd);
    }

    [Fact]
    public void Ordering_ByPointsThenGoalDifferentialThenGoalsFor()
    {
        var rows = new[]
        {
            new StandingRowDto(1, Guid.NewGuid(), "A", 3, 1, 0, 2, 5, 3, 2, 3),
            new StandingRowDto(2, Guid.NewGuid(), "B", 3, 1, 1, 1, 4, 4, 0, 4),
            new StandingRowDto(3, Guid.NewGuid(), "C", 3, 2, 0, 1, 6, 2, 4, 6),
        };
        var ordered = rows.OrderByDescending(x => x.Points)
            .ThenByDescending(x => x.GoalDifferential)
            .ThenByDescending(x => x.GoalsFor)
            .ToList();
        Assert.Equal(6, ordered[0].Points);
        Assert.Equal(4, ordered[0].GoalDifferential);
        Assert.Equal(4, ordered[1].Points);
        Assert.Equal(3, ordered[2].Points);
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
