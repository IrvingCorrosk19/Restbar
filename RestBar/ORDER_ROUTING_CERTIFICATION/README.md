# ORDER ROUTING CERTIFICATION

## Veredicto

# ORDER ROUTING CERTIFICATION: PASS

**15/15 pruebas API** — motor de enrutamiento validado para restaurante enterprise multi-piso/estación.

## Ejecutar

```powershell
dotnet run --urls http://localhost:5001
powershell -ExecutionPolicy Bypass -File ORDER_ROUTING_CERTIFICATION\scripts\Run-RoutingCertification.ps1
```

## Backup

`backups/RestBar_pre_routing_cert_20260704_140923.dump`

## Correcciones aplicadas

| Área | Cambio |
|------|--------|
| KDS `GetKitchenOrdersAsync` | Filtro por `BranchId` / `CompanyId` |
| `StationOrders` | Filtro por estación individual (`stationId`), área (`areaId`), tenant |
| Items sin estación | Ya no aparecen en todas las estaciones (sin fuga) |
| `FindBestStationForProductAsync` | Prioridad por `ProductStockAssignment.Priority` + preferencia por área de mesa |
| `SendToKitchen` | Bloquea productos sin estación configurada |
| `UpdateItemStation` | Nuevo endpoint re-enrutamiento con validación |
| `KitchenApiController` | Snapshot con filtros tenant + estación |
| `SeedEnterpriseRouting` | Multi-piso, multi-bar, multi-cocina, asignaciones producto→estación |

## Escenarios probados

| # | Escenario | Resultado |
|---|-----------|-----------|
| 1 | Multi-piso — Piso 1 no en Cocina Piso 2 | PASS |
| 2 | División automática por estación en una orden | PASS |
| 3 | Bar Principal vs Bar VIP | PASS |
| 6 | Cambio de estación (Horno → Horno B) | PASS |
| 9 | Prioridad Horno A > Horno B | PASS |
| 11 | Reimpresión / snapshot KDS sin duplicados | PASS |
| 15 | Multitenant — Empresa B no ve órdenes A | PASS |

## Limitaciones documentadas

Ver `ROUTING_LIMITATIONS.md` — saturación 150 órdenes, offline SignalR, entidad Piso explícita.
