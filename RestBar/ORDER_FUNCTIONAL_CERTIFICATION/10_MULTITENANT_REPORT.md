# 10 — MULTITENANT REPORT

## Modelo validado

```
Empresa A (admin@restbar.com) — RestBar Centro
  ├── 13 mesas (T-01..T-10, P1-01, P2-01, P3-01)
  └── Sucursal Norte (admin.norte@restbar.com) — N-01

Empresa B (admin.b@restbar.com) — Sucursal B Centro
  └── 1 mesa B-01, producto exclusivo
```

## Pruebas ejecutadas

| ID | Validación | Resultado |
|----|------------|-----------|
| S13-01 | Mesas A (13) ≠ mesas B (1) | PASS |
| S13-02 | Empresa B no lee orden Empresa A | 403 PASS |
| S13-03 | Empresa B no accede pagos Empresa A | 403 PASS |
| S13-04 | Empresa B no cancela orden Empresa A | 403 PASS |
| S13-05 | Norte ve N-01, no productos B | PASS |
| MT-01..05 | Functional multitenant suite | PASS |
| RT-S15-01 | KDS Empresa B no ve items Empresa A | PASS |

## Aislamiento por capa

| Recurso | Filtro |
|---------|--------|
| Mesas POS | `CompanyId` + `BranchId` |
| Mesero | `UserAssignment` → área/mesas |
| Órdenes lectura/escritura | `ValidateTableTenantAccessAsync` |
| Productos | Branch filter en categorías |
| Pagos | `OrderBelongsToUserBranchAsync` |
| KDS | Estaciones por company/branch |

## Contaminación detectada y corregida

1. **Pre-fix:** Mesero veía mesas globales → filtro `FilterTablesForWaiter`.
2. **Pre-fix:** IDOR en Cancel/GetActiveOrder → guards tenant.
3. **Cert-fix:** Reset SQL con nombres de sucursal incorrectos no liberaba mesas → `Cert-Common.ps1` reset global dev.

## Veredicto multitenant

**PASS** — Sin fuga cross-tenant en 119 pruebas. Apto para cadena multi-sucursal.
