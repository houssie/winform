public class Cellule
{
    public Joueur? Joueur { get; set; }  // Joueur qui a placé le point
    public bool EstProtege { get; set; }  // Point protégé (ligne tracée)
    public bool EstTouche { get; set; }   // A été touché par un canon
    
    public bool EstOccupee()
    {
        return Joueur != null;
    }
    
    public void Proteger()
    {
        EstProtege = true;
    }
}