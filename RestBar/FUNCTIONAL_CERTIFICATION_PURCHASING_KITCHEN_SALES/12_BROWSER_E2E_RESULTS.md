# 12 — BROWSER E2E EXECUTED TESTS

**Fecha:** 2026-07-28  
**Ambiente:** Browser Cursor IDE → `http://localhost:5001`  
**Usuario:** `admin@restbar.com`  
**Evidencia:** `browser_evidence/pks-browser-*.png`  
**Estado E2E browser:** **COMPLETADO**

---

## Resultado browser

| ID | Flujo | Resultado | Evidencia |
|----|-------|----------|-----------|
| BR-01 | Login admin | **PASS** | Dashboard Home |
| BR-02 | Dashboard sin módulo Compras/Proveedores | **PASS** (gap confirmado) | `pks-browser-01-dashboard.png` |
| BR-03 | `/Supplier` UI | **BLOCKED** HTTP 404 blank | `pks-browser-02-supplier-missing.png` |
| BR-04 | `/PurchaseOrder` UI | **BLOCKED** ausente | CDP status 404 |
| BR-05 | POS `/Order/Index` mesas | **PASS** | `pks-browser-04-pos-tables.png` |
| BR-06 | Seleccionar mesa (T-10 / retest T-04) | **PASS** | OCUPADA / SELECCIONADA |
| BR-07 | Categoría Platos + agregar Hamburguesa Enterprise | **PASS** | Product cards |
| BR-08 | Confirmar → modal estación → Parrilla | **PASS** | `pks-browser-05-pos-after-confirm.png` |
| BR-09 | Envío a cocina (mesa ENPREPARACION) | **PASS** | `pks-browser-10-order-sent-t04.png` |
| BR-10 | KDS grill: orden visible | **PASS** | `pks-browser-07-kds-grill.png` |
| BR-11 | KDS Marcar como listo | **PASS** | “No hay órdenes pendientes” |
| BR-12 | Pago UI `onclick=processPayment()` sin args | **FAIL→FIXED** | `pks-browser-08-payment-dialog.png` |
| BR-13 | Pago vía browser fetch Amount=6.00 Efectivo | **PASS** | `isFullyPaid=true` |
| BR-15 | Pago UI completo (modal → Procesar Pago) T-04 | **PASS** | `pks-browser-11-payment-modal.png` → `pks-browser-12-payment-success.png` |
| BR-14 | Mesa P1-02 duplicada en UI | **FAIL** (dato/UI) | Screenshots POS |

---

## Flujo browser certificado (happy path) — cerrado

```
Login → POS Index → Mesa T-04 → Platos → Hamburguesa Enterprise
  → Confirmar Pedido → Estación Parrilla → Confirmar y Enviar
  → KDS grill → Marcar listo
  → showPaymentModal() → Efectivo $6.00 → Procesar Pago
  → Mesa T-04 DISPONIBLE
```

**Orden retest UI:** `493ff394-ca45-467e-b6c6-141f54b6d37d`  
**Post-pago:** T-04 = **DISPONIBLE** (liberación de mesa confirmada en UI).

---

## Defectos descubiertos SOLO por browser

| ID | Severidad | Descripción | Fix |
|----|-----------|-------------|-----|
| DEF-UI-PAY-01 | **Critical** | Botón modal `onclick="processPayment()"` sin monto/método → “Error del servidor al procesar el pago” | `submitBootstrapPaymentModal()` + validación en `processPayment` — **retest UI PASS** |
| DEF-UI-TAX-01 | High | `taxRate` 0.07 tratado como % (`/100`) en `order-ui.js` | Normalizar fraction vs percent |
| DEF-UI-TBL-01 | Medium | Mesa `P1-02` aparece duplicada en grid POS | OPEN (dato seed/UI) |

---

## Compras en browser

No existe navegación, menú ni página. Confirmado visualmente y por HTTP 404.

---

## Notas

- `/Order` (sin `/Index`) puede devolver documento vacío; usar `/Order/Index`.
- Admin al enviar orden debe elegir estación manualmente (modal).
- Residual de corridas previas: T-10 en `ParaPago`, T-05 `Ocupada` (no bloquean el happy path T-04).
