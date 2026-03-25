# Installation Base de Données PostgreSQL

## Option 1 : Avec psql (ligne de commande)

### Windows :
```powershell
# Ouvrir PowerShell en tant qu'administrateur

# Se connecter à PostgreSQL (remplacez postgres par votre user si différent)
psql -U postgres

# Une fois connecté dans psql, exécuter le script :
\i 'C:/Users/itiel/Documents/progS4/winform/create_database.sql'

# Vérifier que la base est créée
\l
```

### Alternative PowerShell directe :
```powershell
# Depuis le dossier winform
psql -U postgres -f create_database.sql
```

---

## Option 2 : Avec pgAdmin (interface graphique)

1. Ouvrir **pgAdmin**
2. Se connecter à votre serveur PostgreSQL
3. Clic droit sur **Databases** → **Create** → **Database**
4. Nom : `point_game`
5. Clic sur **SQL** (en haut à droite) et coller le contenu de `create_database.sql`
6. Exécuter

---

## Option 3 : Avec DBeaver (si installé)

1. Créer nouvelle connexion PostgreSQL
2. Fichier → **Open SQL Script** → `create_database.sql`
3. Exécuter (F9 ou Ctrl+Enter)

---

## Vérification

Après exécution, se connecter à la base :
```sql
psql -U postgres -d point_game

-- Vérifier la table
\dt gamestate

-- Vérifier la structure
\d gamestate
```

### Résultat attendu :
```
                  Table « public.gamestate »
        Colonne         |            Type             
------------------------+-----------------------------
 id                     | integer                     
 nom_partie             | character varying(255)      
 date_creation          | timestamp without time zone 
 date_sauvegarde        | timestamp without time zone 
 etat_grille            | text                        
 score_rouge            | integer                     
 score_bleu             | integer                     
 position_canon_gauche  | integer                     
 position_canon_droit   | integer                     
 puissance_canon_gauche | integer                     
 puissance_canon_droit  | integer
```

---

## Troubleshooting

**Erreur "psql n'est pas reconnu"**
- Ajouter PostgreSQL au PATH Windows
- Ou utiliser le chemin complet : `C:\Program Files\PostgreSQL\16\bin\psql.exe`

**Erreur "role postgres does not exist"**
- Remplacer `postgres` par votre utilisateur PostgreSQL
- Exemple : `psql -U itiel`

**Erreur "password authentication failed"**
- Utiliser : `psql -U postgres -W` (demandra le mot de passe)

---

## ⚠️ Important

- La connexion dans le code C# utilise : `User Id=postgres;Password=postgres;`
- Adapter la chaîne de connexion dans `GameDbContext.cs` si vos identifiants sont différents
