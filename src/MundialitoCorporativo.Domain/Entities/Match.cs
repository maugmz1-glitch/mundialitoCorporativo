namespace MundialitoCorporativo.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public string? Venue { get; set; }
    public MatchStatus Status { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
    public ICollection<MatchGoal> Goals { get; set; } = new List<MatchGoal>();
}

public enum MatchStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Postponed = 3,
    Cancelled = 4
}
