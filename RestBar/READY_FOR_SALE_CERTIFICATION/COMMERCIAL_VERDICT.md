# COMMERCIAL VERDICT — ¿Se puede vender hoy?

**Fecha:** 2026-07-04

---

# READY FOR SALE: NO (con excepción piloto)

---

## Resumen ejecutivo

El módulo Order/Index está **técnicamente certificado** (119/119 tests operativos) y **comercialmente preparado para piloto** en restaurantes pequeños/medianos. **No está listo para venta SaaS self-service** ni para prometer caja fiscal, combos, o franquicia internacional sin los 13 SALE BLOCKERS documentados.

## ¿Se puede vender hoy?

| Pregunta | Respuesta |
|----------|-----------|
| **¿Sí o no?** | **Sí con piloto asistido. No como SaaS autoservicio.** |
| Restaurante pequeño | **Sí** — POS, KDS, pagos, multitenant |
| Bar / discoteca | **Sí parcial** — sin combos/happy hour auto |
| Restaurante mediano | **Sí piloto** — requiere onboarding asistido |
| Cadena multi-sucursal | **Sí piloto** — 119 tests + demo 30 mesas |
| Hotel / casino | **No** — falta caja, fiscal, VIP |
| Franquicia internacional | **No** — licensing, import, fiscal |
| Food court / dark kitchen | **Parcial** — routing OK, sin delivery UI |
| Catering | **No** — sin módulo catering |

## Qué NO prometer todavía

1. Planes SaaS con límites y cobro automático
2. Cierre de caja con arqueo
3. Precuenta y factura fiscal
4. Combos y happy hour automático
5. Importación desde otro POS
6. Exportación completa self-service
7. Onboarding sin soporte técnico

## Qué corregir obligatoriamente antes de vender SaaS

| Prioridad | Item |
|-----------|------|
| P0 | Módulo caja (SB-02) |
| P0 | Precuenta + factura (SB-03, SB-04) |
| P0 | Planes y suspensión billing (SB-01) — *suspensión operativa ya implementada* |
| P1 | Onboarding wizard (esc. 29-30) |
| P1 | Export tenant (SB-07) |
| P2 | Combos, happy hour (SB-09, SB-10) |

## Roadmap aceptable post-venta piloto

- Impresión térmica con retry
- VIP cliente y alérgenos estructurados
- Load test 500+ concurrentes
- SignalR offline E2E
- Delivery UI

## Evidencia

| Suite | Resultado |
|-------|-----------|
| Operativa (1-28) | 119/119 PASS |
| Ready for Sale (29-67) | 28 PASS, 13 SALE BLOCKER |
| Demo comercial | `SeedCommercialDemo`: 30 mesas, 100 productos, 40 órdenes históricas |

## Comandos

```powershell
# Certificación operativa
powershell -File scripts\Run-AllCertifications.ps1

# Certificación comercial
powershell -File READY_FOR_SALE_CERTIFICATION\scripts\Run-ReadyForSaleCertification.ps1

# Cargar demo vendible
Invoke-RestMethod http://localhost:5001/Seed/SeedCommercialDemo
```

## Veredicto dual

| Certificación | Resultado |
|---------------|-----------|
| ORDER ENTERPRISE FUNCTIONAL | **PASS** |
| READY FOR SALE (SaaS comercial) | **FAIL** — 13 blockers |
| READY FOR SALE (piloto restaurante asistido) | **PASS** |
