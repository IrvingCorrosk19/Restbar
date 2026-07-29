# 01 — AUDITORÍA EJECUTIVA (Fases 1–2)

**Certificación:** Ultimate Enterprise Business Value  
**Fecha:** 2026-07-29  
**Evidencia:** ORDER 119/119 · PKS 39P/47B · RFS 13 blockers · Multitenant 51/51 · Browser E2E

**Escala:** ✅ Resuelve · ⚠️ Parcial · ❌ No resuelve · 🔍 No verificado

---

# FASE 1 — PROBLEMAS DEL CLIENTE (OPERACIÓN)

| Problema | Calificación | Evidencia |
|----------|--------------|-----------|
| Pedidos perdidos | ⚠️ Parcial | KDS + SignalR reducen; sin offline certificado |
| Pedidos duplicados | ⚠️ Parcial | Idempotencia pagos; no idempotencia orden completa |
| Cocina desorganizada | ✅ Resuelve | KDS 18/18, routing 15/15, browser E2E |
| Bar desorganizado | ✅ Resuelve | Multi-bar routing certificado |
| Meseros desorganizados | ⚠️ Parcial | POS digital; sin app móvil nativa |
| Mesas ocupadas demasiado tiempo | ⚠️ Parcial | Utilización mesa en reportes; sin SLA alertas |
| Cobros lentos | ⚠️ Parcial | Pagos parciales OK; **sin precuenta** (SB-03) |
| Errores humanos | ⚠️ Parcial | Digitalización reduce; alérgenos libres |
| Mala comunicación cocina-salón | ✅ Resuelve | SignalR + KDS certificado |
| Clientes molestos por tiempos | ⚠️ Parcial | KDS acelera; sin visibilidad cliente |
| Productos equivocados | ⚠️ Parcial | Estación por producto; sin validación alérgeno |
| Retrasos | ⚠️ Parcial | KDS ayuda; sin métricas SLA tiempo real |
| Colas / esperas | ❌ | No módulo cola/QMS |
| Falta coordinación | ⚠️ Parcial | Mejora cocina-salón; compras fuera sistema |
| Cancelaciones | ✅ Resuelve | API + log + inventario restore; UI fix 2026-07-29 |
| Cambios de pedido | ⚠️ Parcial | Update qty/items; limitaciones post-cocina |
| Pagos parciales | ✅ Resuelve | Certificado PKS/ORDER |
| Dividir cuentas | ✅ Resuelve | Split bill API certificada |
| Cambio de mesa | ⚠️ Parcial | Intra-tenant OK; **sin re-routing KDS** (DEF-KDS-001) |
| Cambio mesero/cajero | ❌ | Sin handoff formal mesero (shifts existen) |

**Resumen Fase 1:** RestBar **resuelve bien el eje cocina↔salón**. **No resuelve** caja, colas, handoffs formales ni coordinación back-office.

---

# FASE 2 — AHORRO DE DINERO

| Área de ahorro | Impacto | Explicación |
|----------------|---------|-------------|
| Errores operativos | **Medio** | KDS + roles + auditoría |
| Desperdicio alimentos | **Bajo** | Stock estación; sin mermas/caducidad/forecast |
| Tiempo muerto cocina | **Medio-Alto** | KDS elimina re-lectura papel |
| Horas hombre admin | **Bajo** | Reportes API sí; export stub → Excel manual |
| Papel / impresiones | **Medio** | KDS digital; **sin térmica** cocina legacy |
| Llamadas cocina-salón | **Alto** | SignalR sustituye gritos/tickets perdidos |
| Recorridos mesero | **Bajo-Medio** | Menos idas a cocina por status |
| Productos mal preparados | **Medio** | Routing estación correcta |
| Devoluciones / reclamos | **Bajo** | Sin módulo reclamos |
| Errores de cobro | **Medio-Alto** | Idempotencia pagos |
| Fraude | **Medio-Bajo** | Auditoría + descuentos rol; **sin arqueo caja** |
| Descuentos no autorizados | **Medio-Alto** | Waiter 403 en ApplyDiscount |
| Inventario | **Medio-Bajo** | Alertas stock; sin PO |
| Compras / vencimientos / rotación | **Nulo** | Módulo compras ausente |

**Ahorro anual estimado (restaurante mediano, solo capacidades verificadas):** USD $8,000–$25,000 en piloto POS+KDS. **No cuantificable** en food cost/compras.
