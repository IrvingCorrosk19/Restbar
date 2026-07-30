# 03 — Matriz de datos de prueba

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Commits** | `14e12aa` (nav), `29f3e6e` (cash/UpdateItemStatus), `33e47e2` (POS helpers) |

## Credenciales y usuarios

| Rol | Email | Password | Sembrado VPS | Uso en suites |
|-----|-------|----------|--------------|---------------|
| Admin | `admin@restbar.com` | `123456` | Sí | Todas las suites (default) |
| Mesero | `mesero@restbar.com` | `123456` | **Incerto** | WTR-02 (skip si falla login) |
| Cajero | `cajero@restbar.com` | `123456` | **Incerto** | WTR-03 (skip si falla login) |
| Chef | `chef@restbar.com` | `123456` | **Incerto** | WTR-04 (skip si falla login) |
| Admin tenant B | `admin@costa.restbar.com` | `123456` | **No** (típico) | MT-02 (skip) |

## Datos operativos requeridos

| Recurso | Requerido por | Estado VPS | Notas |
|---------|---------------|------------|-------|
| Mesas activas (≥2) | TBL-02, ORD-E2E-01 | Sí | `.table-card` en POS |
| Productos en catálogo | ORD-E2E-01, PAY-03/04 | Sí | `addFirstProduct` helper |
| Estaciones (kitchen/bar) | STN-*, KDS-* | Sí | GetStations > 0 |
| Áreas / pisos | FLR-02 | Parcial | `data-table-area` puede ser Guid vacío |
| Registros de caja | CASH-03, CASH-X02/04 | Sí | Skip si `registerId` sin opciones |
| Sesión de caja activa | CASH-X04 | Opcional | `openCashIfNeeded` helper |
| Feature flag Cash | CASH-*, FF-01 | **Habilitado** (Production) | Confirmado 2026-07-30 |

## Cobertura por suite (spec → IDs)

| Spec | Test IDs |
|------|----------|
| `Floors/floors.spec.js` | FLR-01..04 |
| `Stations/stations.spec.js` | STN-01..05 |
| `Waiters/waiters.spec.js` | WTR-01..05 |
| `Orders/orders-navigation.spec.js` | ORD-NAV-01..06 |
| `Orders/orders-e2e.spec.js` | ORD-E2E-01..05 |
| `Tables/tables.spec.js` | TBL-01..04 |
| `Kitchen/kitchen.spec.js` | KDS-01..04 |
| `Payments/payments.spec.js` | PAY-01..04 |
| `Cash/cash.spec.js` | CASH-01..07 |
| `Cash/cash-extended.spec.js` | CASH-X01..05 |
| `Shifts/shifts.spec.js` | SHF-01..03 |
| `Inventory/inventory.spec.js` | INV-01..08 |
| `Inventory/inventory-order-impact.spec.js` | INV-ORD-01..02 |
| `Multitenant/multitenant.spec.js` | MT-01..02 |
| `Responsive/responsive.spec.js` | RSP-01..03 |
| `Regression/regression.spec.js` | REG-01..06 |
| `Regression/order-cash-negatives.spec.js` | NEG-01..04 |
| `Security/security.spec.js` | SEC-01..05 |
| `Smoke/smoke.spec.js` | SMK-01..03 |

## Gaps de datos conocidos

| Gap | Impacto |
|-----|---------|
| Usuario cross-tenant (`admin@costa.restbar.com`) | MT-02 SKIP frecuente |
| Roles mesero/cajero/chef no sembrados | WTR-02..04 SKIP condicional |
| Sin suite dedicada split bill / cierre caja | Certificación **PARCIAL** en esos módulos |

## Veredicto matriz

**LISTA PARA SUITE COMPLETA** con admin; aislamiento multitenant y RBAC por rol dependen de seed adicional.
