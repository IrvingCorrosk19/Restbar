-- RB-028 Decision Intelligence tables (idempotent)
CREATE TABLE IF NOT EXISTS di_decision_records (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id uuid NOT NULL,
    branch_id uuid NULL,
    recommendation_code varchar(40) NOT NULL,
    category varchar(40) NOT NULL,
    observation varchar(2000) NOT NULL,
    evidence varchar(2000) NOT NULL,
    recommended_action varchar(1000) NOT NULL,
    expected_impact varchar(1000) NULL,
    expected_impact_value numeric(18,4) NULL,
    actual_impact_value numeric(18,4) NULL,
    status varchar(20) NOT NULL,
    created_by_user_id uuid NOT NULL,
    assigned_to_user_id uuid NULL,
    comment varchar(1000) NULL,
    created_at_utc timestamptz NOT NULL DEFAULT now(),
    updated_at_utc timestamptz NULL,
    due_at_utc timestamptz NULL,
    verified_at_utc timestamptz NULL
);
CREATE INDEX IF NOT EXISTS IX_di_decisions_company_status ON di_decision_records (company_id, status, created_at_utc);

CREATE TABLE IF NOT EXISTS di_manual_events (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id uuid NOT NULL,
    branch_id uuid NULL,
    event_date date NOT NULL,
    event_type varchar(40) NOT NULL,
    title varchar(200) NOT NULL,
    notes varchar(500) NULL,
    created_by_user_id uuid NOT NULL,
    created_at_utc timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS IX_di_manual_events_date ON di_manual_events (company_id, event_date);

CREATE TABLE IF NOT EXISTS di_forecast_runs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id uuid NOT NULL,
    branch_id uuid NULL,
    metric_code varchar(40) NOT NULL,
    model_id varchar(40) NOT NULL,
    horizon_days int NOT NULL,
    history_points int NOT NULL,
    mae numeric(18,4) NULL,
    mape numeric(18,4) NULL,
    rmse numeric(18,4) NULL,
    beats_naive boolean NOT NULL DEFAULT false,
    confidence varchar(20) NOT NULL,
    forecast_json text NOT NULL DEFAULT '[]',
    created_at_utc timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS IX_di_forecast_runs_company ON di_forecast_runs (company_id, branch_id, created_at_utc);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260731040000_DecisionIntelligenceEnterprise', '9.0.5')
ON CONFLICT DO NOTHING;
