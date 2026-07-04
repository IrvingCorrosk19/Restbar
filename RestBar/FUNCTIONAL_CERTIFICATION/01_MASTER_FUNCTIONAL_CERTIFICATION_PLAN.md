# Plan Maestro — Certificación Funcional Enterprise RestBar

**Fecha:** 2026-07-04  
**Ambiente:** Desarrollo local (`http://localhost:5001`)  
**Base de datos:** PostgreSQL 18 — `RestBar`  
**Backup previo:** `FUNCTIONAL_CERTIFICATION/backups/RestBar_pre_certification_20260704_125758.dump`

## Objetivo

Certificar funcionalmente RestBar en modo desarrollo: ejecutar flujos reales, corregir defectos hasta aprobación, validar aislamiento multi-tenant y permisos por rol.

## Alcance de esta fase

| Área | Cobertura |
|------|-----------|
| Autenticación (12 roles) | ✅ Ejecutado |
| Permisos y navegación | ✅ Ejecutado |
| Multi-tenant (Empresa A/B) | ✅ Ejecutado |
| POS (mesa → cocina → orden activa) | ✅ Ejecutado |
| Pagos parciales + idempotencia | ✅ Ejecutado |
| KDS / Cocina (chef) | ✅ Ejecutado |
| Reportes (acceso por rol) | ✅ Ejecutado |
| Inventario (inventarista) | ✅ Ejecutado |
| Concurrencia extrema / carga masiva | ⏳ Backlog fase 2 |
| Todos los reportes exportables | ⏳ Backlog fase 2 |
| SignalR reconexión E2E | ⏳ Backlog fase 2 |

## Metodología

1. Backup completo PostgreSQL antes de cambios estructurales
2. Seed de datos demo + multi-tenant (`/Seed/SeedCertificationMultiTenant`)
3. Suite automatizada: `FUNCTIONAL_CERTIFICATION/scripts/Run-FullCertification.ps1`
4. Ciclo: detectar → corregir → compilar → re-ejecutar hasta PASS
5. Documentación de defectos, RCA y fixes

## Criterios de aprobación

- **PASS:** 0 defectos críticos/altos/medios abiertos en escenarios ejecutados
- **FAIL:** Cualquier defecto que impida operación normal del restaurante en escenarios probados

## Herramientas

- `dotnet run --urls "http://localhost:5001"`
- `Run-FullCertification.ps1` — 32 casos automatizados
- `psql` — validación directa de datos
- Scripts SQL en `FUNCTIONAL_CERTIFICATION/scripts/`

## Roles certificados (login + permisos)

| Rol | Email seed | Password |
|-----|------------|----------|
| admin | admin@restbar.com | 123456 |
| manager | gerente@restbar.com | 123456 |
| supervisor | supervisor@restbar.com | 123456 |
| waiter | mesero@restbar.com | 123456 |
| cashier | cajero@restbar.com | 123456 |
| chef | chef@restbar.com | 123456 |
| bartender | bartender@restbar.com | 123456 |
| accountant | contador@restbar.com | 123456 |
| support | soporte@restbar.com | 123456 |
| inventarista | inventarista@restbar.com | 123456 |
| superadmin | superadmin@restbar.com | 123456 |

## Veredicto de suite automatizada

**43/43 PASS** — Ver `04_EXECUTED_TESTS.csv`, `VERDICT.md` y `15_EXECUTIVE_FUNCTIONAL_SUMMARY.md`
