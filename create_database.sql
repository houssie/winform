-- Script de création de la base de données PostgreSQL pour Point+Canon

-- Créer la base de données
CREATE DATABASE point_game
    WITH
    ENCODING = 'UTF8'
    LOCALE_PROVIDER = 'libc'
    LOCALE = 'fr_FR.UTF-8'
    TEMPLATE = template0;

-- Se connecter à la base point_game
\c point_game

-- Créer la table gamestate
CREATE TABLE gamestate (
    id SERIAL PRIMARY KEY,
    nom_partie VARCHAR(255) NOT NULL,
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_sauvegarde TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    etat_grille TEXT,
    score_rouge INTEGER DEFAULT 0,
    score_bleu INTEGER DEFAULT 0,
    position_canon_gauche INTEGER DEFAULT 0,
    position_canon_droit INTEGER DEFAULT 0,
    puissance_canon_gauche INTEGER DEFAULT 5,
    puissance_canon_droit INTEGER DEFAULT 5
);

-- Créer un index sur nom_partie et date_sauvegarde pour les requêtes
CREATE INDEX idx_gamestate_nom ON gamestate(nom_partie);
CREATE INDEX idx_gamestate_date ON gamestate(date_sauvegarde DESC);

-- Afficher confirmation
\dt gamestate

drop table gamestate;
