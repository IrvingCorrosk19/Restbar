# 09 — ORDER CONCURRENCY REPORT

## Pruebas ejecutadas

### S01-06 — Apertura concurrente
- 2 llamadas `SendToKitchen` simultáneas en misma mesa (mesero)
- **Resultado:** PASS — ambas OK, una sola orden activa

### S15-01 — Pagos paralelos
- 3 jobs PowerShell independientes
- Cada uno: login cajero + `POST /api/Payment/partial` $0.25 con IdempotencyKey único
- **Resultado:** PASS — 3/3 exitosos
- Advisory lock PostgreSQL por orden funcionó sin deadlock

### Concurrencia no ejecutada (limitación)

| Caso | Estado |
|------|--------|
| 5 navegadores / 10 usuarios simultáneos | No ejecutado |
| 10 pestañas modificando misma orden | No ejecutado |
| Deadlock stress 100+ hilos | No ejecutado |

## Hallazgos

- **Sin race conditions** detectadas en pagos parciales (3 concurrentes)
- **Sin pérdida de datos** en SendToKitchen concurrente
- IdempotencyKey previene cargos duplicados (S09-04 PASS)

## Riesgo residual

Carga extrema (100 usuarios) no probada; riesgo **bajo** dado advisory locks y versionado de orden.
