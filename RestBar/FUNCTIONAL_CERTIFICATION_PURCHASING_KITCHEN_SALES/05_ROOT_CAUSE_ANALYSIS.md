# 05 — ROOT CAUSE ANALYSIS

## DEF-PO-001 / DEF-PO-002 — Purchasing ausente

**Síntoma:** Todos los endpoints `/PurchaseOrder/*` y `/Supplier/*` responden 404; escenarios de OC no ejecutables.

**Causa raíz:** El producto nunca implementó el bounded context de compras. Existe solo:
- Movimiento de inventario `InventoryMovementType.Purchase` (entrada inmediata de stock)
- JS huérfano `wwwroot/js/supplier/supplier-management.js`
- ViewModels stub en `AdvancedReportsService.GetSupplierAnalysisAsync` (“el sistema no tiene proveedores aún”)
- Migraciones históricas mencionadas en SQL legacy (`AddPurchaseOrders`) **no presentes** en el modelo EF actual

**Por qué no es un bug de configuración:** No hay DbSet, servicios, políticas ni vistas. Es ausencia de feature.

**Impacto negocio:** Un restaurante no puede gestionar proveedores, OC, recepciones parciales ni conciliación de costos de compra.

---

## DEF-SALE-001 / DEF-SALE-002 — Caja y fiscal

**Síntoma:** No hay API/UI de apertura-cierre de caja ni generación de precuenta/factura al pagar.

**Causa raíz:** El dominio Payment cierra órdenes (`api/Payment/partial`) pero no se modeló `CashRegister` / session de caja ni pipeline de documentos fiscales. Confirmado en READY_FOR_SALE (SB-02, SB-03, SB-04).

**Impacto:** Impide operación continua auditada de un local real y venta comercial con cumplimiento fiscal.

---

## DEF-SEED-001 — Mesero inactivo

**Síntoma:** `Get-CertSession mesero@restbar.com` fallaba; flujos kitchen/sales con waiter → FAIL/403 engañosos.

**Causa raíz:** `users.is_active = false` para el mesero canónico. `SeedController.EnsureUserAsync` solo creaba si no existía; **no reactivaba**. Certificaciones previas o pruebas manuales pudieron desactivar el usuario.

**Fix:** Reactivar en seed si `IsActive != true`.

---

## DEF-ORD-001 — NRE en SendToKitchen

**Síntoma:** Body ausente/null → `Object reference not set to an instance of an object` (HTTP 400 genérico).

**Causa raíz:** Acceso a `dto.TableId` sin null-check de `dto`.

**Fix:** Guard `if (dto == null) return BadRequest(...)`.

---

## Falso positivo 403 en mesa P1 (no defecto de seguridad)

**Síntoma inicial:** Waiter 403 en `SendToKitchen` con mesa `P1-01`.

**Causa raíz:** Diseño correcto — mesero solo ve mesas asignadas; `ValidateTableTenantAccessAsync` bloquea mesas fuera de asignación. El script de certificación usaba mesa multi-piso no asignada.

**Acción:** Ajustar pruebas a mesas del waiter; documentar KDS-SEC-01 como PASS de seguridad.
