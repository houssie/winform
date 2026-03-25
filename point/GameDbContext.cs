using Microsoft.EntityFrameworkCore;

namespace point;

public class GameDbContext : DbContext
{
    public DbSet<GameState> GameStates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = @"Server=localhost;Port=5432;Database=point_game;Username=postgres;Password=postgres;";
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

        modelBuilder.Entity<GameState>()
            .Property(g => g.Id)
            .UseIdentityColumn();
    }

    public void EnsureDatabaseCreated()
    {
        try
        {
            // Créer la base de données et les tables si elles n'existent pas
            Database.Migrate();
        }
        catch
        {
            // Si migration échoue, essayer EnsureCreated comme fallback
            try
            {
                Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw new Exception($"Impossible de créer la base de données: {ex.Message}");
            }
        }
    }
}
