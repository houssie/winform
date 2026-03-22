
using System.Drawing;
using System.Collections.Generic;

public class Plateau
{
    public int Taille { get; private set; }  // n
    public Cellule[,] Grille { get; private set; }
    
    public Plateau(int taille)
    {
        Taille = taille;
        Grille = new Cellule[taille + 1, taille + 1];
        
        // Initialiser toutes les cellules vides
        for (int i = 0; i <= taille; i++)
            for (int j = 0; j <= taille; j++)
                Grille[i, j] = new Cellule();
    }
    
    // Vérifier si une position est valide
    public bool EstPositionValide(int ligne, int colonne)
    {
        return ligne >= 0 && ligne <= Taille && 
               colonne >= 0 && colonne <= Taille;
    }
    
    // Placer un point
    public bool PlacerPoint(int ligne, int colonne, Joueur joueur)
    {
        if (!EstPositionValide(ligne, colonne)) return false;
        if (Grille[ligne, colonne].EstOccupee()) return false;
        
        Grille[ligne, colonne].Joueur = joueur;
        Grille[ligne, colonne].EstTouche = false;
        
        return true;
    }
    
    // Tirer au canon
    // Ne supprime pas un point appartenant au tireur lui-même.
    public bool TirerCanon(int ligne, int puissance, CoteCanon cote, Joueur tireur)
    {
        int rawCol = CalculerColonneCible(puissance);
        int colonneCible = (cote == CoteCanon.Droit) ? (Taille - rawCol) : rawCol;

        if (!EstPositionValide(ligne, colonneCible)) return false;

        var cellule = Grille[ligne, colonneCible];
        
        // Ne toucher que si occupée par l'adversaire et non protégée
        if (cellule.EstOccupee() && !cellule.EstProtege && cellule.Joueur != null && cellule.Joueur != tireur)
        {
            cellule.Joueur = null;
            cellule.EstTouche = true;
            return true;
        }
        
        return false;
    }
    
    public int CalculerColonneCible(int puissance)
{
    // Formule : colonne = arrondi((p × n) ÷ 9)
    double resultat = (puissance * Taille) / 9.0;
    return (int)Math.Round(resultat);
}

