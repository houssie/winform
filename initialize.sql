CREATE TABLE IF NOT EXISTS pointcanon_games (
    name TEXT PRIMARY KEY,
    state_json JSONB NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);