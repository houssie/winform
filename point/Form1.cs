using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

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

    private Button[,] boutonsGrille = null!;
    private Label labelStatus = null!;
    private Label labelScore = null!;
    private NumericUpDown numericPuissance = null!;
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
    
    
    private int taillePlateau = 12; 
    
    // Les contrôles de déplacement manuel via bouton ont été retirés :
    // déplacement se fait via les boutons latéraux ou clics dédiés.
    
    public Form1()
    {
        InitializeComponent();
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
        
        // Calcul taille case et panels
        int tailleCase = 600 / (taillePlateau + 1);

        // Panel gauche pour canons (hors plateau)
        Panel panelLeft = new Panel();
        panelLeft.Location = new Point(20, 20);
        panelLeft.Size = new Size(tailleCase, 600);
        panelLeft.BackColor = Color.Transparent;

        // Panel pour la grille (décalé à droite pour laisser la colonne des canons)
        Panel panelGrille = new Panel();
        panelGrille.Location = new Point(20 + tailleCase, 20);
        panelGrille.Size = new Size(600, 600);
        panelGrille.BackColor = Color.Beige;

        // Panel droit pour canons (hors plateau)
        Panel panelRight = new Panel();
        panelRight.Location = new Point(panelGrille.Left + panelGrille.Width, 20);
        panelRight.Size = new Size(tailleCase, 600);
        panelRight.BackColor = Color.Transparent;
        
        // Créer les boutons de la grille
        boutonsGrille = new Button[taillePlateau + 1, taillePlateau + 1];
        
        for (int i = 0; i <= taillePlateau; i++)
        {
            for (int j = 0; j <= taillePlateau; j++)
            {
                Button btn = new Button();
                btn.Location = new Point(j * tailleCase, i * tailleCase);
                btn.Size = new Size(tailleCase, tailleCase);
                btn.BackColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.Tag = new Point(i, j);
                btn.Click += BtnGrille_Click;
                
                panelGrille.Controls.Add(btn);
                boutonsGrille[i, j] = btn;
            }
        }

        // Créer boutons pour canons hors plateau
        boutonsCanonGauche = new Button[taillePlateau + 1];
        boutonsCanonDroit = new Button[taillePlateau + 1];
        for (int i = 0; i <= taillePlateau; i++)
        {
            Button bL = new Button();
            bL.Location = new Point(0, i * tailleCase);
            bL.Size = new Size(tailleCase, tailleCase);
            bL.FlatStyle = FlatStyle.Flat;
            bL.Tag = i; // stocke la ligne
            bL.Click += BoutonCanonGauche_Click;
            panelLeft.Controls.Add(bL);
            boutonsCanonGauche[i] = bL;

            Button bR = new Button();
            bR.Location = new Point(0, i * tailleCase);
            bR.Size = new Size(tailleCase, tailleCase);
            bR.FlatStyle = FlatStyle.Flat;
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
        
        // Puissance du tir
        Label labelPuissance = new Label();
        labelPuissance.Text = "Puissance (1-9):";
        labelPuissance.Location = new Point(10, 190);
        labelPuissance.Size = new Size(200, 25);
        panelInfo.Controls.Add(labelPuissance);
        
        numericPuissance = new NumericUpDown();
        numericPuissance.Location = new Point(10, 215);
        numericPuissance.Size = new Size(200, 25);
        numericPuissance.Minimum = 1;
        numericPuissance.Maximum = 9;
        numericPuissance.Value = 5;
        panelInfo.Controls.Add(numericPuissance);
        
        // Bouton tirer
        buttonTirer = new Button();
        buttonTirer.Text = "TIRER AU CANON";
        buttonTirer.Location = new Point(10, 250);
        buttonTirer.Size = new Size(200, 40);
        buttonTirer.BackColor = Color.Orange;
        buttonTirer.Click += ButtonTirer_Click;
        panelInfo.Controls.Add(buttonTirer);
        
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
    // Permettre au formulaire d'intercepter les touches (raccourcis clavier)
    this.KeyPreview = true;
    this.KeyDown += Form1_KeyDown;
    }
    
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
            MessageBox.Show("Vous êtes en phase canon : utilisez 'DÉPLACER LE CANON' ou 'TIRER AU CANON', puis appuyez sur 'FINIR LE TOUR' pour passer.", "Phase canon active", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    int puissance = (int)numericPuissance.Value;
        // Le canon utilisé est celui du joueur courant : A -> gauche, B -> droite
        CoteCanon cote = (joueurActuel == joueurA) ? CoteCanon.Gauche : CoteCanon.Droit;

    int ligneCanon = (cote == CoteCanon.Gauche) ? canonGauche.PositionLigne : canonDroit.PositionLigne;
    int rawCol = plateau.CalculerColonneCible(puissance);
    int colonneCible = (cote == CoteCanon.Droit) ? (plateau.Taille - rawCol) : rawCol;

    // Animation du tir (affiche la cible correcte selon le côté)
    await AnimerTir(ligneCanon, colonneCible, cote);
    
        // Effectuer le tir
        // Marquer qu'un tir a été effectué dans la phase (si on était en phase)
        tirEffectueDansPhase = true;
    if (plateau.TirerCanon(ligneCanon, puissance, cote, joueurActuel))
    {
        MettreAJourAffichage();
        // Après un tir, la phase canon (si active) se termine
        enPhaseCanon = false;
        // le bouton "Finir le tour" a été retiré.

            joueurActuel = (joueurActuel == joueurA) ? joueurB : joueurA;
            MettreAJourAffichage();

        // Vérifier si le tir a créé un alignement pour l'adversaire
        VerifierAlignements(joueurActuel);
        // Démarrer le tour suivant
        DemarrerTour();
    }
    else
    {
        MessageBox.Show("Tir sans effet ou position invalide!", "Information", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        // Même en cas de tir sans effet, on considère le tir comme consommé et on termine la phase
        enPhaseCanon = false;
        if (buttonFinirTour != null) buttonFinirTour.Enabled = false;
        joueurActuel = (joueurActuel == joueurA) ? joueurB : joueurA;
        MettreAJourAffichage();
        DemarrerTour();
    }
}

    private void DemarrerTour()
    {
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
        // Mettre à jour les boutons de la grille
        for (int i = 0; i <= taillePlateau; i++)
        {
            for (int j = 0; j <= taillePlateau; j++)
            {
                var cellule = plateau.Grille[i, j];
                Button btn = boutonsGrille[i, j];
                
                if (cellule.EstOccupee())
                {
                    var joueurCell = cellule.Joueur;
                    if (joueurCell != null && joueurCell.Couleur == Couleur.Rouge)
                    {
                        btn.Text = "●";
                        btn.ForeColor = Color.Red;
                        btn.Font = new Font("Arial", 12, FontStyle.Bold);
                    }
                    else if (joueurCell != null)
                    {
                        btn.Text = "○";
                        btn.ForeColor = Color.Blue;
                        btn.Font = new Font("Arial", 12, FontStyle.Bold);
                    }
                    
                    if (cellule.EstProtege)
                    {
                        btn.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        btn.BackColor = Color.White;
                    }
                }
                else
                {
                    btn.Text = "";
                    btn.BackColor = cellule.EstTouche ? Color.LightGray : Color.White;
                }
            }

        }

        // Dessiner les canons hors plateau sur les panels latéraux
        if (boutonsCanonGauche != null && canonGauche != null)
        {
            for (int k = 0; k <= taillePlateau; k++)
                boutonsCanonGauche[k].BackColor = Color.Transparent;

            int rG = canonGauche.PositionLigne;
            if (rG >= 0 && rG <= taillePlateau)
            {
                boutonsCanonGauche[rG].BackColor = Color.Orange;
                boutonsCanonGauche[rG].Text = "A";
                boutonsCanonGauche[rG].ForeColor = Color.Red;
                // s'assurer que les boutons latéraux sont cliquables pour déplacer le canon
                boutonsCanonGauche[rG].Enabled = true;
            }
        }

        if (boutonsCanonDroit != null && canonDroit != null)
        {
            for (int k = 0; k <= taillePlateau; k++)
                boutonsCanonDroit[k].BackColor = Color.Transparent;

            int rD = canonDroit.PositionLigne;
            if (rD >= 0 && rD <= taillePlateau)
            {
                boutonsCanonDroit[rD].BackColor = Color.Orange;
                boutonsCanonDroit[rD].Text = "B";
                boutonsCanonDroit[rD].ForeColor = Color.Blue;
                boutonsCanonDroit[rD].Enabled = true;
            }
        }

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
        if (numericPuissance != null) numericPuissance.Enabled = enPhaseCanon && !choixExclusifPlacer;

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
            if (numericPuissance != null)
            {
                numericPuissance.Value = digit;
                if (labelStatus != null && joueurActuel != null)
                    labelStatus.Text = $"{joueurActuel.Nom} — Puissance réglée à {digit}";
            }
            // Si le joueur n'a pas choisi PLACER exclusivement, lancer le tir directement
            if (!choixExclusifPlacer)
            {
                try
                {
                    // Appeler l'action de tir (async void handler) pour démarrer le tir
                    ButtonTirer_Click(this, EventArgs.Empty);
                }
                catch
                {
                    // ignorer les erreurs ici
                }
            }

            e.Handled = true;
        }
    }

    // Click handlers pour déplacer directement les canons en cliquant sur les boutons latéraux
    

private void Sauvegarder_Click(object? sender, EventArgs e)
{
    SaveFileDialog saveDialog = new SaveFileDialog();
    saveDialog.Filter = "Fichiers Point+Canon|*.pcs";
    saveDialog.Title = "Sauvegarder la partie";
    
    if (saveDialog.ShowDialog() == DialogResult.OK)
    {
        // Sauvegarder l'état du jeu
        using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
        {
            writer.WriteLine(taillePlateau);
            writer.WriteLine(joueurA!.Score);
            writer.WriteLine(joueurB!.Score);
            string current = ReferenceEquals(joueurActuel, joueurA) ? "A" : "B";
            writer.WriteLine(current);
            
            // Sauvegarder la grille
            for (int i = 0; i <= taillePlateau; i++)
            {
                for (int j = 0; j <= taillePlateau; j++)
                {
                    var cellule = plateau.Grille[i, j];
                    if (cellule.EstOccupee())
                    {
                        char joueur = cellule.Joueur == joueurA ? 'A' : 'B';
                        char protege = cellule.EstProtege ? 'P' : 'N';
                        writer.WriteLine($"{i},{j},{joueur},{protege}");
                    }
                }
            }
        }
        
        MessageBox.Show("Partie sauvegardée !", "Succès", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
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
    OpenFileDialog openDialog = new OpenFileDialog();
    openDialog.Filter = "Fichiers Point+Canon|*.pcs";
    openDialog.Title = "Charger une partie";

    if (openDialog.ShowDialog() == DialogResult.OK)
    {
        try
        {
            var lines = File.ReadAllLines(openDialog.FileName);

            if (lines.Length < 4)
            {
                MessageBox.Show("Fichier de sauvegarde invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int savedTaille = int.Parse(lines[0]);

            // Si la taille diffère, on reconstruit automatiquement le plateau et l'interface
            if (savedTaille != taillePlateau)
            {
                taillePlateau = savedTaille;
                // Recréer le plateau de la bonne taille (les cellules seront remplies ensuite)
                plateau = new Plateau(taillePlateau);

                // Ajuster positions des canons si nécessaire
                if (canonGauche != null) canonGauche.Deplacer(Math.Min(canonGauche.PositionLigne, taillePlateau), taillePlateau);
                if (canonDroit != null) canonDroit.Deplacer(Math.Min(canonDroit.PositionLigne, taillePlateau), taillePlateau);

                // Reconstruire l'interface : supprimer les contrôles actuels et recréer
                this.Controls.Clear();
                ConfigurerInterface();
            }

            // Scores et joueur courant
            joueurA.Score = int.Parse(lines[1]);
            joueurB.Score = int.Parse(lines[2]);
            joueurActuel = lines[3].Trim() == "A" ? joueurA : joueurB;

            // Si plateau non créé (tailles identiques), on le crée maintenant
            if (plateau == null || plateau.Taille != taillePlateau)
                plateau = new Plateau(taillePlateau);

            // Appliquer les points sauvegardés
            for (int k = 4; k < lines.Length; k++)
            {
                var parts = lines[k].Split(',');
                if (parts.Length < 4) continue;

                if (!int.TryParse(parts[0], out int li)) continue;
                if (!int.TryParse(parts[1], out int co)) continue;

                char joueurChar = parts[2].Length > 0 ? parts[2][0] : '?';
                char protegeChar = parts[3].Length > 0 ? parts[3][0] : 'N';

                if (!plateau.EstPositionValide(li, co)) continue;

                plateau.Grille[li, co].Joueur = joueurChar == 'A' ? joueurA : joueurB;
                plateau.Grille[li, co].EstTouche = false;
                plateau.Grille[li, co].EstProtege = protegeChar == 'P';
            }

            MettreAJourAffichage();
            MessageBox.Show("Partie chargée avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

} // class Form1

} // namespace point
}