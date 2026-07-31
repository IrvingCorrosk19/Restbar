# 00 — Master Multitenant Certification

**Programa:** RB-1001 Enterprise Multitenant Functional Certification  
**Fecha:** 2026-07-31  
**Regla:** No nuevas features — demostrar aislamiento con evidencia.

## Resumen ejecutivo

RestBar implementa aislamiento **Company → Branch** vía claims (`CompanyId`, `BranchId`, `UserRole`) y `TenantScope`. En esta corrida se **corrigieron fugas críticas** (CustomerService sin filtro; SignalR grupos globales `kitchen`/`orders`/`table_all`).

**No** se ejecutó el laboratorio completo pedido (3 tenants × 3 empresas × 5 sucursales × 20 mesas × miles de pedidos × 5000 concurrentes × 50 browsers). Existe seed `ThreeCompaniesCertSeeder` (3 empresas / 1 sucursal cada una) y suite Playwright ampliada.

## Veredicto

Ver `15_FINAL_WORLD_CLASS_CERTIFICATION.md` → **PASS WITH CONDITIONS**

## Índice

| Doc | Tema |
|-----|------|
| 01 | Test matrix |
| 02 | Browser |
| 03 | API |
| 04 | Database |
| 05 | RBAC |
| 06 | Branch isolation |
| 07 | Floor isolation |
| 08 | Reports |
| 09 | Exports |
| 10 | SignalR |
| 11 | Background jobs |
| 12 | Performance |
| 13 | Security |
| 14 | Data leak |
| 15 | Final certification |
