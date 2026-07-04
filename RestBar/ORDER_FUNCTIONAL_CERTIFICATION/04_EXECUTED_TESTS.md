# 04 — EXECUTED TESTS

**Última ejecución:** 2026-07-04  
**Comando:** `scripts/Run-AllCertifications.ps1`  
**Resultado global:** **119/119 PASS**

## Suite 1 — Functional (43)

| ID | Nombre | Resultado |
|----|--------|-----------|
| ENV-01..02 | Seed demo + multitenant | PASS |
| AUTH-01..13 | Login 12 roles + rechazos | PASS |
| SEC-01..10 | Permisos por rol | PASS |
| NAV-01..02 | KDS kitchen/bar routes | PASS |
| POS-01..03 | Mesas, SendToKitchen, GetActiveOrder | PASS |
| PAY-01..05 | Parcial, resumen, sobrepago, idempotencia, 404 | PASS |
| MT-01..05 | Multitenant productos/mesas/pagos | PASS |
| RPT-01..03 | Acceso reportes por rol | PASS |

## Suite 2 — Order/Index (38)

| ID | Escenario | Resultado |
|----|-----------|-----------|
| S01-01..07 | Apertura orden | PASS |
| S02-01..04 | Productos | PASS |
| S03-01..02 | Cambio opinión | PASS |
| S04-01..02 | Cocina | PASS |
| S05-01 | Cancelación | PASS |
| S08-01..02 | Cuentas separadas | PASS |
| S09-01..04 | Pago parcial | PASS |
| S11-01..02 | Cambio mesa | PASS |
| S13-01..05 | Multitenant | PASS |
| S14-01..02 | IDOR | PASS |
| S15-01 | Concurrencia pagos | PASS |
| S17-01 | Auditoría | PASS |

## Suite 3 — Routing (15)

| ID | Resultado |
|----|-----------|
| RT-ENV-01..04 | PASS |
| RT-S01-01..02 Multi-piso | PASS |
| RT-S02-01..03 Multi-estación | PASS |
| RT-S03-01..02 Dos bares | PASS |
| RT-S06-01 Cambio estación | PASS |
| RT-S09-01 Prioridad Horno | PASS |
| RT-S11-01 KDS idempotente | PASS |
| RT-S15-01 Multitenant KDS | PASS |

## Suite 4 — Enterprise Operations (23)

| ID | Módulo | Resultado |
|----|--------|-----------|
| ENT-DISC-01..02 | Descuentos | PASS |
| ENT-TIP-01..02 | Propinas | PASS |
| ENT-REF-01 | Reembolsos | PASS |
| ENT-COMM-01 | Comisiones | PASS |
| ENT-SHIFT-01..04 | Turnos | PASS |
| ENT-INV-01..02 | Movimientos inventario | PASS |
| ENT-XFER-01..03 | Transferencias | PASS |
| ENT-REC-01..02 | Recetas BOM | PASS |
| ENT-CANC-01..02 | Cancel supervisor | PASS |

## Artefactos

- `ORDER_TEST_RESULTS.csv` — última corrida Order
- `ORDER_ROUTING_CERTIFICATION/ROUTING_TEST_RESULTS.csv`
- `ENTERPRISE_OPERATION_CERTIFICATION/ENTERPRISE_TEST_RESULTS.csv`
- `FUNCTIONAL_CERTIFICATION/04_EXECUTED_TESTS.csv`