    // Dans la classe Plateau.cs
public List<List<Point>> VerifierAlignements(Joueur joueur)
{
    List<List<Point>> alignementsTrouves = new List<List<Point>>();
    
    // Vérifier toutes les directions
    int[][] directions = new int[][]
    {
        new int[] { 0, 1 },   // Horizontal droite
        new int[] { 1, 0 },   // Vertical bas
        new int[] { 1, 1 },   // Diagonale bas-droite
        new int[] { 1, -1 }   // Diagonale bas-gauche
    };
    
    for (int i = 0; i <= Taille; i++)
    {
        for (int j = 0; j <= Taille; j++)
        {
            // Si c'est un point du joueur
            if (Grille[i, j].Joueur == joueur && !Grille[i, j].EstProtege)
            {
                foreach (var dir in directions)
                {
                    // Pour éviter les doublons, n'examiner l'alignement que si (i,j)
                    // est le début (le précédent dans la direction négative n'appartient pas au joueur)
                    int prevX = i - dir[0];
                    int prevY = j - dir[1];
                    if (prevX >= 0 && prevX <= Taille && prevY >= 0 && prevY <= Taille)
                    {
                        if (Grille[prevX, prevY].Joueur == joueur) continue; // déjà vu depuis un point précédent
                    }

                    List<Point> alignement = VerifierDirection(i, j, dir[0], dir[1], joueur);

                    if (alignement.Count >= 5)
                    {
                        // Ne prendre que les 5 premiers points
                        List<Point> cinqPoints = alignement.Take(5).ToList();

                        // Vérifier si cet alignement n'est pas déjà protégé
                        bool dejaProtege = true;
                        foreach (var point in cinqPoints)
                        {
                            if (!Grille[point.X, point.Y].EstProtege)
                            {
                                dejaProtege = false;
                                break;
                            }
                        }

                        if (!dejaProtege)
                        {
                            alignementsTrouves.Add(cinqPoints);
                        }
                    }
                }
            }
        }
    }
    
    return alignementsTrouves;
}

private List<Point> VerifierDirection(int x, int y, int dx, int dy, Joueur joueur)
{
    List<Point> points = new List<Point>();
    int i = 0;
    
    // Vérifier dans la direction positive
    while (true)
    {
        int nx = x + (i * dx);
        int ny = y + (i * dy);
        
        if (nx < 0 || nx > Taille || ny < 0 || ny > Taille)
            break;
            
        if (Grille[nx, ny].Joueur == joueur && !Grille[nx, ny].EstProtege)
            points.Add(new Point(nx, ny));
        else
            break;
            
        i++;
    }
    
    i = 1;
    // Vérifier dans la direction négative
    while (true)
    {
        int nx = x - (i * dx);
        int ny = y - (i * dy);
        
        if (nx < 0 || nx > Taille || ny < 0 || ny > Taille)
            break;
            
        if (Grille[nx, ny].Joueur == joueur && !Grille[nx, ny].EstProtege)
            points.Insert(0, new Point(nx, ny));
        else
            break;
            
        i++;
    }
    
    return points;
}

public int MarquerAlignements(Joueur joueur)
{
    var alignements = VerifierAlignements(joueur);
    int pointsGagnes = 0;
    
    foreach (var alignement in alignements)
    {
        // Avant de marquer, vérifier si cet alignement croise une ligne protégée adverse
        if (AlignementCroiseAdversaire(alignement, joueur))
        {
            // Ne pas tracer cet alignement (la règle empêche de croiser une ligne protégée)
            continue;
        }

        // Marquer les points comme protégés
        foreach (var point in alignement)
        {
            Grille[point.X, point.Y].Proteger();
        }
        pointsGagnes++;
    }
    
    return pointsGagnes;
}

private bool AlignementCroiseAdversaire(List<Point> alignement, Joueur joueur)
{
    if (alignement == null || alignement.Count == 0) return false;

    // Segment candidat (entre premier et dernier point)
    Point a1 = alignement.First();
    Point a2 = alignement.Last();

    // Rechercher segments protégés appartenant à l'adversaire
    for (int i = 0; i <= Taille; i++)
    {
        for (int j = 0; j <= Taille; j++)
        {
            var cell = Grille[i, j];
            if (!cell.EstProtege || cell.Joueur == null || cell.Joueur == joueur) continue;

            // Pour chaque direction, si c'est le début d'une séquence protégée, extraire la séquence
            int[][] directions = new int[][] { new int[] {0,1}, new int[] {1,0}, new int[] {1,1}, new int[] {1,-1} };
            foreach (var dir in directions)
            {
                int px = i - dir[0];
                int py = j - dir[1];
                // s'assurer que (i,j) est le début (précédent n'est pas protégé du même joueur)
                if (px >= 0 && px <= Taille && py >= 0 && py <= Taille)
                {
                    var prev = Grille[px, py];
                    if (prev.EstProtege && prev.Joueur == cell.Joueur) continue;
                }

                // parcourir la séquence
                int sx = i;
                int sy = j;
                int ex = sx;
                int ey = sy;
                int len = 0;
                while (ex >= 0 && ex <= Taille && ey >= 0 && ey <= Taille)
                {
                    var c = Grille[ex, ey];
                    if (c.EstProtege && c.Joueur == cell.Joueur)
                    {
                        len++;
                        ex += dir[0];
                        ey += dir[1];
                    }
                    else break;
                }

                if (len >= 2)
                {
                    // segment protégé de l'adversaire de (sx,sy) à (ex-dir, ey-dir)
                    Point b1 = new Point(sx, sy);
                    Point b2 = new Point(ex - dir[0], ey - dir[1]);
                    if (SegmentsIntersect(a1, a2, b1, b2)) return true;
                }
            }
        }
    }

    return false;
}

private static int Orientation(Point p, Point q, Point r)
{
    long val = (long)(q.Y - p.Y) * (r.X - q.X) - (long)(q.X - p.X) * (r.Y - q.Y);
    if (val == 0) return 0;
    return (val > 0) ? 1 : 2;
}

private static bool OnSegment(Point p, Point q, Point r)
{
    return q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
           q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y);
}

