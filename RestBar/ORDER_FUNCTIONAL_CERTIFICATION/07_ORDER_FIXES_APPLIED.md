# 07 — ORDER FIXES APPLIED

## Backend

### `Controllers/OrderController.cs`
- Helpers multitenant: `GetUserBranchId`, `GetUserCompanyId`, `ValidateTableTenantAccessAsync`, `OrderBelongsToUserBranchAsync`
- `GetActiveTables` — filtro por CompanyId + BranchId
- `GetActiveOrder` — `[FromQuery] Guid tableId` + guard tenant
- `SetTableOccupied`, `SendToKitchen`, `Cancel`, `GetOrderItems` — guards IDOR
- `SendToKitchen` — rechaza items vacíos
- `Cancel` — 404 si orden no existe (antes 403)
- **Nuevo** `POST /Order/MoveToTable`

### `Services/OrderService.cs`
- `SetTableOccupiedAsync` — idempotente si mesa ya Ocupada
- **Nuevo** `MoveOrderToTableAsync` — valida branch, mesa destino, libera mesa anterior, SignalR

### `Interfaces/IOrderService.cs`
- Firma `MoveOrderToTableAsync`

## Frontend

### `wwwroot/js/order/payments.js`
- **Nuevo** `updatePaymentInfo()` — sincroniza `#totalPaid` y `#remainingAmount` con `/api/Payment/order/{id}/summary`

### `Views/Order/Index.cshtml`
- Carga `separate-accounts.js` (cuentas separadas completas)
- Eliminados scripts debug: test-loading, debug-signalr, test-multi-screen, test-discount

## Build

```
dotnet build — 0 errores (post-fixes)
```
