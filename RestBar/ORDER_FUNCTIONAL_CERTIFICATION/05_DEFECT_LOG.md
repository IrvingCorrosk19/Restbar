# 05 — DEFECT LOG

## Defectos cerrados (certificación 2026-07-04)

| ID | Sev | Descripción | Fix |
|----|-----|-------------|-----|
| ORD-D01 | Alta | GetActiveOrder `[FromRoute]` vs `?tableId=` | `[FromQuery]` |
| ORD-D02 | Alta | GetActiveTables sin filtro tenant | CompanyId+BranchId |
| ORD-D03 | Alta | IDOR Cancel/GetActiveOrder/SendToKitchen | `ValidateTableTenantAccessAsync` |
| ORD-D04 | Media | `updatePaymentInfo` ausente en JS | `payments.js` |
| ORD-D05 | Media | `separate-accounts.js` no cargado | Index.cshtml |
| ORD-D06 | Media | Cambio mesa no implementado | `MoveToTable` |
| ORD-D07 | Baja | Scripts debug en Index | Removidos |
| ORD-D08 | Baja | SetTableOccupied no idempotente | OrderService |
| ORD-D09 | Baja | Cancel 404 vs 403 | OrderController |
| ORD-D10 | Baja | SendToKitchen items vacíos | Validación controller |
| ORD-D11 | Alta | `PrinterName` EF sin mapear → mesero 0 mesas | `printer_name` column |
| ORD-D12 | Alta | `DiscountAmount` EF sin mapear | snake_case mapping |
| ORD-D13 | Alta | Órdenes fantasma (items cancelados) | Filtro items activos |
| ORD-D14 | Media | GetOrderStatus sin estación | Include PreparedByStation |
| ORD-D15 | Media | Recipe Save concurrency 0 rows | RemoveRange + Add explícito |
| ORD-D16 | Media | POS-02 cert mesa ocupada | Reset mesa + Test-TableFree |
| ORD-D18 | Alta | Reset SQL branch names incorrectos | `Cert-Common.ps1` reset global |

## Defectos abiertos

| ID | Sev | Descripción | Impacto |
|----|-----|-------------|---------|
| ORD-O01 | Media | Impresión térmica sin endpoint | Operación manual KDS |
| ORD-O02 | Media | Pasarela tarjeta no integrada | Solo efectivo/API manual |
| ORD-O03 | Baja | Load test 500+ no ejecutado | Riesgo escala hiperscale |
| ORD-O04 | Baja | SignalR offline E2E no probado | Recuperación parcial vía REST |

**Críticos/altos abiertos: 0**
