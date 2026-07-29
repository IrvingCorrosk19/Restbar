# 05 — BRECHAS DE NEGOCIO (BUSINESS GAPS)

**Prioridad:** P0 = impide operación seria · P1 = limita ventas · P2 = competitividad

---

## P0 — Impiden recuperar inversión en restaurante formal

| ID | Brecha | Impacto negocio | Evidencia |
|----|--------|-----------------|-----------|
| GAP-01 | **Sin caja / arqueo** | Pérdidas efectivo, fraude, cierre imposible | SB-02, SB-11 |
| GAP-02 | **Sin precuenta** | Fricción cobro, disputas cliente | SB-03 |
| GAP-03 | **Sin factura fiscal** | Ilegal/inoperable en mercados regulados | SB-04 |
| GAP-04 | **Sin módulo compras** | Food cost fuera de control, Excel paralelo | PKS 42 BLOCKED, 404 |
| GAP-05 | **Sin proveedores** | No evaluar costos ni cumplimiento compra | Supplier 404, stub report |

---

## P1 — Limitan crecimiento y ventas

| ID | Brecha | Impacto |
|----|--------|---------|
| GAP-06 | Combos (SB-09) | Ticket promedio estático |
| GAP-07 | Happy hour auto (SB-10) | Promociones manuales propensas a error |
| GAP-08 | Impresión térmica (SB-05) | Dependencia pantallas; no ticket cocina/bar físico |
| GAP-09 | Export PDF/Excel real | Gerencia no puede compartir reportes |
| GAP-10 | Cierre de día ritual (SB-08) | Órdenes abiertas fantasma |
| GAP-11 | Onboarding self-service | Costo implementación alto |
| GAP-12 | 3 UIs reportes avanzados rotas | Decisiones operativas bloqueadas en pantalla |

---

## P2 — Competitividad enterprise / SaaS

| ID | Brecha | Evidencia |
|----|--------|-----------|
| GAP-13 | Planes SaaS + billing (SB-01) | No monetización escalable |
| GAP-14 | Import migración POS (SB-06) | Lock-in inverso: difícil entrar |
| GAP-15 | Export tenant completo (SB-07) | Riesgo vendor lock-in percibido |
| GAP-16 | Forecast / analytics predictivo | GrowthForecasts vacío |
| GAP-17 | Delivery UI | Solo tipo orden API |
| GAP-18 | Hotel/casino/franquicia intl. (SB-12/13) | |
| GAP-19 | Load test 500+ concurrentes | No certificado |

---

## Brechas corregidas recientemente (valor positivo)

| Fix | Fecha | Beneficio |
|-----|-------|-----------|
| Resumen pedido limpia al cancelar | 2026-07-29 | Menos errores mesero post-cancelación |
| Modal pago UI (DEF-UI-PAY-01) | 2026-07-28 | Cobro funcional en browser |
| Multitenant 51/51 | 2026-07-28 | Cadena/franquicia piloto viable |
| Descuento restringido por rol | 2026-07-04 | Reduce fraude mesero |

---

## Mapa brecha → módulo a construir

```
P0: CashRegister → Precuenta → Invoice/Fiscal → PurchaseOrder + Supplier
P1: Combos → HappyHour → PrintService → DayClose → Fix report UIs
P2: Subscription → Import/Export → Forecast engine
```
