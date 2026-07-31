# 06 — Dashboard Catalog

## Already native (do not rebuild)

| Dashboard | Evidence | Covers |
|-----------|----------|--------|
| Executive Command Center | `ExecutiveCommandCenterController` + `ExecutiveCommandCenterService` | Sales today, FC%, waste, cash, POs, alerts, scores |
| Food Cost Command | `FoodCostDashboardController` | FC%, menu engineering, waste |
| Procurement Command | `ProcurementDashboardController` | suppliers, open POs |
| Cash Session Dashboard | `CashSessionController.Dashboard` | live sessions |
| BI Nativo hub | `BiNativeController.Index` | SP-backed KPIs + hourly + top products |

## Target dashboards vs readiness

### Dashboard Ejecutivo
| Widget | Data ready? | Surface |
|--------|-------------|---------|
| Ventas | YES | CC + BiNative |
| Utilidad | PARTIAL (estimate) | `sp_profitability` |
| Food Cost | YES if snapshots | FoodCost + SP |
| Caja | YES | Cash + SP |
| Alertas | YES | `bi_alerts` / CC |
| Compras | YES | Procurement |
| Inventario | YES | Inventory health SP |
| Merma | YES | Top waste SP |
| Top productos | YES | Top products SP |

**Status:** Can be operated **today** via Command Center + BiNative (not a second duplicate UI).

### Dashboard Operativo
| Widget | Ready? | Gap |
|--------|--------|-----|
| Pedidos / mesas | YES | live ops screens |
| Cocina / estaciones | YES | KDS + station SP |
| Meseros | YES | waiter SP |
| Tiempos | YES if timestamps | was hardcoded 0 — fixed |

### Dashboard Financiero
| Widget | Ready? | Gap |
|--------|--------|-----|
| Caja / ingresos | YES | |
| Egresos (paid out) | YES | cash movements |
| Rentabilidad | PARTIAL | cost dualidad |
| Costos FC | PARTIAL | snapshot cadence |

### Dashboard Compras
| Widget | Ready? |
|--------|--------|
| Proveedores / OCs / recepciones | YES |
| Ahorros / variaciones precio | PARTIAL (`price_history`) |

### Dashboard Inventario
| Widget | Ready? | Gap |
|--------|--------|-----|
| Rotación / cobertura | PARTIAL | |
| Merma / críticos | YES | |
| Vencimientos | PARTIAL | only receipt line expiry |

## Decision

**Do not invent parallel Power-BI dashboards.** Extend Command Center + BiNative APIs. Full Phase-6 chrome is a productization track, not a readiness blocker.
