using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchGoal> MatchGoals => Set<MatchGoal>();
    public DbSet<MatchCard> MatchCards => Set<MatchCard>();
    public DbSet<Referee> Referees => Set<Referee>();
    public DbSet<User> Users => Set<User>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.LogoUrl).HasMaxLength(500);
        });
        modelBuilder.Entity<Player>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.JerseyNumber).HasMaxLength(10);
            e.Property(x => x.Position).HasMaxLength(50);
            e.HasOne(x => x.Team).WithMany(x => x.Players).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Match>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Venue).HasMaxLength(200);
            e.HasOne(x => x.HomeTeam).WithMany(x => x.HomeMatches).HasForeignKey(x => x.HomeTeamId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AwayTeam).WithMany(x => x.AwayMatches).HasForeignKey(x => x.AwayTeamId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Referee).WithMany(x => x.Matches).HasForeignKey(x => x.RefereeId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<MatchCard>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Match).WithMany(x => x.Cards).HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Player).WithMany(x => x.Cards).HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MatchGoal>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Match).WithMany(x => x.Goals).HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Scorer).WithMany(x => x.GoalsScored).HasForeignKey(x => x.ScorerId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Referee>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LicenseNumber).HasMaxLength(50);
        });
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.PasswordHash).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.Salt).HasColumnType("nvarchar(max)").IsRequired();
            e.HasIndex(x => x.UserName).IsUnique();
        });
        modelBuilder.Entity<IdempotencyRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            e.Property(x => x.RequestMethod).HasMaxLength(10).IsRequired();
            e.Property(x => x.RequestPath).HasMaxLength(500).IsRequired();
            e.Property(x => x.ResponseBody).HasMaxLength(8000);
            e.HasIndex(x => new { x.IdempotencyKey, x.RequestMethod, x.RequestPath }).IsUnique();
        });
    }
}
