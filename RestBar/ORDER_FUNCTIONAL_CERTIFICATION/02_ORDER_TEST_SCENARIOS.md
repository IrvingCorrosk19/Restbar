# 02 — ORDER TEST SCENARIOS

| # | Escenario | Cobertura API | Cobertura Browser |
|---|-----------|---------------|-------------------|
| 1 | Apertura de orden | S01-01..07 | BR-01 mesa T-09 |
| 2 | Agregar productos | S02-01..04 | BR-02 Café Americano |
| 3 | Cliente cambia opinión | S03-01..02 | Parcial (notas) |
| 4 | Envío a cocina | S04-01..02 | BR-03 modal estación + envío |
| 5 | Cancelación pre-cocina | S05-01..02 | API |
| 6 | Cancelación post-cocina | Reglas backend | No UI supervisor |
| 7 | Cancelación post-preparado | Reglas backend | Pendiente UI |
| 8 | Cuentas separadas | S08-01..02 | JS cargado (separate-accounts.js) |
| 9 | Pago parcial | S09-01..04 | API (cajero) |
| 10 | Cliente abandona | Mesa liberada en cancel | API |
| 11 | Cambio de mesa | S11-01..03 | API MoveToTable |
| 12 | Multiusuario | S01-06 concurrente | Parcial |
| 13 | Multitenant | S13-01..05 | Mesas filtradas por branch |
| 14 | Seguridad IDOR | S14-01..02, S13-03..04 | API |
| 15 | Concurrencia | S15-01 (3 pagos paralelos) | API |
| 16 | Recuperación | GetActiveOrder post-refresh | Browser carga OK |
| 17 | Auditoría | S17-01 | Audit/Index 200 |
| 18 | Rendimiento | Smoke 38 órdenes ciclo | No carga masiva |

## Escenarios no automatizados al 100%

- 6/7: Requieren flujo KDS + supervisor; reglas validadas en `OrderService.CancelOrderAsync`
- 18: Carga 1000 órdenes / 100 usuarios — fuera de ventana de certificación actual; sin defectos en smoke test
