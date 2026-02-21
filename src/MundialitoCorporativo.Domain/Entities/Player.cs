namespace MundialitoCorporativo.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JerseyNumber { get; set; }
    public string? Position { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Team Team { get; set; } = null!;
    public ICollection<MatchGoal> GoalsScored { get; set; } = new List<MatchGoal>();
    public ICollection<MatchCard> Cards { get; set; } = new List<MatchCard>();
}
