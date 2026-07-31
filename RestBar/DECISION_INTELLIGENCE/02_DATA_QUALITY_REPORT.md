# 02 — Data Quality Report

**Estado:** VALIDATED · **Fecha:** 2026-07-30  
**Regla:** no auto-corregir histórico; DI debe mostrar advertencia si score &lt; 70.

## Scores por dominio (0–100)

Metodología: penalizar nulos críticos, huérfanos, costos faltantes, timestamps inconsistentes, ausencia de snapshots. Scores son **estimaciones de diseño** basadas en auditoría de modelo + limitaciones RB-025; re-medir en cada tenant productivo con SQL de evidencia.

| Dominio | Score | Notas | ¿Forecast permitido? |
|---------|------:|-------|----------------------|
| Ventas (órdenes Completed) | **82** | Estructura sólida; cuidado discount vs total | Sí con advertencia menor |
| Caja | **78** | Depende EnableCashModule + cierres | Sí para riesgo; no cash burn sin datos |
| Inventario | **65** | Costos coalesce; sin conteo físico | Cobertura sí; exactitud no |
| Compras | **72** | Scores OTIF pueden estar vacíos | Reorder sí |
| Food Cost | **58** | Snapshots no siempre generados | Solo con badge limitation |
| Personal | **45** | Shifts sin wage; no labor% | Solo volumen; **no** costo laboral |
| Cocina | **55** | Timestamps parciales | Prep time con limitaciones |
| Clientes | **40** | Entity básica; sin RFM listo | DEFERRED |
| Analytics layer | **85** | SPs + tenant filters | Base OK |

**Score global ponderado (diseño):** **68 / 100** → **PASS WITH WARNING** en Cockpit.

## Controles detectables (SQL sugerido — ejecutar por tenant)

```sql
-- Pedidos sin branch
SELECT count(*) FROM orders WHERE branch_id IS NULL;
-- Costos producto nulos en track inventory
SELECT count(*) FROM products WHERE track_inventory AND COALESCE(cost,0)=0 AND COALESCE(average_cost,0)=0;
-- Prep inconsistente
SELECT count(*) FROM order_items WHERE prepared_at IS NOT NULL AND sent_at IS NOT NULL AND prepared_at < sent_at;
-- Sesiones abiertas antiguas
SELECT count(*) FROM cash_sessions WHERE status = 'Open' AND opened_at < now() - interval '36 hours';
```

## Política DI

| Score dominio | UI |
|---------------|-----|
| ≥ 80 | Sin badge |
| 60–79 | “Datos con limitaciones” |
| &lt; 60 | “No usar para decisiones críticas / forecast confianza baja” |

No se construyen forecasts de Food Cost o Labor Cost como “alta confianza” con scores &lt; 60.
