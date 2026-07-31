# 00 — MASTER BUSINESS RULES & AUTOMATION PLAN

**Programa:** RB-029 Enterprise Business Rules & Automation Engine  
**Fecha:** 2026-07-31  
**Copilot:** permanece OFF.  
**Flag:** `FeatureFlags:EnableBusinessRules`

## Principios

1. No migrar state machines / hash chains / invariantes de stock al motor.
2. Umbrales Bi/DI/FC/PO son candidatos MIGRATABLE vía plantillas.
3. Acciones destructivas (borrar stock, anular pagos) **prohibidas** en v1.
4. Toda ejecución auditable + idempotente (dedupe key).
5. Tenant obligatorio; Branch opcional.

## Fases

| Fase | Estado |
|------|--------|
| 1 Discovery | PASS |
| 2 Modelo | IMPLEMENTED |
| 3 Condiciones | IMPLEMENTED |
| 4 Acciones | IMPLEMENTED (safe set) |
| 5 Editor visual | IMPLEMENTED (flow JSON builder UI) |
| 6 Simulador | IMPLEMENTED (dry-run) |
| 7 Auditoría | IMPLEMENTED |
| 8 Plantillas | IMPLEMENTED |
| 9 Seguridad | IMPLEMENTED (RBAC + flag + MT scope) |
| 10 Certificación | PILOT READY |

## Arquitectura

```
Event / Manual / Schedule trigger
  → BusinessRulesEngine.Evaluate(facts)
  → ConditionEvaluator (AND/OR/NOT)
  → ActionDispatcher (alert, notification, recommendation, audit, task)
  → RuleExecution + RuleExecutionLog
```

## Veredicto objetivo

**PILOT READY** — no WORLD CLASS sin scheduler distribuido, webhooks firmados, y migración completa de BiInsightEngine a reglas publicadas.
