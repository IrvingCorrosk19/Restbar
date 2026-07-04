# 01 — MASTER TEST PLAN — Order/Index Enterprise

**Módulo:** `Order/Index` (POS RestBar)  
**Fecha certificación:** 2026-07-04  
**Entorno:** ASP.NET Core 8, PostgreSQL 18, `http://localhost:5001`  
**Backup:** `backups/RestBar_pre_order_enterprise_20260704_171831.dump`

## Objetivo

Certificar que Order/Index soporta la operación real de un restaurante mediano-alto con múltiples pisos, estaciones, sucursales y empresas — no solo validación de pantallas.

## Equipo de certificación (rol simulado)

| Rol | Responsabilidad en esta certificación |
|-----|-------------------------------------|
| Principal / Enterprise Architect | Modelo multitenant, routing KDS, integridad transaccional |
| Restaurant Operations Consultant | Flujos mesero → cocina → caja |
| QA Director | Suites automatizadas, criterios PASS/FAIL |
| Software Auditor | IDOR, auditoría, trazabilidad pagos |
| Multi-Tenant SaaS Architect | Aislamiento Empresa → Sucursal → Mesa |

## Alcance ejecutado

| Área | Suite | Casos |
|------|-------|-------|
| Order/Index API + flujos POS | `Run-OrderCertification.ps1` | 36 |
| App-wide (auth, permisos, POS base) | `Run-FullCertification.ps1` | 43 |
| Enrutamiento KDS multi-piso/estación | `Run-RoutingCertification.ps1` | 15 |
| Operaciones enterprise (descuentos, refunds, turnos, inventario) | `Run-EnterpriseCertification.ps1` | 23 |
| **Total automatizado** | `scripts/Run-AllCertifications.ps1` | **117** |

## Escenarios del prompt (28) — estrategia

| Bloque | Escenarios | Método |
|--------|------------|--------|
| Operación core | 1–9, 11 | API automatizada + correcciones iterativas |
| Multi-piso / estaciones | 10–13 | Routing cert + áreas/estaciones seed |
| Inventario / recetas / transferencias | 14–16 | Enterprise cert + `InventoryOperationsService` |
| Personal / cocina | 17–18 | UserAssignment, Shift, KDS API |
| Modos servicio / promos | 19–20 | Análisis código + descuentos server (`ApplyDiscount`) |
| Finanzas | 21–23 | Payment API, TipAllocation, CommissionRule |
| Impresión / recuperación / escala | 24–27 | Análisis + limitaciones documentadas (fase 2) |
| Usabilidad | 28 | Análisis frontend + observaciones operativas |

## Criterios de aprobación

- **0 defectos críticos o altos abiertos** en escenarios ejecutados
- **117/117 tests PASS** en batería completa
- Sin fugas multitenant en mesas, órdenes, pagos, KDS
- Sin inconsistencias financieras en pagos parciales, idempotencia, sobrepago
- Enrutamiento determinístico por estación (no orden completa en KDS incorrecto)

## Metodología

1. Backup PostgreSQL antes de cambios
2. Seed demo + multitenant + enterprise routing
3. Ejecutar → detectar defecto → corregir código → `dotnet build` → re-ejecutar
4. Evidencia: CSVs, logs build, dump backup

## Exclusiones fase 2 (no bloquean PASS operacional)

- Carga 500+ clientes simultáneos (load test dedicado)
- E2E SignalR reconexión prolongada / offline 30+ min
- Pasarela tarjeta / facturación electrónica
- Impresión térmica end-to-end
- Browser E2E Playwright completo 12 roles
