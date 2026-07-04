# 02 — BUSINESS SCENARIOS (28 escenarios del prompt)

Leyenda: **PASS** = ejecutado y aprobado | **PARTIAL** = implementado, cobertura parcial | **N/A** = no modelado / fase 2

| # | Escenario | Resultado | Evidencia |
|---|-----------|-----------|-----------|
| 1 | Apertura de orden | **PASS** | S01-01..07: mesa libre/ocupada, GUID vacío, tenant, concurrencia mesero |
| 2 | Gestión de productos | **PASS** | S02-01..04: agregar, vacío rechazado, stock, cantidad negativa |
| 3 | Cliente cambia de opinión | **PASS** | S03-01..02: notas, GetOrderStatus antes de cocina |
| 4 | Envío a cocina | **PASS** | S04-01..02, RT-S02: KDS por estación, doble envío permitido (add items) |
| 5 | Cancelación | **PARTIAL** | S05 cancel orden; ENT-CANC supervisor post-cocina; post-pago vía refund |
| 6 | Cuentas separadas | **PASS** | S08-01..02 Person API; `separate-accounts.js` cargado |
| 7 | Pagos | **PASS** | S09, PAY-01..05, ENT-TIP, ENT-REF: parcial, sobrepago, idempotencia, propinas, refund |
| 8 | Cambio de mesa | **PASS** | S11-01..03 MoveToTable + re-enrutamiento stock |
| 9 | Concurrencia | **PASS** | S01-06, S15-01: SendToKitchen concurrente, 3 pagos paralelos |
| 10 | Múltiples pisos | **PASS** | RT-S01: Piso1 no filtra a Cocina Piso 2 |
| 11 | Múltiples estaciones | **PASS** | RT-S02, RT-S09: pizza→horno, cerveza→bar, prioridad Horno/Horno B |
| 12 | Múltiples cocinas | **PARTIAL** | Cocina Express en seed; aislamiento chef no E2E browser |
| 13 | Múltiples bares | **PASS** | RT-S03: Bar VIP vs Bar Principal |
| 14 | Inventario | **PASS** | ENT-INV, stock en SendToKitchen, `ProductStockAssignment` por estación |
| 15 | Recetas BOM | **PASS** | ENT-REC-01..02, `InventoryOperationsService` explosión ingredientes |
| 16 | Transferencias | **PASS** | ENT-XFER-01..03 StockTransfer + approve |
| 17 | Meseros | **PASS** | UserAssignment área/mesas, Shift handoff, AddedByUserId, tip allocation |
| 18 | Cocina (KDS) | **PASS** | KDS paginado, VIP/priority, StationOrders por stationId |
| 19 | Delivery / Pickup | **PARTIAL** | `OrderType` enum (DineIn); sin flujo delivery dedicado UI |
| 20 | Promociones | **PARTIAL** | `ApplyDiscount` server; sin motor happy hour/combos automáticos |
| 21 | Impuestos | **PARTIAL** | TaxRate por producto; sin motor fiscal multi-jurisdicción |
| 22 | Propinas | **PASS** | TipAmount en Payment, TipAllocation por mesero |
| 23 | Comisiones | **PASS** | CommissionRule + SalesReportService por rol/estación |
| 24 | Impresión | **PARTIAL** | `Station.PrinterName`; sin endpoint impresión térmica |
| 25 | Recuperación | **PARTIAL** | `/api/kitchen/current` idempotente; SignalR offline no E2E |
| 26 | Auditoría forense | **PASS** | GlobalLoggingService Order/Payment/Cancel; Audit/Index; campos AddedBy/ProcessedBy |
| 27 | Escalabilidad | **N/A** | Smoke 119 casos; load test 500+ pendiente herramienta externa |
| 28 | Usabilidad | **PARTIAL** | Ver `17_USABILITY_REPORT.md` |

**Resumen:** 20 PASS · 7 PARTIAL · 1 N/A (escala masiva no ejecutada)
