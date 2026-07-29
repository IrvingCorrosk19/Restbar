# 06 — FIXES APPLIED

**Fecha:** 2026-07-28  
**Build:** `dotnet build RestBar.csproj` — 0 errores (warnings preexistentes)

---

## FIX-01 — Reactivación de usuarios canónicos en seed

**Archivo:** `Controllers/SeedController.cs` → `EnsureUserAsync`  
**Defect:** DEF-SEED-001  
**Cambio:** Si el usuario existe pero `IsActive != true`, se reactiva y se persiste (`UpdatedBy = Seeder`).

**Retest:** Login `mesero@restbar.com` OK · PKS-ENV-02 PASS

---

## FIX-02 — Null-guard en SendToKitchen

**Archivo:** `Controllers/OrderController.cs` → `SendToKitchen`  
**Defect:** DEF-ORD-001  
**Cambio:** Validación temprana `dto == null` → BadRequest controlado.

**Retest:** Flujos SendToKitchen con body válido PASS (KDS-04, SALE-01, etc.)

---

## FIX-03 — Suite de certificación PKS (harness)

**Archivo:** `FUNCTIONAL_CERTIFICATION_PURCHASING_KITCHEN_SALES/scripts/Run-PurchasingKitchenSalesCertification.ps1`

- Fallback de meseros activos
- Uso de mesas asignadas al waiter
- Payment con `remainingAmount` / Method `Efectivo`|`Tarjeta`
- `UpdateItemStation` con `NewStationId` y estación PSA válida
- Manejo HTML de StationOrders / Order Index

---

## FIX-04 — Browser: modal de pago llamaba processPayment() vacío

**Archivos:** `Views/Order/Index.cshtml`, `wwwroot/js/order/payments.js`  
**Defect:** DEF-UI-PAY-01  
**Cambio:** `submitBootstrapPaymentModal()` lee monto/método del formulario; `processPayment` valida args e incluye `idempotencyKey`.

**Retest:** Pago browser fetch → `isFullyPaid=true`; pago UI modal T-04 → mesa **DISPONIBLE** (`pks-browser-12-payment-success.png`)

---

## FIX-05 — Browser: taxRate fraction vs percent

**Archivo:** `wwwroot/js/order/order-ui.js`  
**Defect:** DEF-UI-TAX-01  
**Cambio:** Si `taxRate <= 1` se trata como fracción (0.07); si no, como porcentaje /100.

---

## No corregido (requiere feature)

| Defect | Motivo |
|--------|--------|
| DEF-PO-001..005 | Módulo Compras completo |
| DEF-SALE-001..005 | Caja, fiscal, combos, Happy Hour, cortesía |
| DEF-KDS-001 | Re-routing en MoveToTable |
| DEF-UI-TBL-01 | Mesa P1-02 duplicada |

---

## Compilación

```
dotnet build RestBar.csproj -c Debug
→ 0 Error(s)
```

App re-ejecutada en `http://localhost:5001` post-fix.  
Browser E2E: ver `12_BROWSER_E2E_RESULTS.md`.
