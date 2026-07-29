# 09 — ROADMAP DE PRODUCTO

**Horizonte:** 18 meses · Basado en gaps P0–P2 verificados

---

## Fase 0 — Estabilización (0–2 meses) ✅ parcial

| Item | Estado |
|------|--------|
| Certificación operativa 119/119 | ✅ |
| Multitenant 51/51 | ✅ |
| Fix cancel → resumen pedido | ✅ 2026-07-29 |
| Fix modal pago | ✅ 2026-07-28 |
| Deploy VPS producción piloto | ✅ |

---

## Fase 1 — Operación restaurante viable (2–5 meses)

**Objetivo:** Cliente opera un turno completo sin Excel paralelo en caja.

| Epic | Entregables |
|------|-------------|
| **E1 Caja** | CashRegister, apertura/cierre, arqueo, movimientos, integración pagos efectivo |
| **E2 Precuenta** | Pre-bill API + UI + impresión |
| **E3 Reportes** | PDF/Excel reales; fix JS sales/operational/customer analysis |
| **E4 Cierre día** | Ritual cierre, bloqueo órdenes abiertas, reporte Z |

**Criterio salida:** RFS SB-02,03,05,08 resueltos · PKS Sales ≥80%

---

## Fase 2 — Back-office (5–9 meses)

**Objetivo:** Control food cost y compras.

| Epic | Entregables |
|------|-------------|
| **E5 Proveedores** | CRUD supplier, multitenant |
| **E6 Órdenes compra** | PO → recepción → stock → costo |
| **E7 Inventario** | Reconciliación, mínimos, alertas compra |
| **E8 Supplier report** | Reemplazar stub ceros |

**Criterio salida:** PKS Purchasing ≥70% PASS

---

## Fase 3 — Ingresos incrementales (9–12 meses)

| Epic | Entregables |
|------|-------------|
| **E9 Combos** | Bundle pricing POS |
| **E10 Happy Hour** | Reglas horario/día |
| **E11 Fiscal v1** | Un país piloto post-pago |
| **E12 Tips dashboard** | Reporte propinas mesero |

---

## Fase 4 — SaaS comercial (12–18 meses)

| Epic | Entregables |
|------|-------------|
| **E13 Subscription** | Planes, límites, billing |
| **E14 Import POS** | Wizard CSV |
| **E15 Export tenant** | Self-service backup |
| **E16 Onboarding** | Wizard día 1 |
| **E17 Forecast v1** | Tendencias + estacionalidad básica |

---

## Fase 5 — Enterprise (18+ meses, opcional)

- Offline POS stress certificado
- Delivery UI + integraciones
- Multi-país fiscal
- Load 500+ concurrent
- Hotel/casino modules

---

## Dependencias críticas

```
E1 Caja → E4 Cierre día → E11 Fiscal
E5 Supplier → E6 PO → E7 Inventario → E8 Reports
E13 Subscription → venta SaaS self-service
```
