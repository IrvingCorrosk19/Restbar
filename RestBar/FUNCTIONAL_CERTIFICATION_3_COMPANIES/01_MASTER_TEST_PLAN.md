# 01 — Master Test Plan — Certificación 3 Empresas Multitenant

**Proyecto:** RestBar  
**Fecha:** 2026-07-05  
**Ambiente:** Desarrollo local (`http://localhost:5001`)  
**Base de datos:** PostgreSQL 18 — `RestBar`  
**Script principal:** `FUNCTIONAL_CERTIFICATION_3_COMPANIES/scripts/Run-ThreeCompaniesCertification.ps1`  
**Seeder:** `GET /Seed/SeedThreeCompaniesCertification` → `ThreeCompaniesCertSeeder.cs`

## Objetivo

Certificar que RestBar opera de forma correcta, segura y consistente con **3 empresas independientes**, cada una con sucursal, pisos/áreas, estaciones, mesas, usuarios por rol y flujos operativos completos.

## Alcance ejecutado

| Área | Casos TC3 | Estado |
|------|-----------|--------|
| Setup y seed 3 empresas | TC3-ENV-01..05 | PASS |
| Aislamiento multitenant | TC3-MT-01..05 | PASS |
| Asignaciones meseros | TC3-ASG-01 | PASS |
| Permisos por rol | TC3-ROL-01..05 | PASS |
| Flujo orden (Costa, Norte, Sur) | TC3-ORD/RTE/SPL/PAY/MOV/CAN-* | PASS |
| Seguridad IDOR cross-company | TC3-SEC-01..04, TC3-MOV-SEC-01 | PASS |
| Auditoría y reportes | TC3-AUD-01, TC3-RPT-01 | PASS |
| Concurrencia pagos | TC3-CON-01 | PASS |

**Total TC3:** 40/40 PASS

## Regresión complementaria

`scripts/Run-AllCertifications.ps1` — 119 casos (Functional 43 + Order 38 + Routing 15 + Enterprise 23): **ALL SUITES PASS**

## Precondiciones

1. Backup PostgreSQL en `FUNCTIONAL_CERTIFICATION_3_COMPANIES/backups/`
2. `dotnet build -c Release`
3. Servidor en `http://localhost:5001`
4. Password usuarios cert: `123456`

## Empresas de prueba

| Empresa | Sucursal | Mesas | Dominio email |
|---------|----------|-------|---------------|
| Restaurante Costa | Costa Centro | C-01..C-10 | `@costa.restbar.com` |
| Restaurante Norte | Norte Mall | NM-01..NM-10 | `@norte.restbar.com` |
| Restaurante Sur | Sur Hotel | S-01..S-15 | `@sur.restbar.com` |

## Criterios de aprobación

- Cero defectos Critical/High abiertos en TC3
- Aislamiento entre empresas verificado (mesas, productos, órdenes, pagos)
- Flujo orden + pago + cambio mesa + cancelación en las 3 empresas
- IDOR cross-company bloqueado (403/404)

## Veredicto

**FUNCTIONAL CERTIFICATION 3 COMPANIES: PASS**
