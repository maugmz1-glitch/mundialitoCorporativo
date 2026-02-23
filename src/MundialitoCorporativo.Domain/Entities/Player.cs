namespace MundialitoCorporativo.Domain.Entities;

public class Player : MundialitoCorporativo.Domain.Common.IAuditable
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JerseyNumber { get; set; }
    public string? Position { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public Team Team { get; set; } = null!;
    public ICollection<MatchGoal> GoalsScored { get; set; } = new List<MatchGoal>();
    public ICollection<MatchCard> Cards { get; set; } = new List<MatchCard>();
}
