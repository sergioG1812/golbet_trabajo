using GolBet.Entities;
using GolBet.Entities.Common;
using Microsoft.EntityFrameworkCore;
namespace GolBet.Repositories.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Bet> Bets => Set<Bet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Team>().Property(t => t.Name).UseCollation("SQL_Latin1_General_CP1_CI_AI");
        modelBuilder.Entity<Team>().HasIndex(t => t.Name).IsUnique();

        modelBuilder.Entity<Match>().HasOne(m => m.HomeTeam).WithMany()
            .HasForeignKey(m => m.HomeTeamId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Match>().HasOne(m => m.AwayTeam).WithMany()
            .HasForeignKey(m => m.AwayTeamId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bet>().HasOne(b => b.Match).WithMany(m => m.Bets)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedDate = utcNow;
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedDate = utcNow;
                entry.Property(e => e.CreatedDate).IsModified = false;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}