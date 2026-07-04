# 05 — ORDER DEFECT LOG

## Defectos encontrados y resueltos

| ID | Severidad | Descripción | Estado |
|----|-----------|-------------|--------|
| ORD-D01 | **Alta** | `GetActiveOrder` usaba `[FromRoute]` pero JS envía `?tableId=` → Guid.Empty | **CORREGIDO** |
| ORD-D02 | **Alta** | `GetActiveTables` sin filtro CompanyId/BranchId | **CORREGIDO** |
| ORD-D03 | **Alta** | Sin guards IDOR en Cancel/GetActiveOrder/SetTableOccupied | **CORREGIDO** |
| ORD-D04 | **Media** | `updatePaymentInfo` no definida en JS | **CORREGIDO** |
| ORD-D05 | **Media** | `separate-accounts.js` no cargado en Index | **CORREGIDO** |
| ORD-D06 | **Media** | Cambio de mesa no implementado | **CORREGIDO** — `MoveToTable` |
| ORD-D07 | **Baja** | Scripts debug en producción Index | **CORREGIDO** — removidos |
| ORD-D08 | **Baja** | `SetTableOccupied` fallaba si mesa ya Ocupada | **CORREGIDO** — idempotente |
| ORD-D09 | **Baja** | Cancel orden inexistente devolvía 403 en vez de 404 | **CORREGIDO** |
| ORD-D10 | **Baja** | SendToKitchen aceptaba items vacíos | **CORREGIDO** |

## Defectos abiertos (no bloquean PASS)

| ID | Severidad | Descripción | Notas |
|----|-----------|-------------|-------|
| ORD-O01 | Baja | Descuentos solo client-side | Documentado; no afecta pagos servidor |
| ORD-O02 | Baja | UI item status PENDIENTE post-envío hasta refresh | Cosmético |
| ORD-O03 | Info | Carga 1000 órdenes no ejecutada | Fuera de alcance smoke |

## Defectos críticos/altos abiertos

**Ninguno.**
