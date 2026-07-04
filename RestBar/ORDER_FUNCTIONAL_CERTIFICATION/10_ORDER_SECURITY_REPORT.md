# 10 — ORDER SECURITY REPORT

## Controles validados

| Control | Prueba | Resultado |
|---------|--------|-----------|
| IDOR orden ajena | S13-04 Cancel cross-tenant | 403 PASS |
| IDOR pago ajeno | S13-03 Payment summary B→A | 403 PASS |
| IDOR mesa ajena | S01-05, S13-02 GetActiveOrder | 403 PASS |
| Orden inexistente | S14-01 Cancel fake GUID | 404 PASS |
| Pago orden fake | S14-02 | 404 PASS |
| Mesas filtradas por tenant | S13-01 A=10, B=1 mesas | PASS |
| Branch Norte aislada | S13-05 N-01 visible | PASS |

## Endpoints endurecidos

- `GET /Order/GetActiveTables` — CompanyId + BranchId
- `GET /Order/GetActiveOrder` — ValidateTableTenantAccess
- `POST /Order/SetTableOccupied` — ValidateTableTenantAccess
- `POST /Order/SendToKitchen` — ValidateTableTenantAccess
- `POST /Order/Cancel` — OrderBelongsToUserBranch + 404
- `POST /Order/MoveToTable` — dual guard orden + mesa destino
- `GET /Order/GetOrderItems` — OrderBelongsToUserBranch
- `POST /api/Payment/partial` — BranchId guard (pre-existente, re-verificado)

## SignalR

- Hub `/orderHub` con `[Authorize]` (certificación app-wide previa)

## Tampering GUID (API)

Modificar OrderId/TableId/PersonId de otro tenant → **403/404** sin exposición de datos.

## Hallazgos abiertos

- Admin/superadmin sin BranchId tienen acceso global (by design)
- Descuentos client-side no validados en servidor (ORD-O01, severidad baja)
