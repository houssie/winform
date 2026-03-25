using Microsoft.EntityFrameworkCore;

namespace point;

public class GameDbContext : DbContext
{
    public DbSet<GameState> GameStates { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Adjust credentials if needed, currently assumes defaults from previous context
        string connectionString = @"Server=localhost;Port=5432;Database=point_game;Username=postgres;Password=postgres;";
        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GameState>()
            .HasKey(g => g.Name);

        modelBuilder.Entity<GameState>()
            .Property(g => g.Name)
            .HasColumnName("name");

        modelBuilder.Entity<GameState>()
            .Property(g => g.StateJson)
            .HasColumnName("state_json")
            .HasColumnType("jsonb");

        modelBuilder.Entity<GameState>()
            .Property(g => g.UpdatedAt)
            .HasColumnName("updated_at");

        modelBuilder.Entity<GameState>()
            .ToTable("pointcanon_games");
    }
}
