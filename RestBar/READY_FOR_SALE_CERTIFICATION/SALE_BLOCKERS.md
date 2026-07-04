# SALE BLOCKERS — Ready for Sale (Escenarios 29-67)

**Fecha:** 2026-07-04  
**Pruebas ejecutadas:** `scripts/Run-ReadyForSaleCertification.ps1`  
**Resultado:** 28 PASS · 18 FAIL/BLOCKER · **13 SALE BLOCKER**

---

## SALE BLOCKER (impiden venta comercial SaaS)

| ID | Escenario | Bloqueo | Acción requerida |
|----|-----------|---------|------------------|
| SB-01 | 35 Licenciamiento/planes | No hay planes básico/pro/enterprise ni límites | Modelo `Subscription` + enforcement |
| SB-02 | 42 Cierre de caja | No existe módulo caja (apertura/arqueo/cierre) | `CashRegister` entity + UI |
| SB-03 | 51 Precuenta | No hay flujo pre-bill | Endpoint + impresión precuenta |
| SB-04 | 52 Factura final | Modelo existe, sin generación en pago | Invoice flow post-payment |
| SB-05 | 50 Impresión térmica | Solo HTML receipt | Driver/API impresión + retry |
| SB-06 | 38 Migración POS | Sin import wizard | CSV/API import productos/mesas/usuarios |
| SB-07 | 39 Exportación completa | Solo audit CSV + payments JSON | Bulk tenant export self-service |
| SB-08 | 40 Cierre de día | Solo reporte JSON, sin ritual de cierre | Day-close workflow + bloqueo órdenes abiertas |
| SB-09 | 56 Combos | No implementado | Combo entity + pricing |
| SB-10 | 58 Happy hour | Precio manual únicamente | Motor precios por horario |
| SB-11 | 32 Caja dueño no técnico | Sin UI cierre caja | Mismo que SB-02 |
| SB-12 | 66 Hotel/casino/franquicia SaaS | Falta fiscal + licensing + escala | Fase 2 enterprise |
| SB-13 | 66-02 Franquicia internacional | Sin facturación multi-país | Por mercado |

---

## FINANCIAL BLOCKER (corregido)

| ID | Estado |
|----|--------|
| Descuento por mesero | **CORREGIDO** — `ApplyDiscount` requiere admin/manager/supervisor |

---

## UX BLOCKER (no bloquean venta piloto, sí venta self-service)

| Escenario | Gap |
|-----------|-----|
| 29 Onboarding wizard | CRUD manual, sin guía |
| 30 Primer día | Sin checklist in-app |
| 33 Soporte | Sin códigos de incidente |
| 54 VIP | Solo `Order.IsVip`, sin programa cliente |
| 55 Alérgenos | Notas libres, sin taxonomía |

---

## Implementado en esta fase

| Feature | Evidencia |
|---------|-----------|
| Demo comercial 30/100/40 | `GET /Seed/SeedCommercialDemo` — RFS-34 PASS |
| Suspensión SaaS tenant | `TenantSubscriptionMiddleware` + login block — RFS-36/37 PASS |
| Control descuentos | `OrderController.ApplyDiscount` role guard — RFS-43 PASS |
| Demo comercial flujo | RFS-31 PASS (orden→cocina→pago→auditoría) |
