# 07 — Informe Flujo de Pedidos

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Orders/orders-navigation.spec.js`, `Orders/orders-e2e.spec.js` |
| **Commits** | `14e12aa`, `33e47e2` |

## Navegación POS (P0)

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| ORD-NAV-01 | POS chrome shows Volver and Inicio | **PASS** | P0 nav fix |
| ORD-NAV-02 | Inicio returns to Home without trap | **PASS** | |
| ORD-NAV-03 | Volver exits to safe returnUrl | **PASS** | |
| ORD-NAV-04 | open redirect returnUrl is rejected | **PASS** | |
| ORD-NAV-05 | KDS Dashboard link uses tag helper / Home | **PASS** | |
| ORD-NAV-06 | Home Pedidos card enters POS with chrome | **PASS** | |

## E2E mesa → cocina

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| ORD-E2E-01 | select table add product send kitchen | **PASS** | Retest 2026-07-30; helper POS `33e47e2` |
| ORD-E2E-02 | KDS receives and can mark ready | **PENDING_FULL_SUITE** | Depende routing estación |
| ORD-E2E-03 | StationOrders pages no 500 | **PENDING_FULL_SUITE** | kitchen + bar |
| ORD-E2E-04 | tables API returns data | **PENDING_FULL_SUITE** | GetActiveTables |
| ORD-E2E-05 | MoveToTable API validation | **PARTIAL** | Solo validación API negativa, no UI transfer |

## Veredicto

**PASS WITH CONDITIONS** — P0 navegación y send-to-kitchen verificados; E2E completo **PENDING_FULL_SUITE**.
