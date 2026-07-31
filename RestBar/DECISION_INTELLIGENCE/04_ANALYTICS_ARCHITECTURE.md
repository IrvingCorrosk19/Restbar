# 04 — Analytics Architecture
**Estado:** DESIGNED + IMPLEMENTED (reuso)
Operational PG -> analytics views/SPs (RB-025) -> DecisionIntelligenceService -> ForecastEngine / RecommendationComposer -> API + Cockpit.
No DWH duplicado. Tablas nuevas: `di_decision_records`, `di_manual_events`, `di_forecast_runs`.
