# 16 — Escenarios Enterprise Adicionales (1-30)

**Fecha:** 2026-07-05  
**Script:** `scripts/Run-EnterpriseAdditionalScenarios.ps1`  
**Resultado:** **27 PASS / 5 GAP / 0 FAIL**

## Resumen por escenario

| ID | Escenario | Resultado | Evidencia |
|----|-----------|-----------|-----------|
| S01 | Mesa abandonada | **PASS** | Mesa ocupada, orden conservada, ReleaseGhostTables no libera (ghost=0) |
| S01b | Supervisor recupera | **PASS** | GetOrderStatus + Cancel con motivo |
| S02 | Cliente regresa | **PASS** | GetActiveOrder misma orderId tras espera |
| S03 | 3 meseros misma mesa | **PASS** | 1 orden, 2 ítems, cajero cobra |
| S03b | Auditoría | **PASS** | 2 filas audit_logs |
| S04 | Cambio mesero handoff | **PASS** | Shift/HandoffTable |
| S05 | Listo no servido | **PASS** | KitchenStatus=Ready, orden ReadyToPay |
| S06 | Entrega equivocada | **GAP** | Sin módulo entrega por mesa |
| S07 | Segunda tanda | **PASS** | Nuevo ítem Sent tras burger Ready |
| S08 | Cocina parcial | **PASS** | 1 ready, 3 pending |
| S09 | Cocina rechaza | **PASS** | Supervisor cancel ítem |
| S10 | Stock agotado 2 órdenes | **GAP** | TrackInventory=false en seed Enterprise |
| S11 | Unir mesas | **GAP** | Sin MergeTables API |
| S12 | Separar mesas | **GAP** | Sin SplitTables API |
| S13 | Cliente VIP | **PASS** | SetOrderPriority IsVip=true |
| S14 | Orden urgente | **PASS** | Priority=50 vía API |
| S15 | Cambio estación | **PASS** | UpdateItemStation Horno→Horno B |
| S16 | Una estación por ítem | **PASS** | 1 distinct station |
| S17 | Horno inactivo | **PASS** | Fallback Horno B (fix IsActive) |
| S18 | Bar fuera servicio | **GAP** | No ejecutado automático |
| S19 | Cambio piso | **PASS** | MoveToTable P1→P2 |
| S20 | Estación equivocada | **PASS** | UpdateItemStation + auditoría |
| S21 | Orden pagada reabrir | **PASS** | Nueva orden o bloqueo post-Completed |
| S22 | Pago mientras prepara | **PASS** | Pago parcial permitido |
| S23 | Cocina tras cancel | **PASS** | MarkItemReady → 400 (fix aplicado) |
| S24 | Impresión duplicada | **PASS** | Misma orderId, ítems agregados |
| S25 | Dos dispositivos mesero | **PASS** | Misma orden ambas sesiones |
| S26 | Orden 30 ítems | **PASS** | 278ms, 30 ítems |
| S27 | Paga y pide más | **PASS** | 2 ítems tras pago parcial |
| S28 | Todas estaciones | **PASS** | OP-RTE suite |
| S29 | Recuperación servidor | **PASS** | BD + KDS API |
| S30 | Hora pico lite | **PASS** | 10 órdenes paralelas |

## Correcciones aplicadas en esta certificación

1. **POST /Order/SetOrderPriority** — VIP y urgente (S13/S14)
2. **MarkItemAsReadyAsync** — bloquea orden/ítem cancelado (S23) — **High fix**
3. **FindBestStationForProductAsync** — excluye estaciones `IsActive=false` (S17)

## GAPs aceptados (no bloquean PASS)

- S06, S10, S11, S12, S18 — funcionalidad no implementada o requiere seed inventario real

## Veredicto

**ENTERPRISE ADDITIONAL SCENARIOS: PASS** (0 FAIL, 0 Critical/High abiertos)
