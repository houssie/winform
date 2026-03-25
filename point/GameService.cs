using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Drawing; // For Point

namespace point;

public class GameService
{
    public static void InitializeDatabase()
    {
        try
        {
            using var context = new GameDbContext();
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur initialisation base de données:\n{ex.Message}", "Erreur BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void SauvegarderPartie(string nomPartie, bool joueurRougeActif, Plateau plateau, Canon canonGauche, Canon canonDroit, int scoreRouge, int scoreBleu, int puissanceGauche, int puissanceDroit)
    {
        try
        {
            var data = new GameData
            {
                DateSauvegarde = DateTime.UtcNow,
                ScoreRouge = scoreRouge,
                ScoreBleu = scoreBleu,
                PositionCanonGauche = Math.Max(0, Math.Min(canonGauche.PositionLigne, 12)),
                PositionCanonDroit = Math.Max(0, Math.Min(canonDroit.PositionLigne, 12)),
                PuissanceCanonGauche = Math.Max(0, Math.Min(puissanceGauche, 9)),
                PuissanceCanonDroit = Math.Max(0, Math.Min(puissanceDroit, 9)),
                JoueurRougeActif = joueurRougeActif,
                Grille = SerializeGrid(plateau),
                Alignements = plateau.AlignementsAffichage
            };

            var json = JsonSerializer.Serialize(data);

            using (var context = new GameDbContext())
            {
                context.Database.EnsureCreated();
                
                var existing = context.GameStates.Find(nomPartie);
                if (existing != null)
                {
                    existing.StateJson = json;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    context.GameStates.Add(new GameState
                    {
                        Name = nomPartie,
                        StateJson = json,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                context.SaveChanges();
            }

            MessageBox.Show($"Partie sauvegardée : {nomPartie}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur sauvegarde:\n{ex.Message}", "Erreur BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public GameState? ChargerPartie(string name)
    {
        try
        {
            using (var context = new GameDbContext())
            {
                context.Database.EnsureCreated();
                return context.GameStates.Find(name);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur chargement:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    public List<string> ListeParties()
    {
        try
        {
            using (var context = new GameDbContext())
            {
                context.Database.EnsureCreated();
                return context.GameStates
                    .OrderByDescending(g => g.UpdatedAt)
                    .Select(g => g.Name)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur récupération liste:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<string>();
        }
    }

    public void SupprimerPartie(string name)
    {
        try
        {
            using (var context = new GameDbContext())
            {
                context.Database.EnsureCreated();
                var gs = context.GameStates.Find(name);
                if (gs != null)
                {
                    context.GameStates.Remove(gs);
                    context.SaveChanges();
                    MessageBox.Show("Partie supprimée", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur suppression:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public GameData? GetGameData(GameState gameState)
    {
        try 
        {
             return JsonSerializer.Deserialize<GameData>(gameState.StateJson);
        }
        catch 
        {
            return null;
        }
    }

    public void RestaurerGrille(GameData data, Plateau plateau, Joueur joueurA, Joueur joueurB)
    {
        try
        {
            var grille = data.Grille;
            for (int i = 0; i < grille.Count && i <= plateau.Taille; i++)
            {
                for (int j = 0; j < grille[i].Count && j <= plateau.Taille; j++)
                {
                    int cellState = grille[i][j];
                    bool estProtege = cellState >= 10;
                    int joueurCode = cellState % 10;

                    if (joueurCode == 1)
                        plateau.Grille[i, j].Joueur = joueurA;
                    else if (joueurCode == 2)
                        plateau.Grille[i, j].Joueur = joueurB;
                    else
                        plateau.Grille[i, j].Joueur = null;

                    if (estProtege)
                        plateau.Grille[i, j].Proteger();
                    else 
                        plateau.Grille[i, j].EstProtege = false;
                }
            }

            if (data.Alignements != null)
            {
                plateau.AlignementsAffichage = data.Alignements;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur restauration:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private List<List<int>> SerializeGrid(Plateau plateau)
    {
        var grille = new List<List<int>>();
        for (int i = 0; i <= plateau.Taille; i++)
        {
            var ligne = new List<int>();
            for (int j = 0; j <= plateau.Taille; j++)
            {
                int cellState = 0;
                if (plateau.Grille[i, j].Joueur != null)
                {
                    cellState = plateau.Grille[i, j].Joueur.Couleur == Couleur.Rouge ? 1 : 2;
                }
                if (plateau.Grille[i, j].EstProtege)
                    cellState += 10;
                ligne.Add(cellState);
            }
            grille.Add(ligne);
        }
        return grille;
    }
}
