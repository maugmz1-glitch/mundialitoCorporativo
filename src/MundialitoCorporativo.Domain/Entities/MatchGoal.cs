namespace MundialitoCorporativo.Domain.Entities;

public class MatchGoal : MundialitoCorporativo.Domain.Common.IAuditable
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid ScorerId { get; set; }
    public int Minute { get; set; }
    public bool IsOwnGoal { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public Match Match { get; set; } = null!;
    public Player Scorer { get; set; } = null!;
}
