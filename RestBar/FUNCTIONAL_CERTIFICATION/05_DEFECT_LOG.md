# Registro de Defectos

**Estado final:** Sin defectos abiertos  
**Última actualización:** 2026-07-04

## Defectos encontrados y cerrados

### DEF-001 — Rate limiter bloquea certificación
- **Severidad:** Alta
- **Síntoma:** HTTP 429 en logins masivos
- **Causa:** Límite 5 req/min en Development
- **Fix:** `Program.cs` — 500 req/min en Development
- **Estado:** ✅ Cerrado

### DEF-002 — Login chef/bartender redirect 404
- **Severidad:** Crítica
- **Síntoma:** Redirect a `StationOrders` inexistente como controller
- **Fix:** `AuthController` redirect a `Order/StationOrders?stationType=`
- **Estado:** ✅ Cerrado

### DEF-003 — Audit Index HTTP 400
- **Severidad:** Alta
- **Síntoma:** Admin no puede abrir auditoría
- **Causa:** Parámetro `action` reservado en MVC + ViewModel duplicado
- **Fix:** `actionFilter` + unificación ViewModel
- **Estado:** ✅ Cerrado

### DEF-004 — GetProductsByCategory retorna vacío
- **Severidad:** Crítica
- **Síntoma:** POS sin productos; categoryId=Guid.Empty
- **Causa:** Ruta `{id?}` no enlazaba parámetro `categoryId`
- **Fix:** `[FromRoute(Name="id")]`
- **Estado:** ✅ Cerrado

### DEF-005 — Chef bloqueado en KDS
- **Severidad:** Alta
- **Síntoma:** AccessDenied en `/Order/StationOrders`
- **Causa:** Política `OrderAccess` sin chef/bartender
- **Fix:** `Program.cs` + `[Authorize(KitchenAccess)]`
- **Estado:** ✅ Cerrado

### DEF-006 — Pagos: columna IdempotencyKey
- **Severidad:** Crítica
- **Síntoma:** `column p.IdempotencyKey does not exist`
- **Causa:** EF sin mapeo a `idempotency_key`
- **Fix:** `RestBarContext` + SQL hardening
- **Estado:** ✅ Cerrado

### DEF-007 — Pagos: columna branch_id
- **Severidad:** Alta
- **Síntoma:** `column p.branch_id does not exist`
- **Causa:** Mapeo incorrecto; DB usa `BranchId` PascalCase
- **Fix:** `HasColumnName("BranchId")`
- **Estado:** ✅ Cerrado

### DEF-008 — IDOR resumen de pago cross-tenant
- **Severidad:** Crítica
- **Síntoma:** HTTP 500 al consultar orden ajena
- **Fix:** Guard branch en `GetOrderPaymentSummary` → 403
- **Estado:** ✅ Cerrado

### DEF-009 — Órdenes duplicadas por mesa
- **Severidad:** Alta
- **Síntoma:** 7 órdenes activas duplicadas
- **Fix:** `repair-duplicate-orders.sql` + índice único
- **Estado:** ✅ Cerrado

### DEF-010 — Tests SEC con lógica invertida
- **Severidad:** Media (falso negativo en script)
- **Fix:** `Run-FullCertification.ps1` — eliminar `-not` incorrecto
- **Estado:** ✅ Cerrado

### DEF-011 — MT-01 falso negativo PowerShell
- **Severidad:** Media
- **Causa:** `.Count` en array de 1 elemento retorna `$null`
- **Fix:** `@(...).Count` + `$_.'name'`
- **Estado:** ✅ Cerrado

## CSV de defectos activos

`05_DEFECT_LOG.csv` — **vacío** tras retest final
