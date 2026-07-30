# 01 — BUSINESS ANALYSIS

**Fecha:** 2026-07-29

## Estado RestBar
| Capacidad | Estado |
|-----------|--------|
| Recipe BOM + explosion venta | ✅ |
| WAC / LastCost (RB-020) | ✅ |
| Theoretical API | ✅ parcial |
| Recipe UI | ❌ |
| OrderItem cost snapshot | ❌ |
| Waste con costo | ❌ |
| Actual vs Theoretical variance | ❌ |
| Menu engineering BCG | ❌ |
| Food Cost % dashboard | ❌ parcial |

## Benchmarks (conceptos)
- **Theoretical** = recipe × sales mix (perfect portion)
- **Actual** = purchases + Δ inventory / usage valued
- **Variance** = Actual − Theoretical (target <1–2 pts)
- **Menu engineering** = popularity × contribution (Stars/Plowhorses/Puzzles/Dogs)

## ROI
Cerrar 1–2 pts de variance = impacto directo en margen. Menu engineering sube mix de Stars.

## Qué NO construir
Segundo Recipe, segundo inventario, motor de costo paralelo a RB-020.
