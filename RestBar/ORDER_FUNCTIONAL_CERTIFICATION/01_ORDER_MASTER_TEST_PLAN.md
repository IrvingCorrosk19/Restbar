# 01 — ORDER MASTER TEST PLAN

**Módulo:** `Order/Index` (POS RestBar)  
**Fecha:** 2026-07-04  
**Entorno:** ASP.NET Core 8, PostgreSQL 18, `http://localhost:5001`  
**Backup:** `ORDER_FUNCTIONAL_CERTIFICATION/backups/RestBar_pre_order_cert_20260704_135205.dump`

## Objetivo

Certificar que el módulo Order/Index soporta operación real de restaurante: apertura de orden, productos, cocina, cancelaciones, cuentas separadas, pagos parciales, cambio de mesa, multiusuario, multitenant, seguridad, concurrencia, recuperación, auditoría y rendimiento.

## Alcance

| Incluido | Excluido (fase posterior) |
|----------|---------------------------|
| Order/Index UI + APIs POS | Reportes fuera de Order |
| SendToKitchen, Cancel, MoveToTable | Carga 10.000 productos / 100 usuarios |
| Pagos parciales vía `/api/Payment` | SignalR reconexión E2E prolongada |
| Person/Cuentas separadas API | Cancelación post-preparado con supervisor UI |
| Multitenant mesas/órdenes/pagos | Rendimiento 1000+ órdenes masivas |

## Roles probados

admin, mesero (waiter), cajero (cashier), admin.b (Empresa B), admin.norte (Sucursal Norte)

## Estrategia

1. **API automatizada** — `scripts/Run-OrderCertification.ps1` (38 casos)
2. **Browser E2E** — Order/Index en pestaña real (login → mesa → producto → cocina)
3. **Corrección iterativa** — defectos corregidos antes de re-ejecutar
4. **Evidencia** — CSVs, screenshots, logs de build

## Criterios de aprobación

- 0 defectos críticos/altos
- Sin fugas multitenant
- Sin inconsistencias financieras en pagos parciales
- Sin race conditions en pagos concurrentes
- GetActiveOrder, mesas y pagos con aislamiento por sucursal

## Entregables

Documentos 01–15 en `ORDER_FUNCTIONAL_CERTIFICATION/`
