using MundialitoCorporativo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MundialitoCorporativo.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Team> Teams { get; }
    DbSet<Player> Players { get; }
    DbSet<Match> Matches { get; }
    DbSet<MatchGoal> MatchGoals { get; }
    DbSet<MatchCard> MatchCards { get; }
    DbSet<Referee> Referees { get; }
    DbSet<User> Users { get; }
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
