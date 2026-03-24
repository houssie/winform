public enum CoteCanon
{
    Gauche,
    Droit
}

public class Canon
{
    public CoteCanon Cote { get; set; }
    public int PositionLigne { get; set; }  // Position verticale (0 à Taille)
    
    public Canon(CoteCanon cote)
    {
        Cote = cote;
        PositionLigne = 0;
    }
    
    public void Deplacer(int nouvelleLigne, int tailleMax)
    {
        if (nouvelleLigne >= 0 && nouvelleLigne <= tailleMax)
            PositionLigne = nouvelleLigne;
    }
}