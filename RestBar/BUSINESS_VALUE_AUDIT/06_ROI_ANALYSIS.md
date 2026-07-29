# 06 — ANÁLISIS DE ROI (RETORNO DE INVERSIÓN)

**Supuestos:** Restaurante mediano 30 mesas · Inversión software $3,000–$8,000/año · Implementación asistida $2,000–$5,000 one-time  
**Base:** solo capacidades verificadas — escenarios conservadores

---

## ¿Cómo ayuda RestBar a recuperar la inversión?

| Palanca ROI | Mecanismo verificado | Impacto estimado anual | Confianza |
|-------------|----------------------|------------------------|-----------|
| **Menos errores cocina** | KDS + routing estaciones | $2,000–$8,000 (menos rehacer platos) | Media |
| **Menos cobros duplicados/evitados** | Idempotencia pagos | $500–$2,000 | Alta |
| **Mayor rotación (indirecto)** | SignalR + KDS más rápido | 2–5% ventas si bien adoptado | Baja-Media |
| **Control descuentos** | Rol manager | $1,000–$5,000 fraude evitado | Media |
| **Multilocal sin reimplementar** | Multitenant certificado | Ahorro IT $5,000–$20,000 vs silos | Alta (cadenas) |
| **Menos papel cocina** | KDS digital | $500–$1,500 supplies | Media |

**ROI positivo plausible en piloto** si el dolor principal es **cocina desorganizada + multisucursal** y el cliente **no depende** de caja fiscal integrada hoy.

---

## ¿Dónde NO genera ROI hoy?

| Área | Por qué |
|------|---------|
| Compras / food cost | Módulo ausente — no reduce compras ni mermas sistemáticamente |
| Caja | Sigue necesitando proceso paralelo (Excel/caja registradora) |
| Fiscal | Contador manual o segundo sistema |
| Upselling combos/HH | Ingreso incremental no capturado |
| Automatización admin | Export/reportes incompletos → horas hombre persisten |

---

## Por stakeholder

### Propietario
- **Beneficio:** visibilidad ventas por sucursal, base SaaS escalable, menor costo que Oracle/Toast en piloto.
- **Riesgo:** prometer funciones no built → churn, multas fiscales si no hay segundo sistema.

### Gerente
- **Beneficio:** KDS, mesas en tiempo real, reportes ventas API.
- **Gap:** sin panel único ni cierre día; trabajo manual persiste.

### Personal operativo
- **Beneficio:** menos gritos cocina, POS claro, pagos parciales.
- **Gap:** curva aprendizaje sin onboarding; impresión térmica ausente.

---

## Payback estimado (piloto POS+KDS)

| Escenario | Payback |
|-----------|---------|
| Solo reemplazo papel cocina + errores | 12–18 meses |
| Cadena 3 sucursales (multitenant) | 8–14 meses |
| Restaurante que **requiere** caja+fiscal+compras integrados | **No payback** — necesita segundo software (+costo) |

---

## Riesgos que reduce (verificado)

- Fugas multitenant (51 casos PASS)
- Cobro duplicado (idempotencia)
- Descuentos no autorizados (403 waiter)
- Órdenes cruzadas entre empresas (403 IDOR)

## Riesgos que NO reduce

- Diferencia de caja efectivo
- Incumplimiento fiscal
- Sobrestock / quiebre por compras
- Lock-in por falta de export/import