private static bool SegmentsIntersect(Point p1, Point q1, Point p2, Point q2)
{
    int o1 = Orientation(p1, q1, p2);
    int o2 = Orientation(p1, q1, q2);
    int o3 = Orientation(p2, q2, p1);
    int o4 = Orientation(p2, q2, q1);

    if (o1 != o2 && o3 != o4) return true;

    if (o1 == 0 && OnSegment(p1, p2, q1)) return true;
    if (o2 == 0 && OnSegment(p1, q2, q1)) return true;
    if (o3 == 0 && OnSegment(p2, p1, q2)) return true;
    if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

    return false;
}

public bool EstPlacementLegal(int x, int y, Joueur joueur)
{
    if (!EstPositionValide(x, y)) return false;
    if (Grille[x, y].EstOccupee()) return false;

    // Simuler le placement
    Grille[x, y].Joueur = joueur;
    Grille[x, y].EstTouche = false;

    try
    {
        var alignements = VerifierAlignements(joueur);
        foreach (var align in alignements)
        {
            if (AlignementCroiseAdversaire(align, joueur))
            {
                return false;
            }
        }

        return true;
    }
    finally
    {
        // Annuler la simulation
        Grille[x, y].Joueur = null;
        Grille[x, y].EstTouche = false;
    }
}

// Vérifier les alignements qui passent par la case (x,y) et ne contiennent aucun point protégé.
public List<List<Point>> VerifierAlignementsDepuis(int x, int y, Joueur joueur)
{
    List<List<Point>> trouv = new List<List<Point>>();
    if (!EstPositionValide(x, y)) return trouv;
    if (Grille[x, y].Joueur != joueur) return trouv;
    // Si la case est protégée mais appartient à l'adversaire, ignorer
    if (Grille[x, y].EstProtege && Grille[x, y].Joueur != joueur) return trouv;

    int[][] directions = new int[][]
    {
        new int[] { 0, 1 },
        new int[] { 1, 0 },
        new int[] { 1, 1 },
        new int[] { 1, -1 }
    };

    foreach (var dir in directions)
    {
        List<Point> pts = new List<Point>();
        // include origin
        pts.Add(new Point(x, y));

        // forward
        int i = 1;
        while (true)
        {
            int nx = x + i * dir[0];
            int ny = y + i * dir[1];
            if (nx < 0 || nx > Taille || ny < 0 || ny > Taille) break;
            var c = Grille[nx, ny];
            // Autoriser les points du joueur même s'ils sont protégés; arrêter dès qu'on rencontre une case non-appartenant au joueur
            if (c.Joueur == joueur) pts.Add(new Point(nx, ny)); else break;
            i++;
        }

        // backward
        i = 1;
        while (true)
        {
            int nx = x - i * dir[0];
            int ny = y - i * dir[1];
            if (nx < 0 || nx > Taille || ny < 0 || ny > Taille) break;
            var c = Grille[nx, ny];
            if (c.Joueur == joueur) pts.Insert(0, new Point(nx, ny)); else break;
            i++;
        }

        if (pts.Count >= 5)
        {
            // take first 5 contiguous that include origin: find any window of size 5 that contains origin
            for (int s = 0; s + 5 <= pts.Count; s++)
            {
                var window = pts.GetRange(s, 5);
                // check if origin inside window
                bool containsOrigin = window.Exists(p => p.X == x && p.Y == y);
                if (containsOrigin)
                {
                    trouv.Add(window);
                    break; // only one alignment per direction starting from this origin
                }
            }
        }
    }

    return trouv;
}

// Marquer et retourner le nombre d'alignements détectés depuis (x,y)
public int MarquerAlignementsDepuis(int x, int y, Joueur joueur)
{
    var alignements = VerifierAlignementsDepuis(x, y, joueur);
    int pointsGagnes = 0;
    foreach (var alignement in alignements)
    {
        if (AlignementCroiseAdversaire(alignement, joueur)) continue;
        foreach (var point in alignement) Grille[point.X, point.Y].Proteger();
        pointsGagnes++;
    }
    return pointsGagnes;
}
}