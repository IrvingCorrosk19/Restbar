# 06 — Análisis multibodega (Station-as-warehouse)

**Fecha:** 2026-07-30

---

## 1. Estado actual (evidencia)

| Hecho | Detalle |
|-------|---------|
| Entidad Warehouse | **No existe** |
| Stock por ubicación | `ProductStockAssignment` con `Station` como location |
| Stock global | `Product.Stock` |
| Transferencias | Station-to-station (Request / Approve / Reject) |
| Dispatch / Receive | No implementados |
| Tipos de estación | Actúan como bodegas operativas (cocina / barra) |

---

## 2. Implicación arquitectónica

Hoy la “bodega” es un **rol de Station**, no un agregado aparte. Multibodega de facto = múltiples estaciones con stock asignado.

| Pros (hecho / diseño) | Contras (hecho) |
|-----------------------|-----------------|
| Sin migración a nueva entidad aún | Semántica cocina/barra mezclada con almacén |
| Transferencias ya modeladas entre stations | Sin despacho/recepción formal |
| Assignments ya existen | Sin costo por ubicación |

---

## 3. Recomendación de diseño (alineada al audit)

**Mapear `Station` → rol warehouse sin crear entidad `Warehouse` todavía.**

| Paso | Recomendación |
|------|---------------|
| 1 | Clasificar/usar tipos de Station como bodegas (kitchen/bar/… ) |
| 2 | Mantener `ProductStockAssignment` como fuente de stock por ubicación |
| 3 | Tratar transferencias station-to-station como movimientos inter-bodega |
| 4 | Diferir entidad Warehouse hasta que dispatch/receive, conteos o costo por ubicación lo exijan |
| 5 | `GetEnterpriseSnapshot` como vista consolidada sobre stations + assignments |

---

## 4. Gaps relacionados

| Gap | Prioridad sugerida (gap analysis) |
|-----|-----------------------------------|
| Conteos por ubicación | P0 |
| Dispatch / receive | P0 |
| Costo por ubicación | P1 |
| Entidad Warehouse explícita | P2 (diferir) |
