namespace MundialitoCorporativo.Domain.Entities;

public class MatchCard : MundialitoCorporativo.Domain.Common.IAuditable
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid PlayerId { get; set; }
    public CardType CardType { get; set; }
    public int Minute { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public Match Match { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

public enum CardType
{
    Yellow = 0,
    Red = 1
}
