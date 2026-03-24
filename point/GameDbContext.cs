using Microsoft.EntityFrameworkCore;

namespace point;

public class GameDbContext : DbContext
{
    public DbSet<GameState> GameStates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = @"Server=localhost;Port=5432;Database=point_game;User Id=postgres;Password=postgres;";
        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GameState>()
            .HasKey(g => g.Id);

        modelBuilder.Entity<GameState>()
            .Property(g => g.DateCreation)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
