using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace point;

[Table("gamestate")]
public class GameState
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("nom_partie")]
    public string NomPartie { get; set; } = "Partie sans nom";

    [Column("date_creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [Column("date_sauvegarde")]
    public DateTime DateSauvegarde { get; set; } = DateTime.Now;

    // États du jeu en JSON pour flexibilité
    [Column("etat_grille")]
    public string? EtatGrille { get; set; }

    [Column("score_rouge")]
    public int ScoreRouge { get; set; }

    [Column("score_bleu")]
    public int ScoreBleu { get; set; }

    [Column("position_canon_gauche")]
    public int PositionCanonGauche { get; set; }

    [Column("position_canon_droit")]
    public int PositionCanonDroit { get; set; }

    [Column("puissance_canon_gauche")]
    public int PuissanceCanonGauche { get; set; }

    [Column("puissance_canon_droit")]
    public int PuissanceCanonDroit { get; set; }
}
