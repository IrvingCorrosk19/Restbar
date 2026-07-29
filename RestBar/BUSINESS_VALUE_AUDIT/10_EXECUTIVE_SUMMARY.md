# 10 — RESUMEN EJECUTIVO

**Auditoría:** Valor de negocio RestBar  
**Fecha:** 2026-07-29  
**Comité:** CEO cadena · Gerencia · CFO · Compras · Chef · Consultores hospitality

---

# VEREDICTO FINAL

## Clasificación del producto

# **LISTO PARA PILOTOS**

*(No es MVP de concepto — el core opera certificado. No está listo para comercialización SaaS ni enterprise competitivo.)*

**Madurez comercial: 48 / 100**

| Dimensión | Score |
|-----------|-------|
| POS + pagos core | 85 |
| KDS / cocina | 88 |
| Multitenant | 82 |
| Reportes ejecutivos | 50 |
| Compras / food cost | 7 |
| Caja / fiscal / continuidad | 30 |
| SaaS comercial | 25 |

---

## ¿Por qué comprar RestBar hoy?

1. **Necesitas POS + KDS en una o varias sucursales** con aislamiento por empresa — **certificado**.
2. **Routing cocina/bar complejo** (varias estaciones, pisos) — **certificado 15/15**.
3. **Pagos parciales/mixtos** sin duplicar cobros — **certificado**.
4. **Presupuesto menor** que suite enterprise y disposición a **piloto asistido**.
5. **Control multitenant** para franquicia regional en una sola plataforma.

## ¿Por qué NO comprar RestBar hoy?

1. Necesitas **caja, precuenta y factura fiscal integradas** — **no existen**.
2. Necesitas **compras y proveedores** — **no existen** (404 verificado).
3. Necesitas **combos, happy hour, impresión térmica** — **no existen**.
4. Esperas **onboarding self-service** como Square/Toast — **no existe**.
5. Compites en **hotel, casino o franquicia internacional regulada** — **no preparado**.

---

## Problemas reales que SÍ resuelve

| Problema | Solución RestBar |
|----------|------------------|
| Órdenes perdidas entre salón y cocina | KDS + SignalR |
| Cobro desorganizado en mesa grande | Pagos parciales + split bill |
| Varias marcas/sucursales mezcladas | Multitenant certificado |
| Descuentos no autorizados | Guard por rol |
| Sin visibilidad qué vende | Reportes ventas API |
| Inventario se agota en estación | Stock por estación + alertas |

---

## Problemas importantes que NO resuelve

| Problema | Estado |
|----------|--------|
| Cuadrar caja al cierre | ❌ |
| Cumplir facturación electrónica | ❌ |
| Comprar al proveedor correcto al precio correcto | ❌ |
| Saber food cost % real | ❌ |
| Predecir demanda próxima semana | ❌ |
| Migrar desde otro POS sin consultor | ❌ |
| Operar 100% igual que Toast en checklist estándar | ❌ |

---

## Indispensable para competir en mercado

1. Caja + arqueo  
2. Precuenta + impresión operativa  
3. Fiscal (mínimo un mercado)  
4. Compras + proveedores  
5. Export reportes ejecutivos  
6. Combos  
7. Onboarding import  

---

## Módulos antes de vender comercialmente (orden)

1. **Caja**  
2. **Precuenta / impresión**  
3. **Compras**  
4. **Cierre de día**  
5. **Fiscal país piloto**  
6. **SaaS billing**  

---

## ¿Pagaría un restaurante hoy?

| Perfil | ¿Paga? | Por qué |
|--------|--------|---------|
| Restaurante 1 local, cocina caótica, sin fiscal estricta | **Sí** (piloto $200–500/mes + setup) | ROI en KDS |
| Cadena 3–10 locales regional | **Sí** (piloto, precio custom) | Multitenant |
| Restaurante exige factura + caja día 1 | **No** | Gap P0 |
| Comparación activa con Toast/Square | **Probablemente no** | Checklist pierde |
| Consultora que implementa y customiza | **Sí** | Base extensible |

---

## Mayor ROI para el cliente (priorizar desarrollo)

1. **Caja** — elimina proceso paralelo #1  
2. **Compras** — control food cost (30–35% ventas típico)  
3. **KDS ya entregado** — acelerar adopción en ventas  
4. **Precuenta** — acelera rotación mesas  
5. **Combos/HH** — incremento ticket directo  

---

## Evidencia citada

| Artefacto | Resultado |
|-----------|-----------|
| ORDER_OPERATIONAL | 119/119 PASS |
| PKS | 39 PASS · 47 BLOCKED · FAIL |
| RFS | 13 SALE BLOCKERS |
| Multitenant | 51/51 PASS local |
| Browser E2E | POS→KDS→pago PASS |
| COMMERCIAL_VERDICT | Piloto sí · SaaS no |

---

## Frase para el inversionista

> RestBar **ya resuelve el problema de la cocina conectada al salón en múltiples sucursales**, con evidencia de certificación funcional. **Aún no resuelve el problema del dueño de cuadrar dinero, comprar insumos y cumplir fiscal** — por eso es **Listo para Pilotos**, no **Listo para Comercialización**.

---

**Documentos relacionados:** `01`–`09` en `BUSINESS_VALUE_AUDIT/`
