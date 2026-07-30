# 04 — DATABASE DESIGN

## executive_snapshots
id, company_id, branch_id, period_type (Today/Day/Week), snapshot_json, enterprise_score, generated_at

## bi_insights
id, company_id, branch_id, insight_type, severity, title, explanation, recommended_action, entity_type?, entity_id?, is_dismissed, created_at

## bi_alerts
id, company_id, branch_id, alert_code, severity, message, source_module, is_resolved, created_at

## bi_scores
id, company_id, branch_id?, subject_type (Branch/Supplier/Product), subject_id, score, dimensions_json, computed_at

## bi_audit_events
id, company_id, branch_id, actor_user_id, query_name, filters_json, duration_ms, ip?, event_hash, created_at_utc

## forecast_seeds
id, company_id, branch_id, metric_code, as_of_date, value, source, created_at  
*(sin predicción ML — solo seed histórico)*

Índices: (branch_id, created_at) en insights/alerts; (company_id, subject_type, subject_id) en scores.
