# 📜 POINT+CANON — RÈGLES OFFICIELLES

---

## 1. PRÉSENTATION

**Point+Canon** est un jeu de stratégie pour **2 joueurs** sur un plateau carré. Chaque joueur place ses points et peut utiliser un canon à tir courbé pour éliminer les points adverses. Le but est d'aligner 5 points pour marquer des points.

---

## 2. MATÉRIEL

- **Plateau** : carré de **n×n carreaux**
  - Soit (n+1) lignes horizontales
  - Soit (n+1) lignes verticales
  - Soit (n+1)² intersections où placer des points
- **2 joueurs** : 
  - Joueur A (●)
  - Joueur B (○)
- **2 canons** (1 par joueur) placés sur les bords verticaux du plateau

---

## 3. BUT DU JEU

- Aligner **5 points** de sa couleur :
  - Horizontalement
  - Verticalement
  - En diagonale
- Dès qu'un joueur réalise un alignement de 5 points, ils sont **automatiquement tracés** (reliés) et le joueur marque **1 point**
- Le jeu continue jusqu'à ce que les joueurs décident d'arrêter (ou jusqu'à ce que le plateau soit plein)

---

## 4. DÉROULEMENT D'UN TOUR

Les joueurs jouent **à tour de rôle**.

Au **début de son tour**, chaque joueur doit choisir **UNE ET UNE SEULE** des deux actions suivantes :

### OPTION A : PLACER UN POINT
- Le joueur place **1 point** de sa couleur sur une intersection **vide** du plateau
- Son tour se termine **immédiatement** après ce placement

### OPTION B : TIRER AU CANON
- Le joueur utilise son canon pour éliminer **1 point adverse**
- Il doit :
  1. Choisir le canon **gauche** ou **droite**
  2. Positionner le canon sur une **ligne horizontale** (déplacement vertical sur le bord)
  3. Choisir une **puissance p** (de 1 à 9)
  4. Tirer **1 seule fois**
- Son tour se termine **immédiatement** après le tir

---

## 5. LE CANON

### 5.1 Positionnement
- Le canon est placé sur les **bords verticaux** du plateau :
  - **Canon gauche** : bord gauche, tire vers la droite
  - **Canon droit** : bord droit, tire vers la gauche
- À chaque tour, le joueur peut **déplacer verticalement** son canon sur n'importe quelle ligne horizontale

### 5.2 Puissance et trajectoire
- La puissance **p** est un nombre entier de **1 à 9**
- Le canon tire en **trajectoire courbée** (en arc)
- La puissance détermine la **colonne du point impacté** sur la même ligne horizontale :

**Formule :** `colonne ciblée = arrondi((p × n) ÷ 9)`

(L'arrondi se fait à l'entier le plus proche)

### 5.3 Conséquences du tir
- Le point adverse situé à l'intersection (ligne du canon, colonne ciblée) est **éliminé** et retiré du plateau
- Si aucun point adverse n'est présent à cet emplacement, le tir est **sans effet** (mais le tour est quand même passé)
- Certains points peuvent être **inaccessibles** à cause des arrondis de la formule

---

## 6. ALIGNEMENTS ET SCORE

### 6.1 Détection des alignements
- Dès qu'un joueur place un point (ou après un tir adverse), on vérifie s'il a **5 de ses points alignés** :
  - Horizontalement
  - Verticalement
  - En diagonale (dans les deux sens)

### 6.2 Traçage automatique
- Dès qu'un alignement de 5 points est détecté :
  - Les 5 points sont **automatiquement reliés** (tracés) sur le plateau
  - Le joueur marque **1 point**
  - Ces 5 points deviennent **intouchables** :
    - Ils ne peuvent plus être éliminés par le canon adverse
    - Ils ne peuvent pas être utilisés par l'adversaire

### 6.3 Alignements multiples
- Si un joueur réalise **plus de 5 points alignés** (ex: 6 d'affilée) :
  - Le traçage se fait automatiquement dès les 5 premiers
  - Cela compte pour **1 seul point**
  - Les 5 points tracés sont protégés, les éventuels points supplémentaires au-delà des 5 restent normaux (et peuvent encore être éliminés)

---

## 7. RÈGLES DE NON-CROISEMENT

### 7.1 Principe fondamental
**Une ligne tracée par un joueur ne peut pas être croisée par une ligne de l'adversaire.**

### 7.2 Conséquences
- ✅ Un joueur peut tracer des lignes qui **croisent ses propres lignes** (il est chez lui)
- ❌ Un joueur **ne peut pas** tracer une ligne qui croise une ligne déjà tracée par l'adversaire
- Comme les points tracés sont forcément aux intersections, cette règle est naturelle : l'adversaire ne peut pas placer ses points sur des intersections déjà occupées par les points tracés

### 7.3 Exemple
Si Joueur A a tracé une ligne horizontale sur la ligne 4 (colonnes 2 à 6) :
- Joueur B ne peut pas tracer une ligne verticale à la colonne 4 (car elle croiserait la ligne de A aux points déjà occupés)
- Joueur B peut tracer une ligne verticale à la colonne 1 (elle ne croise pas la ligne de A)

---

## 8. CAS PARTICULIERS

### 8.1 Plateau plein
Si toutes les intersections sont occupées sans qu'aucun joueur n'ait atteint le score convenu, la partie s'arrête et le joueur avec le plus de points gagne.

### 8.2 Tir sur case vide
Si un joueur tire au canon sur une intersection où il n'y a pas de point adverse, rien ne se passe mais son tour est tout de même terminé.

### 8.3 Choix du canon
Le joueur peut utiliser indifféremment le canon gauche ou droit à chaque tour où il choisit l'option tir.

---

## 9. STRATÉGIES (suggestions)

- **Protection** : Une fois une ligne tracée, les points qui la composent sont définitivement protégés
- **Blocage** : Le canon permet de cibler les points clés qui pourraient former un alignement adverse
- **Anticipation** : Il faut prévoir où l'adversaire peut placer ses futurs points et quels sont ses alignements potentiels
- **Économie** : Choisir entre placer un point (construction) ou tirer (destruction) est un choix stratégique crucial

---

## 10. VARIANTES POSSIBLES

- **Taille du plateau** : n peut être choisi (n=12, n=15, n=18...)
- **Score à atteindre** : Jouer en 3, 5 ou 7 points gagnants
- **Canon amélioré** : Permettre des tirs avec des puissances différentes selon la position
- **Protection temporaire** : Points protégés seulement pendant X tours

---

*Règles officielles v1.0 — Document créé le [Date]*
