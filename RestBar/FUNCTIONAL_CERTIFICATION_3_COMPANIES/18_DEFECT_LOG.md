# 18 — Registro de Defectos

**Fecha:** 2026-07-05  
**Certificación:** 3 Empresas Multitenant

## Defectos encontrados durante certificación

| ID | Severidad | Descripción | Estado |
|----|-----------|-------------|--------|
| DEF-TC3-001 | **Medium** | Variable `$pid` en script PowerShell conflictúa con variable automática `$PID` (read-only). Bloqueaba tests TC3-SEC-* y TC3-CON-01 | **Corregido** |
| DEF-TC3-002 | **Low** | TC3-ASG-01 marcaba PASS aunque overlap > 0 (lógica `else {"PASS"}`) | **Corregido** |
| DEF-TC3-003 | **Low** | TC3-CON-01 marcaba PASS aunque pagos paralelos fallaran | **Corregido** |

## Defectos críticos / altos

**Ninguno abierto.**

## Defectos conocidos fuera de alcance TC3 (Ready for Sale)

13 SALE BLOCKERS documentados en `READY_FOR_SALE_CERTIFICATION/COMMERCIAL_VERDICT.md` (caja, factura, planes SaaS, etc.) — no bloquean certificación operativa multitenant.

## Riesgos residuales (no defectos de código)

| ID | Severidad | Descripción |
|----|-----------|-------------|
| RISK-001 | Medium | SignalR multitenant no probado con 2 browsers simultáneos |
| RISK-002 | Low | Reportes no cuadrados post-venta en las 3 empresas (cleanup cancela órdenes) |
| RISK-003 | Low | Solo 1 sucursal por empresa en seeder (no multi-sucursal same company) |
