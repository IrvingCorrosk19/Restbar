# 08 — SECURITY VALIDATION

**Fecha:** 2026-07-28 · Ambiente: `http://localhost:5001`

---

## Controles verificados en vivo

| Control | Resultado | Evidencia |
|---------|-----------|-----------|
| Auth cookie `RestBarAuth` | PASS | Login roles admin/chef/waiter/cashier |
| Mesero 403 en mesa no asignada | PASS | KDS-SEC-01 HTTP 403 |
| Mesero sin descuento | PASS | SALE-08 HTTP 403 |
| Inventarista sin PaymentAccess | PASS | XCUT-02 HTTP 403 |
| KitchenAccess en StationOrders | PASS | Chef 200; mesero no opera KDS |
| orderHub no anónimo libre | PASS | negotiate (auth pipeline) |
| Segundo tenant admin.b | PASS | XCUT-01 |

---

## Políticas relevantes (`Program.cs`)

| Policy | Roles |
|--------|-------|
| OrderAccess | admin, manager, supervisor, waiter, cashier, chef, bartender |
| KitchenAccess | admin, manager, supervisor, chef, bartender |
| PaymentAccess | admin, manager, supervisor, cashier, accountant |
| InventoryAccess | admin, manager, supervisor, accountant, inventarista |

---

## Hallazgos

1. **Positivo:** Asignación de mesas al mesero reduce superficie de IDOR entre pisos.
2. **Gap:** Sin módulo PO no hay matriz de permisos de aprobación/recepción que auditar.
3. **Residual:** `KitchenService` legacy no usado — no es riesgo directo de auth.
4. **Seed:** Usuarios desactivados no se reactivaban (FIXED) — riesgo de denegación de servicio operativa en demos.

---

## Conclusión seguridad (alcance PKS)

Controles de roles/tenant en **Kitchen/Sales core: PASS**.  
Seguridad de **Purchasing: N/A** (módulo inexistente) — no certificable.
