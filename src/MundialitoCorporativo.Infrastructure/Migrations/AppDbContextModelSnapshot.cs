using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MundialitoCorporativo.Infrastructure.Persistence;

#nullable disable

namespace MundialitoCorporativo.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.IdempotencyRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");
                    b.Property<string>("IdempotencyKey")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");
                    b.Property<string>("RequestMethod")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");
                    b.Property<string>("RequestPath")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");
                    b.Property<int>("ResponseStatusCode")
                        .HasColumnType("int");
                    b.Property<string>("ResponseBody")
                        .HasMaxLength(8000)
                        .HasColumnType("nvarchar(8000)");
                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");
                    b.HasKey("Id");
                    b.HasIndex("IdempotencyKey", "RequestMethod", "RequestPath")
                        .IsUnique();
                    b.ToTable("IdempotencyRecords");
                });

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.Match", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");
                    b.Property<Guid>("AwayTeamId")
                        .HasColumnType("uniqueidentifier");
                    b.Property<int?>("AwayScore")
                        .HasColumnType("int");
                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");
                    b.Property<Guid>("HomeTeamId")
                        .HasColumnType("uniqueidentifier");
                    b.Property<int?>("HomeScore")
                        .HasColumnType("int");
                    b.Property<DateTime>("ScheduledAtUtc")
                        .HasColumnType("datetime2");
                    b.Property<int>("Status")
                        .HasColumnType("int");
                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("datetime2");
                    b.Property<string>("Venue")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");
                    b.HasKey("Id");
                    b.HasIndex("AwayTeamId");
                    b.HasIndex("HomeTeamId");
                    b.ToTable("Matches");
                });

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.MatchGoal", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");
                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");
                    b.Property<bool>("IsOwnGoal")
                        .HasColumnType("bit");
                    b.Property<Guid>("MatchId")
                        .HasColumnType("uniqueidentifier");
                    b.Property<int>("Minute")
                        .HasColumnType("int");
                    b.Property<Guid>("ScorerId")
                        .HasColumnType("uniqueidentifier");
                    b.HasKey("Id");
                    b.HasIndex("MatchId");
                    b.HasIndex("ScorerId");
                    b.ToTable("MatchGoals");
                });

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");
                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");
                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");
                    b.Property<string>("JerseyNumber")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");
                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");
                    b.Property<string>("Position")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");
                    b.Property<Guid>("TeamId")
                        .HasColumnType("uniqueidentifier");
                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("datetime2");
                    b.HasKey("Id");
                    b.HasIndex("TeamId");
                    b.ToTable("Players");
                });

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.Team", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");
                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");
                    b.Property<string>("LogoUrl")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");
                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");
                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("datetime2");
                    b.HasKey("Id");
                    b.ToTable("Teams");
                });

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.Match", b =>
                {
                    b.HasOne("MundialitoCorporativo.Domain.Entities.Team", "AwayTeam")
                        .WithMany("AwayMatches")
                        .HasForeignKey("AwayTeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                    b.HasOne("MundialitoCorporativo.Domain.Entities.Team", "HomeTeam")
                        .WithMany("HomeMatches")
                        .HasForeignKey("HomeTeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                    b.Navigation("AwayTeam");
                    b.Navigation("HomeTeam");
                });

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.MatchGoal", b =>
                {
                    b.HasOne("MundialitoCorporativo.Domain.Entities.Match", "Match")
                        .WithMany("Goals")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("MundialitoCorporativo.Domain.Entities.Player", "Scorer")
                        .WithMany("GoalsScored")
                        .HasForeignKey("ScorerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                    b.Navigation("Match");
                    b.Navigation("Scorer");
                });

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.Player", b =>
                {
                    b.HasOne("MundialitoCorporativo.Domain.Entities.Team", "Team")
                        .WithMany("Players")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                    b.Navigation("Team");
                });

            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.Match", b => { b.Navigation("Goals"); });
            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.Player", b => { b.Navigation("GoalsScored"); });
            modelBuilder.Entity("MundialitoCorporativo.Domain.Entities.Team", b =>
                {
                    b.Navigation("AwayMatches");
                    b.Navigation("HomeMatches");
                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
