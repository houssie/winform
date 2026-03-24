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
        using var context = new GameDbContext();
        try
        {
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur connexion PostgreSQL:\n{ex.Message}", "Erreur BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void SauvegarderPartie(string nomPartie, bool joueurRougeActif, Plateau plateau, Canon canonGauche, Canon canonDroit, int scoreRouge, int scoreBleu, int puissanceGauche, int puissanceDroit)
    {
        try
        {
            var etatJson = SérialiserGrille(plateau);

            var gameState = new GameState
            {
                NomPartie = nomPartie,
                DateSauvegarde = DateTime.Now,
                EtatGrille = etatJson,
                ScoreRouge = scoreRouge,
                ScoreBleu = scoreBleu,
                PositionCanonGauche = canonGauche.PositionLigne,
                PositionCanonDroit = canonDroit.PositionLigne,
                PuissanceCanonGauche = puissanceGauche,
                PuissanceCanonDroit = puissanceDroit
            };

            _context.GameStates.Add(gameState);
            _context.SaveChanges();

            MessageBox.Show($"Partie sauvegardée : {nomPartie}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur sauvegarde:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public GameState? ChargerPartie(int id)
    {
        try
        {
            return _context.GameStates.FirstOrDefault(g => g.Id == id);
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
            return _context.GameStates
                .OrderByDescending(g => g.DateSauvegarde)
                .ToList();
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
            var gameState = _context.GameStates.Find(id);
            if (gameState != null)
            {
                _context.GameStates.Remove(gameState);
                _context.SaveChanges();
                MessageBox.Show("Partie supprimée", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        plateau.Grille[i, j].Joueur = new Joueur { Couleur = "Rouge" };
                    else if (joueurCode == 2)
                        plateau.Grille[i, j].Joueur = new Joueur { Couleur = "Bleu" };

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
                    cellState = plateau.Grille[i, j].Joueur.Couleur == "Rouge" ? 1 : 2;
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
