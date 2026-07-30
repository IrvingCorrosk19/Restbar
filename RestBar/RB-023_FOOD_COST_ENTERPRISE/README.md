# RB-023 — FOOD COST & PROFITABILITY ENGINE

**Supreme Edition · Motor de rentabilidad**  
**Estado:** DISEÑO + IMPLEMENTACIÓN v1

## Regla suprema
No un porcentaje. Un **motor financiero** que gana margen, detecta desperdicio y fraude.

## Veredicto
**APTO E IMPLEMENTADO (flag off)** — ver `20_FINAL_CERTIFICATION.md`

## Docs 01–20
Business · Requirements · Domain · Database · Architecture · Cost · Recipe · Profitability · Variance · Waste · Menu Engineering · Simulation · Audit · Security · Command Center · Reports · KPIs · Tests · Implementation · Certification

## Integración
| Módulo | Relación |
|--------|----------|
| RB-020 Cost Engine | REUSAR WAC / Theoretical |
| Recipe / RecipeLine | EXTENDER (yield, waste%) |
| OrderItem | EXTENDER snapshot costo |
| InventoryMovement.Waste | EXTENDER UnitCost |
| RB-010 Cash | Intacta |
| POS / KDS | Hook opt-in flag |
