# 05 — Technical Audit

| Tema | Evaluación | Evidencia |
|------|------------|-----------|
| Arquitectura | Monolito modular ASP.NET 8 + PG | RB-026 |
| Capas | Controllers→Services→EF; sin repos universales | PASS WITH CONDITIONS |
| God services | OrderService / AdvancedReports grandes | Deuda P1 |
| DI | Factories `new` en Program | Deuda P2 |
| Deuda TODO | Reports exports, Email MinStock, etc. | P1–P2 |
| Coverage unit | **~0.41%** líneas (baseline RB-027) | Bloquea Engineering Excellence CERTIFIED |
| Integration tests | **0** harness | Hueco P0 calidad |
| CI | G1–G3 enforced; G4 browser condicional | RB-027 |
| Migraciones | EF + SQL analytics/DI/Rules | Ops debe aplicar SQL en VPS |
| Escalabilidad | No lab 5k tenants | No hiperescala |

**No se modifica arquitectura en este programa.**
