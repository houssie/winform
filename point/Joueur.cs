public enum Couleur
{
    Rouge,   // Joueur A (●)
    Bleu     // Joueur B (○)
}

public class Joueur
{
    public string Nom { get; set; }
    public Couleur Couleur { get; set; }
    public int Score { get; set; }
    public bool EstActif { get; set; }
    
    public Joueur(string nom, Couleur couleur)
    {
        Nom = nom;
        Couleur = couleur;
        Score = 0;
        EstActif = true;
    }
    
    public override string ToString()
    {
        return $"{Nom} ({Couleur}) - Score: {Score}";
    }
}