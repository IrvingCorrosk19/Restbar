# 03 — ENTERPRISE TEST CASES

**Total casos automatizados:** 119  
**Última ejecución:** 2026-07-04 — **119/119 PASS**

## Suite Order/Index (38)

| ID | Caso | Resultado |
|----|------|-----------|
| ORD-ENV-01..03 | Sesiones admin/mesero/cajero | PASS |
| S01-01..07 | Apertura orden, GUID, tenant, concurrencia | PASS |
| S02-01..04 | Productos, stock, cantidad negativa | PASS |
| S03-01..02 | Cambio opinión pre-cocina | PASS |
| S04-01..02 | Envío cocina / doble envío | PASS |
| S05-01..02 | Cancelación pre-pago | PASS |
| S08-01..02 | Cuentas separadas (2 personas) | PASS |
| S09-01..04 | Pagos parciales, sobrepago, idempotencia | PASS |
| S11-01..03 | Cambio mesa, mesa ocupada rechazada | PASS |
| S13-01..05 | Aislamiento multitenant | PASS |
| S14-01..02 | IDOR cancel/payment | PASS |
| S15-01 | 3 pagos paralelos | PASS |
| S17-01 | Auditoría admin | PASS |

## Suite Functional (43)

Auth 13 roles, permisos SEC/NAV, POS-01..03, PAY-01..05, MT-01..05, RPT-01..03.

## Suite Routing (15)

RT-ENV, RT-S01 multi-piso, RT-S02 multi-estación, RT-S03 bares, RT-S06 cambio estación, RT-S09 prioridad horno, RT-S11 idempotencia KDS, RT-S15 multitenant KDS.

## Suite Enterprise Operations (23)

ENT-DISC, ENT-TIP, ENT-REF, ENT-COMM, ENT-SHIFT (4), ENT-INV (2), ENT-XFER (3), ENT-REC (2), ENT-CANC (2).

## Casos manuales / fase 2 (no automatizados)

| ID | Caso | Prioridad |
|----|------|-----------|
| MAN-01 | 10 usuarios simultáneos misma mesa | Alta |
| MAN-02 | SignalR reconexión offline 30s | Alta |
| MAN-03 | Load test 500 órdenes concurrentes | Alta |
| MAN-04 | Impresión térmica comanda | Media |
| MAN-05 | Pasarela tarjeta | Media |
| MAN-06 | Delivery UI completo | Media |

## Comando re-ejecución

```powershell
powershell -File scripts\Run-AllCertifications.ps1
```
