# 09 — Performance Report (RB-024)

**Fecha:** 2026-07-31

## Índices existentes

Migración `20260730023951_InventoryQueryPerformanceIndexes`:
- `products (company_id, is_active, track_inventory)`
- `product_stock_assignments` query indexes

## Patrones

| Área | Estado |
|------|--------|
| AsNoTracking en lecturas Inventory/GetLowStock / snapshot / movements | **SÍ** |
| Take(500) en GetMovementsByDateRange | **SÍ** |
| Take(20) recent movements snapshot | **SÍ** |
| Cache distribuido inventario | **NO** |
| Movimientos masivos batch API | **NO** |

## Riesgos

- Deduct receta N+1 (por línea Reduce+Log) — aceptable para tickets típicos; batch futuro P2.
- Valor inventario en snapshot carga productos activos en memoria — OK para cientos/miles bajos; particionar P2.

## Veredicto performance ciclo

**PASS WITH CONDITIONS** — índices y AsNoTracking presentes; sin estrés test de 10k SKUs en este ciclo.
