# 11 — ORDER MULTITENANT REPORT

## Modelo

```
Empresa A (admin@restbar.com)
  ├── Sucursal Central (10 mesas T-01..T-10)
  └── Sucursal Norte (admin.norte@restbar.com, mesa N-01)

Empresa B (admin.b@restbar.com)
  └── Sucursal B1 (1 mesa, producto exclusivo)
```

## Pruebas

| ID | Validación | Resultado |
|----|------------|-----------|
| S13-01 | Mesas A (10) ≠ mesas B (1) | PASS |
| S13-02 | Empresa B no lee orden Empresa A | 403 PASS |
| S13-03 | Empresa B no accede pagos Empresa A | 403 PASS |
| S13-04 | Empresa B no cancela orden Empresa A | 403 PASS |
| S13-05 | Norte ve N-01, no productos B | PASS |

## Aislamiento por capa

| Recurso | Filtro aplicado |
|---------|-----------------|
| Mesas POS | `GetActiveTablesByCompanyAndBranchAsync` |
| Órdenes lectura | `ValidateTableTenantAccessAsync` |
| Órdenes escritura | BranchId en creación + guards |
| Productos | `GetProductsByCategory` branch filter |
| Pagos | BranchId IDOR guard |
| Personas | `OrderBelongsToUserBranchAsync` |

## Fuga detectada y corregida

**Pre-fix:** Waiter en sucursal A podía ver mesas globales.  
**Post-fix:** Solo mesas de su CompanyId + BranchId.

## SignalR / KDS

Grupos por orden/mesa; sin evidencia de mezcla cross-tenant en pruebas ejecutadas.
