# 17 — Informe Impacto Inventario por Ventas

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Inventory/inventory-order-impact.spec.js`, `Inventory/inventory.spec.js` |

## Impacto post-pedido

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| INV-ORD-01 | inventory page still works after kitchen send | **PASS** | Retest 2026-07-30 |
| INV-ORD-02 | GetInventoryData after order activity | **PENDING_FULL_SUITE** | API post-actividad |

## Inventario general (suite previa)

| ID | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| INV-01 | page loads after login | **PASS** (prev) | 8/8 suite previa |
| INV-02 | dashboard card navigates | **PASS** (prev) | |
| INV-03 | low-stock alerts API + UI | **PASS** (prev) | |
| INV-04 | support APIs respond 200 | **PASS** (prev) | |
| INV-05 | consumption report filters | **PASS** (prev) | |
| INV-06 | export does not 500 | **PASS** (prev) | |
| INV-07 | unauthorized redirect | **PASS** (prev) | |
| INV-08 | no significant console errors | **PASS** (prev) | |

## Gap declarado

No se verifica decremento cuantitativo de stock tras venta (assert delta qty).

## Veredicto

**PARTIAL** — estabilidad post-send verificada; delta de stock E2E **NOT_COVERED**.
