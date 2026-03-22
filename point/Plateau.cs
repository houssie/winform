
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

public class Plateau
{
    public int Taille { get; private set; }
    public Cellule[,] Grille { get; private set; }

    public Plateau(int taille)
    {
        Taille = taille;
        Grille = new Cellule[taille, taille];
        for (int i = 0; i < taille; i++)
            for (int j = 0; j < taille; j++)
                Grille[i, j] = new Cellule();
    }

    public bool EstPositionValide(int ligne, int colonne)
    {
        return ligne >= 0 && ligne < Taille && colonne >= 0 && colonne < Taille;
    }

    public bool PlacerPoint(int ligne, int colonne, Joueur joueur)
    {
        if (!EstPositionValide(ligne, colonne)) return false;
        if (Grille[ligne, colonne].EstOccupee()) return false;

        Grille[ligne, colonne].Joueur = joueur;
        Grille[ligne, colonne].EstTouche = false;
        return true;
    }

    // Tirer au canon. Ne supprime pas un point appartenant au tireur.
    public bool TirerCanon(int ligne, int puissance, CoteCanon cote, Joueur tireur)
    {
        int rawCol = CalculerColonneCible(puissance);
        int colonneCible = (cote == CoteCanon.Droit) ? (Taille - 1 - rawCol) : rawCol;

        if (!EstPositionValide(ligne, colonneCible)) return false;

        var cellule = Grille[ligne, colonneCible];
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
        // Map puissance 0..9 to column 0..Taille-1
        double resultat = (puissance * (Taille - 1)) / 9.0;
        int col = (int)Math.Round(resultat);
        if (col < 0) col = 0;
        if (col >= Taille) col = Taille - 1;
        return col;
    }

    // Recherche d'alignements (séquences de 5) pour un joueur
    public List<List<Point>> VerifierAlignements(Joueur joueur)
    {
        var result = new List<List<Point>>();

        int[][] directions = new int[][]
        {
            new int[] { 0, 1 },
            new int[] { 1, 0 },
            new int[] { 1, 1 },
            new int[] { 1, -1 }
        };

        for (int i = 0; i < Taille; i++)
        {
            for (int j = 0; j < Taille; j++)
            {
                if (Grille[i, j].Joueur != joueur || Grille[i, j].EstProtege) continue;

                foreach (var dir in directions)
                {
                    int prevX = i - dir[0];
                    int prevY = j - dir[1];
                    if (prevX >= 0 && prevX < Taille && prevY >= 0 && prevY < Taille)
                    {
                        if (Grille[prevX, prevY].Joueur == joueur) continue; // déjà traité
                    }

                    var align = VerifierDirection(i, j, dir[0], dir[1], joueur);
                    if (align.Count >= 5)
                    {
                        var firstFive = align.Take(5).ToList();
                        bool tousProteges = firstFive.All(p => Grille[p.X, p.Y].EstProtege);
                        if (!tousProteges) result.Add(firstFive);
                    }
                }
            }
        }

        return result;
    }

    // Retourne la séquence contiguë (incluant l'origine) dans les deux sens pour un joueur
    private List<Point> VerifierDirection(int x, int y, int dx, int dy, Joueur joueur)
    {
        var pts = new List<Point>();

        // backward
        int k = 1;
        while (true)
        {
            int nx = x - k * dx;
            int ny = y - k * dy;
            if (nx < 0 || nx >= Taille || ny < 0 || ny >= Taille) break;
            if (Grille[nx, ny].Joueur == joueur && !Grille[nx, ny].EstProtege) pts.Insert(0, new Point(nx, ny)); else break;
            k++;
        }

        // origin
        pts.Add(new Point(x, y));

        // forward
        k = 1;
        while (true)
        {
            int nx = x + k * dx;
            int ny = y + k * dy;
            if (nx < 0 || nx >= Taille || ny < 0 || ny >= Taille) break;
            if (Grille[nx, ny].Joueur == joueur && !Grille[nx, ny].EstProtege) pts.Add(new Point(nx, ny)); else break;
            k++;
        }

        return pts;
    }

