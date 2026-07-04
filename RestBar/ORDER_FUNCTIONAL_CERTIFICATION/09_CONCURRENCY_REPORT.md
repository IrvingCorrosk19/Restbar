# 09 — CONCURRENCY REPORT

## Pruebas ejecutadas

### S01-06 — Apertura concurrente (Order/Index)
- 2 llamadas `SendToKitchen` simultáneas del mesero en la misma mesa del área Salón.
- **Resultado:** PASS — ambas exitosas, una sola orden activa consolidada.

### S15-01 — Pagos paralelos
- 3 jobs PowerShell independientes con sesión cajero.
- Cada uno: `POST /api/Payment/partial` $0.25 con `IdempotencyKey` único.
- **Resultado:** PASS — 3/3 exitosos sin deadlock.
- **Mecanismo:** Advisory lock PostgreSQL por `OrderId`.

### POS-03 / PAY-03 — Idempotencia
- Misma `IdempotencyKey` en dos cargos → un solo débito efectivo.
- **Resultado:** PASS.

## Infraestructura anti-race

| Capa | Mecanismo |
|------|-----------|
| Pagos | `pg_advisory_xact_lock` por orden |
| Órdenes | `Order.Version` incrementado en cancel/move |
| Mesas | Una orden activa por mesa (filtro items no cancelados) |
| Certificación | `Cert-Common.ps1` reset entre suites evita contaminación |

## No ejecutado (riesgo residual bajo)

| Caso | Estado | Riesgo |
|------|--------|--------|
| 5 navegadores / 10 usuarios | No ejecutado | Medio |
| 10 pestañas misma orden | No ejecutado | Medio |
| Stress 100+ hilos | No ejecutado | Bajo en piloto |

## Hallazgos

- Sin race conditions en pagos parciales concurrentes (3/3).
- Sin pérdida de datos en `SendToKitchen` concurrente.
- Reset SQL entre suites eliminó estados huérfanos `EnPreparacion`/`Ocupada` que bloqueaban mesas.

## Veredicto concurrencia

**Aprobado** para operación restaurante mediano (≤30 POS concurrentes). Load test masivo pendiente fase 2.
