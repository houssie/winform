using System.Text.Json;

namespace point;

public class GameService
{
    private readonly GameDbContext _context;

    public GameService()
    {
        _context = new GameDbContext();
    }

    public static void InitializeDatabase()
    {
        try
        {
            using var context = new GameDbContext();
            context.EnsureDatabaseCreated();
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
            var etatJson = SérialiserGrille(plateau) ?? "[]";

            var gameState = new GameState
            {
                NomPartie = nomPartie ?? "Sans nom",
                DateSauvegarde = DateTime.UtcNow,
                EtatGrille = etatJson,
                ScoreRouge = scoreRouge,
                ScoreBleu = scoreBleu,
                PositionCanonGauche = Math.Max(0, Math.Min(canonGauche.PositionLigne, 12)),
                PositionCanonDroit = Math.Max(0, Math.Min(canonDroit.PositionLigne, 12)),
                PuissanceCanonGauche = Math.Max(0, Math.Min(puissanceGauche, 9)),
                PuissanceCanonDroit = Math.Max(0, Math.Min(puissanceDroit, 9))
            };

            using (var context = new GameDbContext())
            {
                // Créer la table si elle n'existe pas
                context.Database.EnsureCreated();
                
                context.GameStates.Add(gameState);
                context.SaveChanges();
            }

            MessageBox.Show($"Partie sauvegardée : {nomPartie}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            string errorMsg = $"Erreur sauvegarde:\n{ex.Message}";
            if (ex.InnerException != null)
                errorMsg += $"\n\nDétails:\n{ex.InnerException.Message}";
            if (ex.InnerException?.InnerException != null)
                errorMsg += $"\n\nSous-détails:\n{ex.InnerException.InnerException.Message}";
            MessageBox.Show(errorMsg, "Erreur BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public GameState? ChargerPartie(int id)
    {
        try
        {
            using (var context = new GameDbContext())
            {
                // Créer la table si elle n'existe pas
                context.Database.EnsureCreated();
                
                return context.GameStates.FirstOrDefault(g => g.Id == id);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur chargement:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    public List<GameState> ListeParties()
    {
        try
        {
            using (var context = new GameDbContext())
            {
                // Créer la table si elle n'existe pas
                context.Database.EnsureCreated();
                
                return context.GameStates
                    .OrderByDescending(g => g.DateSauvegarde)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur récupération liste:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<GameState>();
        }
    }

    public void SupprimerPartie(int id)
    {
        try
        {
            using (var context = new GameDbContext())
            {
                // Créer la table si elle n'existe pas
                context.Database.EnsureCreated();
                
                var gameState = context.GameStates.Find(id);
                if (gameState != null)
                {
                    context.GameStates.Remove(gameState);
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

    public void RestaurerGrille(GameState gameState, Plateau plateau)
    {
        try
        {
            if (string.IsNullOrEmpty(gameState.EtatGrille)) return;

            var grille = JsonSerializer.Deserialize<List<List<int>>>(gameState.EtatGrille);
            if (grille == null) return;

            // Restaurer l'état de la grille
            for (int i = 0; i < grille.Count && i <= plateau.Taille; i++)
            {
                for (int j = 0; j < grille[i].Count && j <= plateau.Taille; j++)
                {
                    int cellState = grille[i][j];
                    
                    // 0 = vide, 1 = rouge, 2 = bleu, protégé codé en 10+
                    bool estProtege = cellState >= 10;
                    int joueurCode = cellState % 10;

                    if (joueurCode == 1)
                        plateau.Grille[i, j].Joueur = new Joueur("Joueur A", Couleur.Rouge);
                    else if (joueurCode == 2)
                        plateau.Grille[i, j].Joueur = new Joueur("Joueur B", Couleur.Bleu);

                    if (estProtege)
                        plateau.Grille[i, j].Proteger();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur restauration grille:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string SérialiserGrille(Plateau plateau)
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

        return JsonSerializer.Serialize(grille);
    }
}
