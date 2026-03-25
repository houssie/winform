using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace point;

[Table("pointcanon_games")]
public class GameState
{
    [Key]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("state_json", TypeName = "jsonb")]
    public string StateJson { get; set; } = null!;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class GameData
{
    public DateTime DateSauvegarde { get; set; }
    public int ScoreRouge { get; set; }
    public int ScoreBleu { get; set; }
    public int PositionCanonGauche { get; set; } // Ligne (0..12)
    public int PositionCanonDroit { get; set; }  // Ligne (0..12)
    public int PuissanceCanonGauche { get; set; }
    public int PuissanceCanonDroit { get; set; }
    public bool JoueurRougeActif { get; set; }
    public List<List<int>> Grille { get; set; } = new();
    public List<List<Point>>? Alignements { get; set; }
}
