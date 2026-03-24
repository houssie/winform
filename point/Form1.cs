using System;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualBasic;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace point
{
public partial class Form1 : Form
{
    private Plateau plateau = null!;
    private Joueur joueurA = null!;
    private Joueur joueurB = null!;
    private Joueur joueurActuel = null!;
    private Canon canonGauche = null!;
    private Canon canonDroit = null!;
    private GameService gameService = null!;

    private Button[,] boutonsGrille = null!;
    private Panel? panelGrille = null;
    private Panel? panelLeft = null;
    private Panel? panelRight = null;
    private Label labelStatus = null!;
    private Label labelScore = null!;
    private int puissanceCourante = 5;
    // Le sélecteur de canon assigné a été retiré : le canon utilisé est déterminé automatiquement.
    private Button buttonTirer = null!;
    private Button[] boutonsCanonGauche = null!;
    private Button[] boutonsCanonDroit = null!;
    private Button buttonFinirTour = null!;
    private bool enPhaseCanon = false;
    private bool phaseRequireTir = false;
    private bool tirEffectueDansPhase = false;
    private Button buttonChoisirTirer = null!;
    private Button buttonChoisirPlacer = null!;
    private bool choixExclusifPlacer = false;
    private Label labelPhase = null!;
    private bool attenteChoix = false; // vrai tant que le joueur n'a pas choisi TIRER ou PLACER
    
    // Variables pour le drag-and-drop des canons
    private bool isCanonGaucheDragging = false;
    private bool isCanonDroitDragging = false;
    private int draggingStartY = 0;
    
    private int taillePlateau = 12; 
    private readonly string debugLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "point_debug.log");
    
    // Les contrôles de déplacement manuel via bouton ont été retirés :
    // déplacement se fait via les boutons latéraux ou clics dédiés.
    
    public Form1()
    {
        InitializeComponent();
        GameService.InitializeDatabase();
        gameService = new GameService();
        InitialiserJeu();
    }
    
    private void InitialiserJeu()
    {
        // Créer le plateau
        plateau = new Plateau(taillePlateau);
        
        // Créer les joueurs
        joueurA = new Joueur("Joueur A", Couleur.Rouge);
        joueurB = new Joueur("Joueur B", Couleur.Bleu);
        joueurActuel = joueurA;
        
        // Créer les canons
        canonGauche = new Canon(CoteCanon.Gauche);
        canonDroit = new Canon(CoteCanon.Droit);
        
        // Configurer l'interface
        ConfigurerInterface();
        
        // Placer les points de démonstration comme sur l'image
        InitialiserDemonstration();
        
        // Mettre à jour l'affichage
        MettreAJourAffichage();
        // Démarrer le premier tour
        DemarrerTour();
    }
    
    private void ConfigurerInterface()
    {
        this.Text = "Point+Canon - Jeu de stratégie";
        this.Size = new Size(900, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        
        // Calcul taille case et panels (grille n x n)
        int tailleCase = 600 / (taillePlateau);

        // Panel gauche pour canons (hors plateau)
        panelLeft = new Panel();
        panelLeft.Location = new Point(20, 20);
        panelLeft.Size = new Size(tailleCase, 600);
        panelLeft.BackColor = Color.Transparent;
        panelLeft.Paint += PanelCanonGauche_Paint;
        panelLeft.MouseDown += PanelLeft_MouseDown;
        panelLeft.MouseMove += PanelLeft_MouseMove;
        panelLeft.MouseUp += PanelLeft_MouseUp;

        // Panel pour la grille (décalé à droite pour laisser la colonne des canons)
        panelGrille = new Panel();
        panelGrille.Location = new Point(20 + tailleCase, 20);
        panelGrille.Size = new Size(600, 600);
        panelGrille.BackColor = Color.White;
        
        // Ajouter un événement Paint pour dessiner la grille et les points
        panelGrille.Paint += PanelGrille_Paint;

        // Panel droit pour canons (hors plateau)
        panelRight = new Panel();
        panelRight.Location = new Point(panelGrille.Left + panelGrille.Width, 20);
        panelRight.Size = new Size(tailleCase, 600);
        panelRight.BackColor = Color.Transparent;
        panelRight.Paint += PanelCanonDroit_Paint;
        panelRight.MouseDown += PanelRight_MouseDown;
        panelRight.MouseMove += PanelRight_MouseMove;
        panelRight.MouseUp += PanelRight_MouseUp;
        
        // Créer les boutons de la grille avec (taillePlateau+1) x (taillePlateau+1) intersections
        boutonsGrille = new Button[taillePlateau + 1, taillePlateau + 1];

        for (int i = 0; i <= taillePlateau; i++)
        {
            for (int j = 0; j <= taillePlateau; j++)
            {
                Button btn = new Button();
                // Positions aux intersections (lignes et colonnes)
                int xPos = (j * 600) / taillePlateau - 5;  // centré sur l'intersection
                int yPos = (i * 600) / taillePlateau - 5;  // centré sur l'intersection
                btn.Location = new Point(xPos, yPos);
                btn.Size = new Size(10, 10);  // Petit bouton juste pour le clic
                btn.BackColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Tag = new Point(i, j);
                btn.Click += BtnGrille_Click;
                
                panelGrille.Controls.Add(btn);
                boutonsGrille[i, j] = btn;
            }
        }

        // Créer boutons pour canons hors plateau (taillePlateau+1 lignes)
        boutonsCanonGauche = new Button[taillePlateau + 1];
        boutonsCanonDroit = new Button[taillePlateau + 1];
        for (int i = 0; i <= taillePlateau; i++)
        {
            Button bL = new Button();
            int yPos = (i * 600) / taillePlateau - 5;
            bL.Location = new Point(0, yPos);
            bL.Size = new Size(10, 10);
            bL.FlatStyle = FlatStyle.Flat;
            bL.FlatAppearance.BorderSize = 0;
            bL.Tag = i; // stocke la ligne
            bL.Click += BoutonCanonGauche_Click;
            panelLeft.Controls.Add(bL);
            boutonsCanonGauche[i] = bL;

            Button bR = new Button();
            bR.Location = new Point(0, yPos);
            bR.Size = new Size(10, 10);
            bR.FlatStyle = FlatStyle.Flat;
            bR.FlatAppearance.BorderSize = 0;
            bR.Tag = i; // stocke la ligne
            bR.Click += BoutonCanonDroit_Click;
            panelRight.Controls.Add(bR);
            boutonsCanonDroit[i] = bR;
        }
        
        // Panel d'information
        Panel panelInfo = new Panel();
        panelInfo.Location = new Point(650, 20);
        panelInfo.Size = new Size(220, 600);
        panelInfo.BackColor = Color.LightGray;
        
        // Label statut
        labelStatus = new Label();
        labelStatus.Location = new Point(10, 20);
        labelStatus.Size = new Size(200, 30);
        labelStatus.Font = new Font("Arial", 12, FontStyle.Bold);
        labelStatus.BorderStyle = BorderStyle.FixedSingle;
        labelStatus.BackColor = Color.FromArgb(255, 245, 200);
        labelStatus.TextAlign = ContentAlignment.MiddleCenter;
        labelStatus.Padding = new Padding(4);
        panelInfo.Controls.Add(labelStatus);

        // Label phase (indicateur clair)
        labelPhase = new Label();
        labelPhase.Location = new Point(10, 55);
        labelPhase.Size = new Size(200, 20);
        labelPhase.Font = new Font("Arial", 9, FontStyle.Bold);
        labelPhase.TextAlign = ContentAlignment.MiddleCenter;
        labelPhase.BackColor = Color.Transparent;
        panelInfo.Controls.Add(labelPhase);
        
        // Label score
        labelScore = new Label();
        labelScore.Location = new Point(10, 80);
        labelScore.Size = new Size(200, 60);
        labelScore.Font = new Font("Arial", 10);
        panelInfo.Controls.Add(labelScore);
        
        // Le choix manuel de canon a été supprimé : le canon est déterminé par le joueur actif.
        
        // La puissance est maintenue par la variable `puissanceCourante` (raccourcis Ctrl+1..9).
        
        // Ajouter les panels
        this.Controls.Add(panelLeft);
        this.Controls.Add(panelGrille);
        this.Controls.Add(panelRight);
        // placer panelInfo à droite du panelRight
        panelInfo.Location = new Point(panelRight.Right + 20, 20);
        this.Controls.Add(panelInfo);

        // Dans ConfigurerInterface(), ajoutez après le bouton Tirer :

// Le contrôle de position du canon et son bouton ont été retirés :
// on déplace désormais les canons via les boutons latéraux.

    // Bouton pour terminer la phase canon / finir le tour
    // Le bouton "FINIR LE TOUR" a été retiré : le tour se termine automatiquement
    // après les actions requises (placement ou tir).

    // Boutons d'action non-modal: Tirer ou Placer (montrent au début du tour)
    buttonChoisirTirer = new Button();
    buttonChoisirTirer.Text = "CHOISIR: TIRER";
    buttonChoisirTirer.Location = new Point(10, 310);
    buttonChoisirTirer.Size = new Size(95, 30);
    buttonChoisirTirer.BackColor = Color.LightSalmon;
    buttonChoisirTirer.Visible = false;
    buttonChoisirTirer.Click += ButtonChoisirTirer_Click;
    panelInfo.Controls.Add(buttonChoisirTirer);

    buttonChoisirPlacer = new Button();
    buttonChoisirPlacer.Text = "CHOISIR: PLACER";
    buttonChoisirPlacer.Location = new Point(115, 310);
    buttonChoisirPlacer.Size = new Size(95, 30);
    buttonChoisirPlacer.BackColor = Color.LightGreen;
    buttonChoisirPlacer.Visible = false;
    buttonChoisirPlacer.Click += ButtonChoisirPlacer_Click;
    panelInfo.Controls.Add(buttonChoisirPlacer);

    // Légende explicative
    GroupBox legendBox = new GroupBox();
    legendBox.Text = "Légende";
    legendBox.Location = new Point(10, 460);
    legendBox.Size = new Size(200, 140);
    legendBox.Font = new Font("Arial", 9, FontStyle.Regular);

    Label l1 = new Label();
    l1.Text = "● Joueur A (Rouge)";
    l1.Location = new Point(10, 20);
    l1.Size = new Size(180, 20);
    l1.ForeColor = Color.Red;
    legendBox.Controls.Add(l1);

    Label l2 = new Label();
    l2.Text = "○ Joueur B (Bleu)";
    l2.Location = new Point(10, 40);
    l2.Size = new Size(180, 20);
    l2.ForeColor = Color.Blue;
    legendBox.Controls.Add(l2);

    Label l3 = new Label();
    l3.Text = "Fond vert: point protégé";
    l3.Location = new Point(10, 60);
    l3.Size = new Size(180, 20);
    legendBox.Controls.Add(l3);

    Label l4 = new Label();
    l4.Text = "Canon gauche : Joueur A (Rouge)";
    l4.Location = new Point(10, 80);
    l4.Size = new Size(180, 20);
    l4.ForeColor = Color.Red;
    legendBox.Controls.Add(l4);

    Label l5 = new Label();
    l5.Text = "Canon droite : Joueur B (Bleu)";
    l5.Location = new Point(10, 100);
    l5.Size = new Size(180, 20);
    l5.ForeColor = Color.Blue;
    legendBox.Controls.Add(l5);

    Label l6 = new Label();
    l6.Text = "Utiliser 'TIRER AU CANON' pour tirer";
    l6.Location = new Point(10, 120);
    l6.Size = new Size(180, 20);
    legendBox.Controls.Add(l6);

    panelInfo.Controls.Add(legendBox);

  MenuStrip menuStrip = new MenuStrip();
    
    // Menu Fichier
    ToolStripMenuItem menuFichier = new ToolStripMenuItem("Fichier");
    ToolStripMenuItem nouvellePartie = new ToolStripMenuItem("Nouvelle partie");
    ToolStripMenuItem sauvegarder = new ToolStripMenuItem("Sauvegarder");
    ToolStripMenuItem charger = new ToolStripMenuItem("Charger");
    ToolStripMenuItem quitter = new ToolStripMenuItem("Quitter");
    
    nouvellePartie.Click += NouvellePartie_Click;
    sauvegarder.Click += Sauvegarder_Click;
    charger.Click += Charger_Click;
    quitter.Click += (s, e) => Application.Exit();
    
    menuFichier.DropDownItems.AddRange(new ToolStripMenuItem[] { 
        nouvellePartie, sauvegarder, charger, quitter });
    
    // Menu Aide
    ToolStripMenuItem menuAide = new ToolStripMenuItem("Aide");
    ToolStripMenuItem regles = new ToolStripMenuItem("Règles du jeu");
    regles.Click += Regles_Click;
    menuAide.DropDownItems.Add(regles);
    
    menuStrip.Items.AddRange(new ToolStripMenuItem[] { menuFichier, menuAide });
    
    this.MainMenuStrip = menuStrip;
    this.Controls.Add(menuStrip);

    // Ajouter option pour changer la taille du plateau à la volée
    ToolStripMenuItem changerTaille = new ToolStripMenuItem("Changer taille...");
    changerTaille.Click += ChangerTaille_Click;
    menuFichier.DropDownItems.Add(changerTaille);
    // Permettre au formulaire d'intercepter les touches (raccourcis clavier)
    this.KeyPreview = true;
    this.KeyDown += Form1_KeyDown;
    }

    // --- Fonctions de redimensionnement du plateau (inserées ici pour rester dans la classe)
    
    
    private void BtnGrille_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not Point pos) return;
        // Si on attend le choix du joueur, bloquer toute action jusqu'au choix
        if (attenteChoix)
        {
            MessageBox.Show("Choisissez d'abord: TIRER ou PLACER", "Choix requis", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Si on est en phase canon (après placement ou choix), on n'autorise pas de nouveau placement
        if (enPhaseCanon)
        {
            MessageBox.Show("Vous êtes en phase canon : déplacez le canon (boutons latéraux) ou tirez (CHOISIR: TIRER / Ctrl+1..9). Le tour se terminera automatiquement.", "Phase canon active", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        // Vérifier la règle de non-croisement avant de placer
        if (!plateau.EstPlacementLegal(pos.X, pos.Y, joueurActuel))
        {
            MessageBox.Show("Placement interdit : croisement d'une ligne protégée adverse.", "Placement interdit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Placer un point
        if (plateau.PlacerPoint(pos.X, pos.Y, joueurActuel))
        {
            Log($"BtnGrille_Click: placement par {joueurActuel?.Nom} en ({pos.X},{pos.Y})");
            // Vérifier les alignements uniquement liés à la case placée
            int pointsGagnes = plateau.MarquerAlignementsDepuis(pos.X, pos.Y, joueurActuel);
            if (pointsGagnes > 0)
            {
                joueurActuel.Score += pointsGagnes;
                MettreAJourAffichage();
                MessageBox.Show($"{joueurActuel.Nom} a marqué {pointsGagnes} point(s) !", "Point", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Si le joueur avait choisi PLACER exclusivement au début du tour,
            // le tour se termine immédiatement (pas de phase canon possible)
            if (choixExclusifPlacer)
            {
                choixExclusifPlacer = false;
                // Passer au joueur suivant
                joueurActuel = (joueurActuel == joueurA) ? joueurB : joueurA;
                MettreAJourAffichage();
                DemarrerTour();
                return;
            }

            // Après placement, terminer immédiatement le tour : plus de tir possible ce tour
            // (règle : PLACER est exclusif)
            enPhaseCanon = false;
            phaseRequireTir = false;
            tirEffectueDansPhase = false;
            if (buttonChoisirTirer != null) buttonChoisirTirer.Visible = false;
            if (buttonChoisirPlacer != null) buttonChoisirPlacer.Visible = false;

            // Passer au joueur suivant
            joueurActuel = (joueurActuel == joueurA) ? joueurB : joueurA;
            MettreAJourAffichage();
            DemarrerTour();
            return;
        }
        else
        {
            MessageBox.Show("Case déjà occupée ou invalide!", "Erreur", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
    
    private async void ButtonTirer_Click(object? sender, EventArgs e)
    {
        // Si le joueur a choisi PLACER de manière exclusive, empêcher tout tir
        if (choixExclusifPlacer)
        {
            MessageBox.Show("Vous avez choisi PLACER exclusivement : le tir n'est pas autorisé ce tour.", "Action non autorisée", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
    int puissance = puissanceCourante;
        // Le canon utilisé est celui du joueur courant : A -> gauche, B -> droite
        CoteCanon cote = (joueurActuel == joueurA) ? CoteCanon.Gauche : CoteCanon.Droit;

    int ligneCanon = (cote == CoteCanon.Gauche) ? canonGauche.PositionLigne : canonDroit.PositionLigne;
    int rawCol = plateau.CalculerColonneCible(puissance);
    int colonneCible = (cote == CoteCanon.Droit) ? (plateau.Taille - 1 - rawCol) : rawCol;

    // Prévenir réentrance : mémoriser le tireur et passer immédiatement au joueur suivant
    Joueur tireur = joueurActuel;
    Log($"ButtonTirer_Click: début - tireur={tireur?.Nom}, puissance={puissance}");
    Joueur suivant = (joueurActuel == joueurA) ? joueurB : joueurA;
    // Marquer qu'un tir a été effectué dans la phase (si on était en phase)
    tirEffectueDansPhase = true;
    enPhaseCanon = false;
    // Bascule du joueur avant l'animation pour éviter double-tir dû à des événements clavier
    joueurActuel = suivant;
    MettreAJourAffichage();

    // Animation du tir (affiche la cible correcte selon le côté)
    await AnimerTir(ligneCanon, colonneCible, cote);

    // Effectuer le tir en utilisant le tireur mémorisé
    if (plateau.TirerCanon(ligneCanon, puissance, cote, tireur))
    {
        Log($"ButtonTirer_Click: tir EFFECTIF par {tireur?.Nom} sur ({ligneCanon},{colonneCible})");
        MettreAJourAffichage();
        // Vérifier si le tir a créé un alignement pour le joueur courant (déjà basculé)
        VerifierAlignements(joueurActuel);
        // Démarrer le tour suivant (le joueur a déjà été basculé)
        DemarrerTour();
    }
    else
    {
        Log($"ButtonTirer_Click: tir SANS EFFET par {tireur?.Nom} sur ({ligneCanon},{colonneCible})");
        MessageBox.Show("Tir sans effet ou position invalide!", "Information", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        // Même en cas de tir sans effet, on considère le tir comme consommé et on termine la phase
        enPhaseCanon = false;
        if (buttonFinirTour != null) buttonFinirTour.Enabled = false;
        MettreAJourAffichage();
        DemarrerTour();
    }
}

    private void DemarrerTour()
    {
        Log($"DemarrerTour: joueurActuel={(joueurActuel==null?"null":joueurActuel.Nom)}");
        // Affiche un message demandant l'action du joueur courant
        MettreAJourAffichage();

        if (joueurActuel == null) return;
        // Réinitialiser l'état du tour : aucune action active tant que le joueur ne choisit pas
        enPhaseCanon = false;
        phaseRequireTir = false;
        tirEffectueDansPhase = false;
        choixExclusifPlacer = false;
        attenteChoix = true;

        // Montrer les boutons non-modaux pour choisir l'action
        if (buttonChoisirTirer != null) { buttonChoisirTirer.Visible = true; buttonChoisirTirer.Enabled = true; }
        if (buttonChoisirPlacer != null) { buttonChoisirPlacer.Visible = true; buttonChoisirPlacer.Enabled = true; }
        // Mettre à jour le statut pour inviter le joueur
        labelStatus.Text = $"{joueurActuel.Nom} — choisissez: TIRER ou PLACER";

        // Petite animation pour attirer l'attention sur le choix
        AnimerBoutonsChoix();
    }

    // Le bouton "FINIR LE TOUR" et sa logique ont été supprimés :
    // le tour se termine automatiquement selon les règles (après placement ou tir).

    private void ButtonChoisirTirer_Click(object? sender, EventArgs e)
    {
        Log($"ButtonChoisirTirer_Click: joueur={(joueurActuel==null?"null":joueurActuel.Nom)}");
        // Entrer en phase canon et exiger un tir pour finir le tour
        enPhaseCanon = true;
        phaseRequireTir = true;
        tirEffectueDansPhase = false;
        if (buttonFinirTour != null) buttonFinirTour.Enabled = true;

        // Cacher les boutons d'action
        if (buttonChoisirTirer != null) buttonChoisirTirer.Visible = false;
        if (buttonChoisirPlacer != null) buttonChoisirPlacer.Visible = false;

        labelStatus.Text = $"{joueurActuel.Nom} — Phase canon (tir requis). Déplacez le canon ou réglez la puissance.";
        MettreAJourAffichage();
        attenteChoix = false;
    }

    private void ButtonChoisirPlacer_Click(object? sender, EventArgs e)
    {
        Log($"ButtonChoisirPlacer_Click: joueur={(joueurActuel==null?"null":joueurActuel.Nom)}");
        // Permettre placement normalement (pas de tir requis)
        enPhaseCanon = false;
        phaseRequireTir = false;
        tirEffectueDansPhase = false;

        // Marquer que le choix PLACER est exclusif : le tour se termine après placement
        choixExclusifPlacer = true;

        // Cacher les boutons d'action
        if (buttonChoisirTirer != null) buttonChoisirTirer.Visible = false;
        if (buttonChoisirPlacer != null) buttonChoisirPlacer.Visible = false;

        // Désactiver le bouton tirer car PLACER est exclusif
        if (buttonTirer != null) buttonTirer.Enabled = false;

        labelStatus.Text = $"{joueurActuel.Nom} — Placez un point sur la grille.";
        MettreAJourAffichage();
        attenteChoix = false;
    }
    
    private void VerifierAlignements(Joueur joueur)
    {
        // Appelle la logique du plateau pour détecter et marquer les alignements
        int pointsGagnes = plateau.MarquerAlignements(joueur);

        if (pointsGagnes > 0)
        {
            joueur.Score += pointsGagnes;
            MettreAJourAffichage();
            MessageBox.Show($"{joueur.Nom} a marqué {pointsGagnes} point(s) !", "Point", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    
    private void MettreAJourAffichage()
    {
        // Rafraîchir le panel pour redessiner la grille et les points
        if (panelGrille != null) panelGrille.Invalidate();

        // Rafraîchir les panels des canons
        if (panelLeft != null) panelLeft.Invalidate();
        if (panelRight != null) panelRight.Invalidate();

        // Mettre à jour les labels
        labelStatus.Text = $"Au tour de: {joueurActuel!.Nom} ({joueurActuel!.Couleur})";
        labelScore.Text = $"Score:\n{joueurA!.Nom}: {joueurA!.Score}\n{joueurB!.Nom}: {joueurB!.Score}";

        // Mettre à jour l'indicateur de phase
        if (labelPhase != null)
        {
            if (enPhaseCanon)
            {
                labelPhase.Text = phaseRequireTir ? "Phase Canon — TIR REQUIS" : "Phase Canon — tir optionnel";
                labelPhase.BackColor = phaseRequireTir ? Color.LightCoral : Color.LightGoldenrodYellow;
            }
            else
            {
                labelPhase.Text = "Phase Normal — placement possible";
                labelPhase.BackColor = Color.Transparent;
            }
        }

        // Activer/désactiver les contrôles selon la phase
        if (buttonTirer != null) buttonTirer.Enabled = enPhaseCanon && !choixExclusifPlacer;

        // Montrer/cacher les boutons CHOISIR selon l'état du tour
        if (!enPhaseCanon)
        {
            if (buttonChoisirPlacer != null) { buttonChoisirPlacer.Visible = true; buttonChoisirPlacer.BringToFront(); }
            if (buttonChoisirTirer != null) { buttonChoisirTirer.Visible = !choixExclusifPlacer; buttonChoisirTirer.BringToFront(); }
        }
        else
        {
            if (buttonChoisirPlacer != null) buttonChoisirPlacer.Visible = false;
            if (buttonChoisirTirer != null) buttonChoisirTirer.Visible = false;
        }

        // Animer l'indicateur de tour
        AnimerIndicateurTour();
    }

    private void PanelGrille_Paint(object? sender, PaintEventArgs e)
    {
        // Dessiner la grille
        using (Pen gridPen = new Pen(Color.Black, 1))
        {
            for (int k = 0; k <= taillePlateau; k++)
            {
                int pos = (k * 600) / taillePlateau;
                // Lignes verticales
                e.Graphics.DrawLine(gridPen, pos, 0, pos, 600);
                // Lignes horizontales
                e.Graphics.DrawLine(gridPen, 0, pos, 600, pos);
            }
        }

        // Dessiner les points aux intersections
        if (plateau == null) return;
        
        for (int i = 0; i <= taillePlateau; i++)
        {
            for (int j = 0; j <= taillePlateau; j++)
            {
                var cellule = plateau.Grille[i, j];
                if (!cellule.EstOccupee()) continue;

                // Position de l'intersection
                int xCenter = (j * 600) / taillePlateau;
                int yCenter = (i * 600) / taillePlateau;
                int rayon = 8;

                // Dessiner le point
                if (cellule.Joueur != null)
                {
                    if (cellule.Joueur.Couleur == Couleur.Rouge)
                    {
                        // Point plein rouge
                        using (Brush brush = new SolidBrush(Color.Red))
                        {
                            e.Graphics.FillEllipse(brush, xCenter - rayon, yCenter - rayon, rayon * 2, rayon * 2);
                        }
                    }
                    else
                    {
                        // Point creux bleu
                        using (Pen pen = new Pen(Color.Blue, 2))
                        {
                            e.Graphics.DrawEllipse(pen, xCenter - rayon, yCenter - rayon, rayon * 2, rayon * 2);
                        }
                    }

                    // Fond vert si protection
                    if (cellule.EstProtege)
                    {
                        int rectSize = 20;
                        using (Brush brush = new SolidBrush(Color.FromArgb(100, 144, 238, 144)))
                        {
                            e.Graphics.FillRectangle(brush, xCenter - rectSize, yCenter - rectSize, rectSize * 2, rectSize * 2);
                        }
                    }
                }
            }
        }
    }

    private async void AnimerIndicateurTour()
    {
        if (labelStatus == null || joueurActuel == null) return;

        Color original = labelStatus.BackColor;
        Color highlight = joueurActuel.Couleur == Couleur.Rouge ? Color.FromArgb(255, 230, 230) : Color.FromArgb(230, 230, 255);

        try
        {
            for (int i = 0; i < 2; i++)
            {
                labelStatus.BackColor = highlight;
                await Task.Delay(300);
                labelStatus.BackColor = original;
                await Task.Delay(150);
            }
        }
        catch
        {
            // ignore si la fenêtre est en train de se fermer
        }
    }

    private void PanelCanonGauche_Paint(object? sender, PaintEventArgs e)
    {
        if (canonGauche == null) return;

        int yCenter = (canonGauche.PositionLigne * 600) / taillePlateau;
        int rayon = 8;
        
        // Dessiner le canon gauche en rouge
        using (Brush brush = new SolidBrush(Color.Red))
        {
            e.Graphics.FillEllipse(brush, 20 - rayon, yCenter - rayon, rayon * 2, rayon * 2);
        }
        
        // Dessiner "A" 
        using (Font font = new Font("Arial", 8, FontStyle.Bold))
        using (Brush textBrush = new SolidBrush(Color.White))
        {
            e.Graphics.DrawString("A", font, textBrush, 15, yCenter - 6);
        }
    }

    private void PanelCanonDroit_Paint(object? sender, PaintEventArgs e)
    {
        if (canonDroit == null) return;

        int yCenter = (canonDroit.PositionLigne * 600) / taillePlateau;
        int rayon = 8;
        
        // Dessiner le canon droit en bleu (cercle creux)
        using (Pen pen = new Pen(Color.Blue, 2))
        {
            e.Graphics.DrawEllipse(pen, 20 - rayon, yCenter - rayon, rayon * 2, rayon * 2);
        }
        
        // Dessiner "B"
        using (Font font = new Font("Arial", 8, FontStyle.Bold))
        using (Brush textBrush = new SolidBrush(Color.Blue))
        {
            e.Graphics.DrawString("B", font, textBrush, 15, yCenter - 6);
        }
    }

    // Gestion du drag-and-drop pour le canon gauche
    private void PanelLeft_MouseDown(object? sender, MouseEventArgs e)
    {
        isCanonGaucheDragging = true;
        draggingStartY = e.Y;
    }

    private void PanelLeft_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!isCanonGaucheDragging) return;

        // Convertir position Y en ligne (0 à taillePlateau)
        int newLigne = (e.Y * taillePlateau) / 600;
        if (newLigne < 0) newLigne = 0;
        if (newLigne > taillePlateau) newLigne = taillePlateau;

        canonGauche.PositionLigne = newLigne;
        if (panelLeft != null) panelLeft.Invalidate();
    }

    private void PanelLeft_MouseUp(object? sender, MouseEventArgs e)
    {
        isCanonGaucheDragging = false;
        MettreAJourAffichage();
    }

    // Gestion du drag-and-drop pour le canon droit
    private void PanelRight_MouseDown(object? sender, MouseEventArgs e)
    {
        isCanonDroitDragging = true;
        draggingStartY = e.Y;
    }

    private void PanelRight_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!isCanonDroitDragging) return;

        // Convertir position Y en ligne (0 à taillePlateau)
        int newLigne = (e.Y * taillePlateau) / 600;
        if (newLigne < 0) newLigne = 0;
        if (newLigne > taillePlateau) newLigne = taillePlateau;

        canonDroit.PositionLigne = newLigne;
        if (panelRight != null) panelRight.Invalidate();
    }

    private void PanelRight_MouseUp(object? sender, MouseEventArgs e)
    {
        isCanonDroitDragging = false;
        MettreAJourAffichage();
    }


    // Initialiser une démonstration avec des points placés en diagonale
    private void InitialiserDemonstration()
    {
        // Pas de points placés par défaut au démarrage
        // Les joueurs placent leurs propres points
    }

    private async void AnimerBoutonsChoix()
    {
        if (buttonChoisirTirer == null || buttonChoisirPlacer == null) return;

        Color origTirer = buttonChoisirTirer.BackColor;
        Color origPlacer = buttonChoisirPlacer.BackColor;

        try
        {
            for (int i = 0; i < 4; i++)
            {
                buttonChoisirTirer.BackColor = Color.Yellow;
                buttonChoisirPlacer.BackColor = Color.Yellow;
                await Task.Delay(220);
                buttonChoisirTirer.BackColor = origTirer;
                buttonChoisirPlacer.BackColor = origPlacer;
                await Task.Delay(180);
            }
        }
        catch
        {
            // ignore si la fenêtre se ferme
        }
    }

    // Le bouton de déplacement manuel a été supprimé : déplacement via boutons latéraux.

private async Task AnimerTir(int ligne, int colonne, CoteCanon cote)
{
    // Sécurité : vérifier bornes avant d'accéder au tableau
    if (ligne < 0 || ligne > taillePlateau || colonne < 0 || colonne > taillePlateau)
    {
        Log($"AnimerTir: indices hors bornes ({ligne},{colonne}) taille={taillePlateau}");
        return;
    }

    Button btnCible = boutonsGrille[ligne, colonne];
    Color originalColor = btnCible.BackColor;

    // Effet de flash
    btnCible.BackColor = Color.Yellow;
    await Task.Delay(100);
    btnCible.BackColor = Color.Red;
    await Task.Delay(100);
    btnCible.BackColor = originalColor;
}

    // Click handlers pour déplacer directement les canons en cliquant sur les boutons latéraux
    private void BoutonCanonGauche_Click(object? sender, EventArgs e)
    {
        if (sender is not Button b) return;
        if (b.Tag is not int ligne) return;
        canonGauche.Deplacer(ligne, taillePlateau);
        MettreAJourAffichage();
    }

    private void BoutonCanonDroit_Click(object? sender, EventArgs e)
    {
        if (sender is not Button b) return;
        if (b.Tag is not int ligne) return;
        canonDroit.Deplacer(ligne, taillePlateau);
        MettreAJourAffichage();
    }

    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.Control) return;

        int digit = -1;
        if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            digit = e.KeyCode - Keys.D0;
        else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            digit = e.KeyCode - Keys.NumPad0;

        if (digit >= 1 && digit <= 9)
        {
            puissanceCourante = digit;
            if (labelStatus != null && joueurActuel != null)
                labelStatus.Text = $"{joueurActuel.Nom} — Puissance réglée à {digit}";

            // Si le joueur a choisi TIRER (enPhaseCanon) et n'a pas choisi PLACER exclusivement,
            // lancer le tir directement. Ne pas auto-tirer si on attend encore le choix.
            if (!choixExclusifPlacer && !attenteChoix && enPhaseCanon)
            {
                try
                {
                    ButtonTirer_Click(this, EventArgs.Empty);
                }
                catch
                {
                    // ignorer
                }
            }

            e.Handled = true;
        }
    }

    // Click handlers pour déplacer directement les canons en cliquant sur les boutons latéraux
    

private void Sauvegarder_Click(object? sender, EventArgs e)
{
    string nomPartie = Interaction.InputBox("Nom de la partie :", "Sauvegarder", $"Partie_{DateTime.Now:HHmmss}");
    if (string.IsNullOrWhiteSpace(nomPartie)) return;
    
    gameService.SauvegarderPartie(
        nomPartie, 
        ReferenceEquals(joueurActuel, joueurA), 
        plateau, 
        canonGauche, 
        canonDroit, 
        joueurA.Score, 
        joueurB.Score, 
        puissanceCourante,
        puissanceCourante
    );
}

private void Regles_Click(object? sender, EventArgs e)
{
    string regles = @"RÈGLES DE POINT+CANON

Objectif : Aligner 5 points de sa couleur
- Horizontalement
- Verticalement
- En diagonale

Actions possibles à chaque tour :
1. Placer un point sur une case vide
2. Tirer au canon pour éliminer un point adverse

Le canon tire en arc de cercle :
Colonne = arrondi((Puissance × Taille) ÷ 9)

Les points alignés deviennent protégés et rapportent 1 point.";
    
    MessageBox.Show(regles, "Règles du jeu", 
        MessageBoxButtons.OK, MessageBoxIcon.Information);
}

private void NouvellePartie_Click(object? sender, EventArgs e)
{
    InitialiserJeu();
    MettreAJourAffichage();
    MessageBox.Show("Nouvelle partie démarrée.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
}

private void Charger_Click(object? sender, EventArgs e)
{
    try
    {
        var parties = gameService.ListeParties();
        if (parties.Count == 0)
        {
            MessageBox.Show("Aucune partie sauvegardée.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Créer un formulaire pour choisir la partie
        using (var form = new Form())
        {
            form.Text = "Charger une partie";
            form.Size = new Size(400, 300);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;

            var listBox = new ListBox();
            listBox.Dock = DockStyle.Top;
            listBox.Height = 200;
            foreach (var p in parties)
                listBox.Items.Add($"{p.NomPartie} ({p.DateSauvegarde:g})");
            form.Controls.Add(listBox);

            var btnCharger = new Button { Text = "Charger", Dock = DockStyle.Bottom, Height = 30 };
            var btnAnnuler = new Button { Text = "Annuler", Dock = DockStyle.Bottom, Height = 30 };
            form.Controls.Add(btnCharger);
            form.Controls.Add(btnAnnuler);

            btnCharger.Click += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    var gameState = parties[listBox.SelectedIndex];
                    ChargerEtatPartie(gameState);
                    form.DialogResult = DialogResult.OK;
                }
            };

            btnAnnuler.Click += (s, e) => form.DialogResult = DialogResult.Cancel;
            form.ShowDialog();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

private void ChargerEtatPartie(GameState gameState)
{
    try
    {
        // Créer nouveau plateau
        plateau = new Plateau(taillePlateau);
        
        // Restaurer les scores
        joueurA.Score = gameState.ScoreRouge;
        joueurB.Score = gameState.ScoreBleu;
        
        // Restaurer positions canons
        canonGauche.Deplacer(gameState.PositionCanonGauche, taillePlateau);
        canonDroit.Deplacer(gameState.PositionCanonDroit, taillePlateau);
        
        // Restaurer puissances
        puissanceCourante = gameState.PuissanceCanonGauche;
        
        // Restaurer la grille depuis JSON
        gameService.RestaurerGrille(gameState, plateau);
        
        MettreAJourAffichage();
        MessageBox.Show("Partie chargée avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Erreur restauration : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

    private void ChangerTaille_Click(object? sender, EventArgs e)
    {
        string input = Interaction.InputBox("Nouvelle taille du plateau (entier entre 5 et 30)", "Changer taille", taillePlateau.ToString());
        if (string.IsNullOrWhiteSpace(input)) return;
        if (!int.TryParse(input, out int newSize))
        {
            MessageBox.Show("Valeur invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (newSize < 5 || newSize > 30)
        {
            MessageBox.Show("Choisissez une taille entre 5 et 30.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ChangerTaillePlateau(newSize);
    }

    // Change la taille du plateau en préservant les points existants qui rentrent dans la nouvelle grille.
    private void ChangerTaillePlateau(int newSize)
    {
        if (newSize == taillePlateau) return;

        // Construire un nouveau plateau et copier les données existantes
        Plateau nouveau = new Plateau(newSize);
        int min = Math.Min(taillePlateau, newSize);
                for (int i = 0; i < min; i++)
                {
                    for (int j = 0; j < min; j++)
                    {
                        var ancienne = plateau.Grille[i, j];
                        if (ancienne.EstOccupee())
                        {
                            nouveau.Grille[i, j].Joueur = ancienne.Joueur;
                            if (ancienne.EstProtege) nouveau.Grille[i, j].Proteger();
                            nouveau.Grille[i, j].EstTouche = ancienne.EstTouche;
                        }
                    }
                }

        // Remplacer le plateau et mettre à jour la taille
        plateau = nouveau;
        taillePlateau = newSize;

        // Ajuster les canons pour rester dans les bornes
        if (canonGauche != null) canonGauche.Deplacer(Math.Min(canonGauche.PositionLigne, Math.Max(0, taillePlateau - 1)), taillePlateau);
        if (canonDroit != null) canonDroit.Deplacer(Math.Min(canonDroit.PositionLigne, Math.Max(0, taillePlateau - 1)), taillePlateau);

        // Reconstruire l'interface (recrée boutons et panels)
        this.Controls.Clear();
        ConfigurerInterface();
        MettreAJourAffichage();

        MessageBox.Show($"Taille du plateau changée à {newSize} (points hors bornes perdus).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // Simple logger qui écrit dans un fichier pour reproduire les séquences d'actions
    private void Log(string line)
    {
        try
        {
            string text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {line}" + Environment.NewLine;
            File.AppendAllText(debugLogPath, text, Encoding.UTF8);
        }
        catch
        {
            // ignore logging errors
        }
    }

} // class Form1

} // namespace point

