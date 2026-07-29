# 07 — EXECUTIVE DASHBOARD

**Nombre producto:** RestBar Command Center  
**Promesa:** Estado del negocio en **< 5 segundos**. No es un dashboard bonito: es un centro de control.

---

# 1. Audiencias

| Vista | Usuario | Foco |
|-------|---------|------|
| Owner | Dueño / CEO cadena | Dinero, riesgos, sucursales |
| Manager | Gerente local | Hoy: ventas, cocina, caja, stock |
| Kitchen Lead | Chef | Estaciones, tiempos, merma |
| Buyer | Compras | Quiebres, PO sugeridos |
| Franchise HQ | Franquiciador | Benchmark locales |

Una sola app; **roles filtran widgets**.

---

# 2. Layout Command Center (Owner / Manager)

```
┌─────────────────────────────────────────────────────────────┐
│  RELOJ NEGOCIO: Abierto ●  Ventas hoy $X  vs ayer ±%       │
│  [Alertas críticas: 3]  [Riesgos: 2]  [Oportunidades: 4]   │
├──────────────┬──────────────┬──────────────┬────────────────┤
│ FINANCIERO   │ OPERATIVO    │ COCINA/BAR   │ INVENTARIO     │
│ Ventas       │ Mesas occ.   │ SLA estaciones│ Stock crítico │
│ Ticket prom. │ Rotación     │ Cola KDS      │ Merma hoy     │
│ Cobros       │ Tiempo mesa  │ Retrasos      │ FC teórico    │
│ Caja status  │ Waiters top  │               │               │
├──────────────┴──────────────┴──────────────┴────────────────┤
│ SUCURSALES / FRANQUICIAS (heatmap: verde/ámbar/rojo)        │
├─────────────────────────────────────────────────────────────┤
│ COPILOT STRIP: “3 acciones recomendadas para hoy” → Ejecutar│
├─────────────────────────────────────────────────────────────┤
│ PRONÓSTICO 7 DÍAS (sparkline) │ PROMO LIFT │ CLIENTES CHURN │
└─────────────────────────────────────────────────────────────┘
```

---

# 3. Widgets obligatorios (<5s)

| Widget | Fuente datos (extender) | Estado objetivo |
|--------|-------------------------|-----------------|
| Ventas hoy / vs LY / vs target | SalesReportService | P1 |
| Ticket promedio | Idem | P1 |
| Estado caja (abierta/cerrada/diff) | CashSession | P0→P1 |
| Mesas ocupación + rotación | Table + Orders | P1 |
| Estaciones SLA (cola, tiempo) | Kitchen + OrderItems | P1 |
| Stock crítico | InventoryAnalysis | P1 |
| Food cost flash (7d) | Recipe + PO | P1 |
| Top/bottom productos margen | Profitability | P1 |
| Sucursales heatmap | GetBranchSales | P1 |
| Alertas | Notification + rules engine | P1 |
| Acciones Copilot | Rules → luego IA | P2–P3 |

---

# 4. Alertas críticas (ejemplos)

| Severidad | Ejemplo | Acción sugerida |
|-----------|---------|-----------------|
| P1 | Caja abierta >14h sin cierre | Forzar cierre |
| P1 | Estación Grill SLA >15 min | Reasignar / alerta chef |
| P1 | Stock 0 en top-seller | PO urgente / 86 plato |
| P2 | Descuentos >X% del día | Auditoría |
| P2 | Sucursal −20% vs peer | Llamar gerente |
| P3 | Producto margen negativo | Revisar precio/receta |

---

# 5. Respuestas automáticas del Command Center

Debe poder responder sin salir de la pantalla:

1. ¿Cómo va el negocio hoy?  
2. ¿Dónde pierdo dinero esta semana?  
3. ¿Qué sucursal está en rojo?  
4. ¿Qué estación es el cuello de botella?  
5. ¿Qué debo comprar mañana? (si PO+forecast)  
6. ¿Qué 3 acciones ejecuto ahora?  

---

# 6. Requisitos no funcionales

| Req | Target |
|------|--------|
| TTI datos frescos | <5s (cache 30–60s OK) |
| Multitenant | Company/Branch scoped |
| Realtime | SignalR para cocina/caja alerts |
| Mobile | Responsive gerente |
| No N+1 | Endpoint agregado `/api/command-center/snapshot` |

---

# 7. Qué NO incluir en v1

- Gráficos decorativos sin acción  
- 40 KPIs en primera vista (máx 12 widgets)  
- Drill-down infinito (link a módulo, no clonar AdvancedReports)

---

# 8. Fases entrega

| Fase | Entrega |
|------|---------|
| CC-1 | Snapshot ventas+mesas+estaciones+stock (APIs existentes) |
| CC-2 | Caja + alertas reglas |
| CC-3 | Food cost + heatmap sucursales |
| CC-4 | Copilot strip + forecast sparkline |
