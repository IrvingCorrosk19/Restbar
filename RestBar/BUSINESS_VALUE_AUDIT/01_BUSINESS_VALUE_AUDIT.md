# 01 — AUDITORÍA DE VALOR DE NEGOCIO

**Fecha:** 2026-07-29  
**Metodología:** Comité ejecutivo + consultores hospitality  
**Base:** Certificaciones funcionales ejecutadas (119/119 operativos, PKS FAIL, RFS 13 blockers) — **sin asumir funcionalidades**

---

## Pregunta central

**¿Por qué un restaurante debería comprar RestBar en lugar de otro sistema?**

### Respuesta honesta (evidencia)

| Segmento | ¿Comprar RestBar? | Por qué |
|----------|-------------------|---------|
| Restaurante pequeño/mediano (1–3 locales) que necesita POS + cocina en tiempo real | **Sí, en piloto asistido** | Core certificado: mesa→orden→KDS→pago, multitenant, roles |
| Cadena que exige compras, caja, fiscal, combos, SaaS self-service | **No hoy** | 13 sale blockers + módulo compras ausente |
| Reemplazo directo de Toast / Oracle / Square en operación completa | **No** | Faltan caja, fiscal, impresión térmica, onboarding, escala comercial |

**Diferenciador verificable hoy:** plataforma **multitenant nativa** (empresa/sucursal) con **KDS multi-estación** (cocina + bar + pisos) y **pagos parciales/mixtos** certificados — a **menor madurez comercial** que líderes de mercado.

---

## 1. AHORRO DE DINERO

Escala: **Alto** · **Medio** · **Bajo** · **Nulo** · **No verificado**

| Área | Calificación | Evidencia |
|------|--------------|-----------|
| Desperdicio alimentos | **Bajo** | Descuento inventario al enviar a cocina (`TrackInventory`); sin mermas, caducidad ni forecast de compra |
| Desperdicio bebidas | **Bajo** | Stock por estación (`ProductStockAssignment`); sin control de pours/porcentaje bar |
| Errores de cocina | **Medio** | KDS por estación certificado (18/18); routing multi-piso; sin re-routing al cambiar mesa (DEF-KDS-001) |
| Errores de meseros | **Medio** | Orden digital + estados; permisos por rol; sin guía alérgenos estructurada |
| Errores de caja | **Nulo** | **No existe módulo caja** (SB-02, SB-11) |
| Cobros duplicados | **Medio-Alto** | Idempotencia en pagos certificada (PKS, ORDER) |
| Pérdidas por cancelaciones | **Medio** | Cancelación orden + log + restauración inventario; UI resumen corregida 2026-07-29 |
| Productos regalados / cortesías | **Bajo** | Descuentos con guard de rol (manager); sin workflow cortesía auditada |
| Pérdidas por inventario | **Medio-Bajo** | Alertas stock bajo, transferencias entre estaciones; **sin PO ni costeo de compras** |
| Fraude interno | **Medio** | Auditoría (`audit_logs`), permisos, descuentos restringidos; **sin arqueo de caja** |
| Tiempo perdido | **Medio-Alto** | SignalR cocina↔POS; menos papel en cocina certificado |
| Costos administrativos | **Bajo** | Reportes JSON existen; export PDF/Excel **stub** |
| Horas hombre / reprocesos | **Medio** | Flujo digital reduce ida y vuelta cocina; onboarding manual |
| Costos operativos totales | **Medio-Bajo** | Ahorro real concentrado en **cocina + cobro**; no en back-office |

---

## 2. INCREMENTO DE VENTAS

| Área | Calificación | Evidencia |
|------|--------------|-----------|
| Ticket promedio | **Bajo** | Sin upselling guiado, combos (SB-09), ni happy hour auto (SB-10) |
| Ventas por mesa / rotación | **Medio** | Utilización mesas en `AdvancedReports`; sin KPI tiempo real en dashboard |
| Velocidad de servicio | **Medio-Alto** | KDS + SignalR certificados |
| Experiencia cliente | **Medio-Bajo** | POS funcional; sin precuenta (SB-03), impresión térmica (SB-05) |
| Upselling / cross-selling | **Nulo** | No implementado |
| Promocionar alta rentabilidad | **Bajo** | Reporte rentabilidad producto/categoría (API real); sin prompts en POS |
| Productos más/menos vendidos | **Medio-Alto** | `GetTopProducts`, `GetTopSellingProducts` — APIs verificadas |
| Oportunidades comerciales | **Bajo** | Tendencias históricas parciales; **forecast vacío** |

---

## 3. EFICIENCIA OPERATIVA

| Flujo | Calificación | Evidencia |
|-------|--------------|-----------|
| Cocina | **Alto** | 18/18 PKS + browser E2E Parrilla→listo |
| Bar | **Medio-Alto** | Routing multi-bar certificado (ORDER_ROUTING 15/15) |
| Mesero | **Medio-Alto** | POS certificado 119 tests |
| Cajero | **Bajo** | Pagos OK; **sin caja ni cierre formal** |
| Gerente / supervisor | **Medio** | Reportes + roles; sin dashboard ejecutivo unificado |
| Comunicación áreas | **Medio-Alto** | SignalR orden/mesa/cocina |
| Cobro / cierre mesa | **Medio** | Pago parcial/mixto; sin precuenta ni ritual cierre día (SB-08) |

---

## 4. CONTROL GERENCIAL (tiempo real)

| Métrica | ¿Disponible? | Evidencia |
|---------|--------------|-----------|
| Ventas del día | **Parcial** | `GetSalesMetrics`, `PaymentView/DashboardStats` |
| Ventas por hora | **Parcial** | `GetDailySales` |
| Por sucursal / empresa | **Sí (reporte)** | `GetBranchSales`, multitenant 51/51 |
| Por mesero / cajero | **Parcial** | `GetEmployeeSales` |
| Por estación / categoría | **Parcial** | AdvancedReports estación; categorías en Reports |
| Ticket promedio | **Sí** | SalesReportService |
| Tiempos mesa/cocina/bar | **Parcial** | `GetStationPerformance`, `GetTableUtilization` — no wallboard |
| Productos agotados | **Parcial** | InventoryAnalysis low/out stock |
| Órdenes abiertas/canceladas | **Parcial** | Datos en BD + audit; no widget único tiempo real |
| Mesas disponibles/ocupadas | **Sí (POS)** | `GetActiveTables` + SignalR |

**Conclusión:** el gerente **no tiene un panel único en tiempo real** tipo command center; debe navegar POS + reportes.

---

## 5–11

Ver documentos 02–10 en esta carpeta.

---

## Madurez comercial estimada

| Dimensión | % |
|-----------|---|
| POS + KDS operativo | 85 |
| Pagos continuos (caja/fiscal) | 35 |
| Compras / proveedores | 7 |
| Reportes ejecutivos listos | 50 |
| SaaS comercial | 25 |
| **Promedio ponderado negocio restaurante completo** | **~48** |
