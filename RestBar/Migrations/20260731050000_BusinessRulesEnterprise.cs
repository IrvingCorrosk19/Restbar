using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations;

public partial class BusinessRulesEnterprise : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS br_rules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id uuid NOT NULL,
    branch_id uuid NULL,
    name varchar(120) NOT NULL,
    description varchar(500) NULL,
    category varchar(40) NOT NULL,
    priority int NOT NULL DEFAULT 100,
    status varchar(20) NOT NULL,
    current_version_number int NOT NULL DEFAULT 0,
    effective_from_utc timestamptz NULL,
    effective_to_utc timestamptz NULL,
    created_by_user_id uuid NOT NULL,
    approved_by_user_id uuid NULL,
    created_at_utc timestamptz NOT NULL DEFAULT now(),
    updated_at_utc timestamptz NOT NULL DEFAULT now(),
    require_approval_to_publish boolean NOT NULL DEFAULT false,
    template_code varchar(80) NULL
);
CREATE INDEX IF NOT EXISTS IX_br_rules_company_status ON br_rules (company_id, status, priority);

CREATE TABLE IF NOT EXISTS br_rule_versions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rule_id uuid NOT NULL,
    version_number int NOT NULL,
    root_logic varchar(10) NOT NULL,
    flow_json text NOT NULL,
    notes text NULL,
    created_by_user_id uuid NOT NULL,
    created_at_utc timestamptz NOT NULL DEFAULT now(),
    is_published boolean NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_br_rule_versions_unique ON br_rule_versions (rule_id, version_number);

CREATE TABLE IF NOT EXISTS br_rule_conditions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rule_version_id uuid NOT NULL,
    sort_order int NOT NULL,
    negate boolean NOT NULL DEFAULT false,
    field_key varchar(80) NOT NULL,
    operator varchar(20) NOT NULL,
    value_json varchar(500) NOT NULL
);

CREATE TABLE IF NOT EXISTS br_rule_actions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rule_version_id uuid NOT NULL,
    sort_order int NOT NULL,
    action_type varchar(40) NOT NULL,
    parameters_json text NOT NULL
);

CREATE TABLE IF NOT EXISTS br_rule_schedules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rule_id uuid NOT NULL,
    cron_or_interval varchar(40) NOT NULL,
    is_enabled boolean NOT NULL DEFAULT true,
    last_run_utc timestamptz NULL
);

CREATE TABLE IF NOT EXISTS br_rule_executions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rule_id uuid NOT NULL,
    rule_version_id uuid NOT NULL,
    company_id uuid NOT NULL,
    branch_id uuid NULL,
    mode varchar(20) NOT NULL,
    result varchar(20) NOT NULL,
    dedupe_key varchar(120) NOT NULL,
    facts_json text NOT NULL,
    error_message text NULL,
    duration_ms int NOT NULL,
    triggered_by_user_id uuid NULL,
    created_at_utc timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS IX_br_exec_dedupe ON br_rule_executions (company_id, dedupe_key);

CREATE TABLE IF NOT EXISTS br_rule_execution_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    execution_id uuid NOT NULL,
    step_type varchar(40) NOT NULL,
    message varchar(200) NOT NULL,
    detail_json text NULL,
    created_at_utc timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS br_rule_templates (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    code varchar(40) NOT NULL,
    name varchar(120) NOT NULL,
    description varchar(500) NULL,
    category varchar(40) NOT NULL,
    flow_json text NOT NULL,
    is_system boolean NOT NULL DEFAULT true
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_br_templates_code ON br_rule_templates (code);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP TABLE IF EXISTS br_rule_execution_logs;
DROP TABLE IF EXISTS br_rule_executions;
DROP TABLE IF EXISTS br_rule_schedules;
DROP TABLE IF EXISTS br_rule_actions;
DROP TABLE IF EXISTS br_rule_conditions;
DROP TABLE IF EXISTS br_rule_versions;
DROP TABLE IF EXISTS br_rule_templates;
DROP TABLE IF EXISTS br_rules;
");
    }
}
