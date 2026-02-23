namespace MundialitoCorporativo.Domain.Entities;

public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestMethod { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public int ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ResponseContentType { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
