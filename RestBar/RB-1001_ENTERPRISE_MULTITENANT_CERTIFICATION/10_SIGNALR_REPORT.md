# 10 — SignalR Report

## Antes (fuga demostrable por diseño)

Grupos globales: `kitchen`, `orders`, `table_all`, `stock_updates`, `cash_dashboard`, `station_{type}` **sin CompanyId**.  
Cualquier usuario autenticado unido a `kitchen` recibía eventos de **todas** las empresas en el mismo hub.

## Después (fix RB-1001)

| Grupo | Nuevo formato |
|-------|---------------|
| Kitchen | `c_{company:N}_kitchen` |
| Orders | `c_{company:N}_orders` |
| Table all | `c_{company:N}_table_all` |
| Stock | `c_{company:N}_stock_updates` |
| Cash dash | `c_{company:N}_cash_dashboard` |
| Station | `c_{company:N}_station_{type}` |
| Order/Table id | `order_{guid}` / `table_{guid}` (único) |

- `OrderHub` une usando claim `CompanyId`.
- `OrderHubService` resuelve CompanyId desde Order/Table/Product/Register antes de fan-out.
- Unit: `SignalRTenantGroupsTests` **PASS**.

## Residual

- Lab 50 browsers multi-tenant: **no ejecutado**.
- Clientes JS sin claim CompanyId no reciben kitchen (fail closed).
