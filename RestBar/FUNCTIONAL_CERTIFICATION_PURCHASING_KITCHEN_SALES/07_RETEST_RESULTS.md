# 07 — RETEST RESULTS

**Corrida final:** 2026-07-28 19:06:12  
**Script:** `scripts/Run-PurchasingKitchenSalesCertification.ps1`  
**Artefactos:** `PKS_TEST_RESULTS.csv`, `PKS_DEFECTS.csv`, `PKS_RUN_SUMMARY.json`

---

## Totales

| Métrica | Valor |
|---------|-------|
| PASS | **39** |
| FAIL | **0** |
| BLOCKED | **47** |
| TOTAL | **86** |

---

## Por módulo

| Módulo | Pass | Fail | Blocked | Interpretación |
|--------|------|------|---------|----------------|
| Setup | 2 | 0 | 0 | Entorno OK |
| Purchasing | 3 | 0 | **42** | Módulo ausente (BLOCKED = Critical gap) |
| Kitchen | **18** | 0 | 0 | Core KDS certificado en vivo |
| Sales | **12** | 0 | **5** | Core POS OK; gaps caja/fiscal/combos/HH |
| Security | 2 | 0 | 0 | Multitenant admin.b + permisos |
| Audit | 1 | 0 | 0 | |
| SignalR | 1 | 0 | 0 | negotiate |

---

## Retest de fixes

| Fix | Antes | Después |
|-----|-------|---------|
| Seed mesero inactivo | PKS-ENV-02 FAIL / waiter=False | **PASS** |
| SendToKitchen NRE body null | 400 Object reference | Guard BadRequest (no regresión en flows) |
| Script mesa P1 no asignada | 403 falsos FAIL kitchen/sales | **Kitchen 18/18 · Sales core 12/12** |

---

## Retest browser UI (post FIX-04)

| Caso | Resultado |
|------|-----------|
| Modal pago → Procesar Pago (T-04, $6.00 Efectivo) | **PASS** — mesa liberada a DISPONIBLE |
| Flujo completo mesa→producto→KDS→pago UI | **PASS** |

## Regresiones

Ninguna observada en flujos kitchen/sales core tras FIX-01/02/04.

---

## Criterio PASS/FAIL de certificación

Aunque FAIL de ejecución = 0, existen **47 BLOCKED** incluyendo **Critical** (PO completo, caja, fiscal) ⇒ la certificación funcional enterprise de los **tres** módulos es **FAIL**.