    public int MarquerAlignements(Joueur joueur)
    {
        var aligns = VerifierAlignements(joueur);
        int pts = 0;
        foreach (var a in aligns)
        {
            if (AlignementCroiseAdversaire(a, joueur)) continue;
            foreach (var p in a) Grille[p.X, p.Y].Proteger();
            pts++;
        }
        return pts;
    }

    private bool AlignementCroiseAdversaire(List<Point> alignement, Joueur joueur)
    {
        if (alignement == null || alignement.Count == 0) return false;
        Point a1 = alignement.First();
        Point a2 = alignement.Last();

        int[][] directions = new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { 1, 1 }, new int[] { 1, -1 } };

        for (int i = 0; i < Taille; i++)
        {
            for (int j = 0; j < Taille; j++)
            {
                var cell = Grille[i, j];
                if (!cell.EstProtege || cell.Joueur == null || cell.Joueur == joueur) continue;

                foreach (var dir in directions)
                {
                    int px = i - dir[0];
                    int py = j - dir[1];
                    if (px >= 0 && px < Taille && py >= 0 && py < Taille)
                    {
                        var prev = Grille[px, py];
                        if (prev.EstProtege && prev.Joueur == cell.Joueur) continue;
                    }

                    int sx = i, sy = j;
                    int ex = sx, ey = sy;
                    int len = 0;
                    while (ex >= 0 && ex < Taille && ey >= 0 && ey < Taille)
                    {
                        var c = Grille[ex, ey];
                        if (c.EstProtege && c.Joueur == cell.Joueur)
                        {
                            len++;
                            ex += dir[0]; ey += dir[1];
                        }
                        else break;
                    }

                    if (len >= 2)
                    {
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
        return q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) && q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y);
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

        var backup = new Cellule { Joueur = Grille[x, y].Joueur, EstTouche = Grille[x, y].EstTouche, EstProtege = Grille[x, y].EstProtege };
        Grille[x, y].Joueur = joueur;
        Grille[x, y].EstTouche = false;

        try
        {
            var aligns = VerifierAlignements(joueur);
            foreach (var a in aligns)
            {
                if (AlignementCroiseAdversaire(a, joueur)) return false;
            }
            return true;
        }
        finally
        {
            Grille[x, y].Joueur = backup.Joueur;
            Grille[x, y].EstTouche = backup.EstTouche;
            Grille[x, y].EstProtege = backup.EstProtege;
        }
    }

    // Vérifier alignements qui incluent explicitement (x,y)
    public List<List<Point>> VerifierAlignementsDepuis(int x, int y, Joueur joueur)
    {
        var trouv = new List<List<Point>>();
        int[][] directions = new int[][]
        {
            new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { 1, 1 }, new int[] { 1, -1 }
        };

        foreach (var dir in directions)
        {
            var pts = new List<Point> { new Point(x, y) };

            // forward
            int i = 1;
            while (true)
            {
                int nx = x + i * dir[0];
                int ny = y + i * dir[1];
                if (nx < 0 || nx >= Taille || ny < 0 || ny >= Taille) break;
                var c = Grille[nx, ny];
                if (c.Joueur == joueur) pts.Add(new Point(nx, ny)); else break;
                i++;
            }

            // backward
            i = 1;
            while (true)
            {
                int nx = x - i * dir[0];
                int ny = y - i * dir[1];
                if (nx < 0 || nx >= Taille || ny < 0 || ny >= Taille) break;
                var c = Grille[nx, ny];
                if (c.Joueur == joueur) pts.Insert(0, new Point(nx, ny)); else break;
                i++;
            }

            if (pts.Count >= 5)
            {
                for (int s = 0; s + 5 <= pts.Count; s++)
                {
                    var window = pts.GetRange(s, 5);
                    if (window.Exists(p => p.X == x && p.Y == y))
                    {
                        trouv.Add(window);
                        break;
                    }
                }
            }
        }

        return trouv;
    }

    public int MarquerAlignementsDepuis(int x, int y, Joueur joueur)
    {
        var aligns = VerifierAlignementsDepuis(x, y, joueur);
        int pts = 0;
        foreach (var a in aligns)
        {
            if (AlignementCroiseAdversaire(a, joueur)) continue;
            foreach (var p in a) Grille[p.X, p.Y].Proteger();
            pts++;
        }
        return pts;
    }
}