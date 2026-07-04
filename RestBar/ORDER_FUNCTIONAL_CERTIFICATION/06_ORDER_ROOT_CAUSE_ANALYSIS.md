# 06 — ORDER ROOT CAUSE ANALYSIS

## ORD-D01 — GetActiveOrder no cargaba órdenes

**Síntoma:** POS no detectaba orden activa al seleccionar mesa.  
**Causa raíz:** `OrderController.GetActiveOrder` enlazaba `[FromRoute(Name="id")]` pero `order-management.js` llama `/Order/GetActiveOrder?tableId=`.  
**Fix:** Cambiar a `[FromQuery] Guid tableId`.

## ORD-D02 — Fuga multitenant en mesas

**Síntoma:** Usuario veía mesas de otras sucursales.  
**Causa raíz:** `GetActiveTables` llamaba `GetActiveTablesAsync()` sin filtro.  
**Fix:** Usar `GetActiveTablesByCompanyAndBranchAsync` con claims CompanyId/BranchId.

## ORD-D03 — IDOR en operaciones POS

**Síntoma:** Empresa B podía intentar operaciones cross-tenant.  
**Causa raíz:** Sin validación branch en Cancel, GetActiveOrder, SetTableOccupied, SendToKitchen.  
**Fix:** Helpers `ValidateTableTenantAccessAsync` y `OrderBelongsToUserBranchAsync`.

## ORD-D06 — Cambio de mesa inexistente

**Síntoma:** Escenario 11 sin implementar.  
**Causa raíz:** No existía endpoint ni servicio.  
**Fix:** `OrderService.MoveOrderToTableAsync` + `POST /Order/MoveToTable`.

## Fallos en script de certificación (no defectos de app)

**Síntoma:** Pagos fallaban con "orden cancelada".  
**Causa raíz:** `Get-UsableTable` cancelaba TODAS las mesas incluyendo la orden bajo prueba.  
**Fix:** `Get-SecondaryTable` + reordenar escenario cancelación al final.
