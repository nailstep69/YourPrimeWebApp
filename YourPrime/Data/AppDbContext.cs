using Microsoft.EntityFrameworkCore;
using YourPrime.Entities;

namespace YourPrime.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<UserTeam> UserTeams { get; set; }
    
    
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<TournamentTeam> TournamentTeams { get; set; }
    public DbSet<TournamentMatch> TournamentMatches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserTeam>()
            .HasKey(ut => new { ut.UserId, ut.TeamId });

        modelBuilder.Entity<UserTeam>()
            .HasOne(ut => ut.User)
            .WithMany(u => u.UserTeams)
            .HasForeignKey(ut => ut.UserId);

        modelBuilder.Entity<UserTeam>()
            .HasOne(ut => ut.Team)
            .WithMany(t => t.UserTeams)
            .HasForeignKey(ut => ut.TeamId);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Captain)
            .WithMany()
            .HasForeignKey(t => t.CaptainId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        
        
        // ТУРНИР
        
        
        modelBuilder.Entity<TournamentTeam>()
            .HasKey(tt => new { tt.TournamentId, tt.TeamId });

        modelBuilder.Entity<TournamentTeam>()
            .HasOne(tt => tt.Tournament)
            .WithMany(t => t.TournamentTeams)
            .HasForeignKey(tt => tt.TournamentId);

        modelBuilder.Entity<TournamentTeam>()
            .HasOne(tt => tt.Team)
            .WithMany()
            .HasForeignKey(tt => tt.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TournamentTeam>()
            .HasOne(tt => tt.Captain)
            .WithMany()
            .HasForeignKey(tt => tt.CaptainId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TournamentMatch>()
            .HasOne(m => m.Tournament)
            .WithMany(t => t.Matches)
            .HasForeignKey(m => m.TournamentId);

        modelBuilder.Entity<TournamentMatch>()
            .HasOne(m => m.TeamA)
            .WithMany()
            .HasForeignKey(m => m.TeamAId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TournamentMatch>()
            .HasOne(m => m.TeamB)
            .WithMany()
            .HasForeignKey(m => m.TeamBId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TournamentMatch>()
            .HasOne(m => m.WinnerTeam)
            .WithMany()
            .HasForeignKey(m => m.WinnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}