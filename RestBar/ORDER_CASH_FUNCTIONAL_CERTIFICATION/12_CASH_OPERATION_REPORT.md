# 12 — Informe Operaciones de Caja (apertura / movimientos)

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Cash/cash.spec.js`, `Cash/cash-extended.spec.js` |
| **Feature flags** | Cash **habilitado** en Production |

## RB-010 Cash Management

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| CASH-01 | Dashboard loads when module enabled | **PENDING_FULL_SUITE** | Screenshot `cash-01-dashboard.png` |
| CASH-02 | Open wizard shows register select | **PENDING_FULL_SUITE** | Screenshot `cash-02-open-wizard.png` |
| CASH-03 | Open session happy path or validation | **PENDING_FULL_SUITE** | Skip si sin registros |
| CASH-04 | Cash registers index | **PENDING_FULL_SUITE** | |
| CASH-05 | Paid-in API requires session (negative) | **PARTIAL** | API negativa, no UI |
| CASH-06 | Verify chain endpoint no 500 | **PENDING_FULL_SUITE** | |
| CASH-07 | Responsive dashboard viewport | **PENDING_FULL_SUITE** | |

## Cash extended

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| CASH-X01 | module enabled dashboard | **PENDING_FULL_SUITE** | No ModuleDisabled |
| CASH-X02 | open wizard and open or already open | **PENDING_FULL_SUITE** | `openCashIfNeeded` helper |
| CASH-X03 | registers page | **PENDING_FULL_SUITE** | |
| CASH-X04 | double open is rejected or handled | **PASS** | DEF-CASH-OPEN-001 FIXED — TempData, no crash |
| CASH-X05 | paid-in negative still not 500 | **PARTIAL** | API smoke |

## Veredicto

**PASS WITH CONDITIONS** — doble apertura corregida y verificada; suite completa caja **PENDING_FULL_SUITE**.
