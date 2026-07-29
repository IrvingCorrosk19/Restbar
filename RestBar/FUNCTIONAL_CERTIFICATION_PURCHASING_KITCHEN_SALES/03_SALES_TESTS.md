# 03 — SALES TESTS (Ventas / POS)

**Suite:** `scripts/Run-PurchasingKitchenSalesCertification.ps1`  
**Ejecución:** 2026-07-28 19:06:12 · `http://localhost:5001`  
**Resultado módulo:** **12 PASS · 0 FAIL · 5 BLOCKED** (gaps comerciales Critical/High)

---

## Flujo verificado

```
Mesa (asignada) → Orden → Productos → Cocina → Ready → Pago parcial/mixto → Cierre
```

---

## Matriz ejecutada

| ID | Escenario | Resultado | Evidencia |
|----|-----------|-----------|-----------|
| SALE-01 | Venta normal | **PASS** | orderId generado |
| SALE-02 | Payment summary / totales | **PASS** | `remainingAmount` / `totalOrderAmount` |
| SALE-03 | Pago parcial efectivo | **PASS** | Method=Efectivo |
| SALE-04 | Pago mixto + cierre | **PASS** | Efectivo + Tarjeta |
| SALE-05 | Cobro duplicado (idempotency) | **PASS** | Misma IdempotencyKey |
| SALE-06 | TakeOut | **PASS** | OrderType |
| SALE-07 | Delivery (tipo) | **PASS** | Tipo API; UI delivery = gap |
| SALE-08 | Descuento denegado a mesero | **PASS** | HTTP 403 |
| SALE-09 | Descuento supervisor+ | **PASS** | HTTP 200 |
| SALE-10 | Cambio de mesa | **PASS** | Sin re-routing estaciones |
| SALE-11 | Refund API | **PASS** | Endpoint existe (400 payload inválido) |
| SALE-12 | POS Index | **PASS** | HTTP 200 |

---

## Gaps bloqueantes (prompt / operación continua)

| ID | Escenario | Severidad | Resultado |
|----|-----------|-----------|-----------|
| SALE-GAP-01 | Happy Hour automático | High | **BLOCKED** (SB-10) |
| SALE-GAP-02 | Combos | High | **BLOCKED** (SB-09) |
| SALE-GAP-03 | Cierre de caja / arqueo | **Critical** | **BLOCKED** (SB-02) |
| SALE-GAP-04 | Precuenta / factura fiscal | **Critical** | **BLOCKED** (SB-03/04) |
| SALE-GAP-05 | Cortesía estructurada | Medium | **BLOCKED** |

---

## Escenarios del prompt — cobertura

| Escenario | Estado |
|-----------|--------|
| Venta normal / rápida / para llevar | Cubierto (rápida = mismo POS) |
| Delivery | Tipo OK; sin UI/courier |
| Efectivo / tarjeta / mixto / parcial | PASS |
| Dividido / unir cuentas | No re-ejecutado aquí; certificado en suites ORDER previas |
| Cambio mesero/cajero | Roles OK; no swap de turno formal |
| Reembolso | API presente |
| Cancelación | PASS vía kitchen cancel |
| Dos cajeros / concurrencia | Cubierto en ORDER_OPERATIONAL previa |
| Liberación automática mesa | Tras pago completo en flujo mixto |

---

## Validaciones

| Área | Resultado |
|------|-----------|
| Totales | PASS |
| Impuestos | Presentes en ítems (`taxRate`) |
| Propinas | No ejercitado en esta corrida |
| Inventario | Deducción en send |
| Caja / arqueo | **AUSENTE** |
| Auditoría | Cancel + payments con logs |
| Reportes | Existen; no re-validados exhaustivamente aquí |

---

## Conclusión

**POS core sí opera.** **Operación continua de restaurante (caja + fiscal) NO está completa** — bloqueadores Critical abiertos.
