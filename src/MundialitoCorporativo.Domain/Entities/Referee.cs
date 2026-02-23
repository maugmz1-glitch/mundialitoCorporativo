namespace MundialitoCorporativo.Domain.Entities;

public class Referee : MundialitoCorporativo.Domain.Common.IAuditable
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
