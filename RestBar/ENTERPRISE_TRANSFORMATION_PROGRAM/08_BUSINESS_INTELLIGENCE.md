# 08 — BUSINESS INTELLIGENCE

**Diseño only.** RestBar pasa de reportes descriptivos a plataforma de decisiones.

---

# 1. Principio

> Un KPI sin decisión asociada no se publica.  
> Un reporte sin export o acción se oculta.

---

# 2. Arquitectura BI propuesta

```
OLTP (PostgreSQL RestBar)
    │ CDC / nightly ETL (job hosted)
    ▼
Analytics schema / Warehouse ligero (mismo PG schema `bi_*` año 1)
    │
    ├── Fact: sales_line, payment, inventory_move, labor_hour (futuro)
    ├── Dim: date, branch, product, station, employee, supplier, customer
    │
    ▼
KPI Service (materialized views / snapshot tables)
    │
    ├── Command Center API
    ├── Decision API (“qué promocionar”)
    ├── Export Service (PDF/Excel real)
    └── Future: ML feature store
```

**Año 1:** no Snowflake. Schema `bi` en Postgres + jobs.  
**Año 2:** warehouse externo si volumen cadena lo exige.

---

# 3. Capas analíticas

| Capa | Contenido | Madurez destino |
|------|-----------|-----------------|
| Descriptiva | Qué pasó | Ya parcial → completar |
| Diagnóstica | Por qué | Varianzas FC, SLA, descuentos |
| Predictiva | Qué pasará | Forecast ventas/stock |
| Prescriptiva | Qué hacer | Reglas + Copilot |

---

# 4. Decisiones que el sistema debe responder

| Decisión | Datos mínimos | Módulo |
|----------|---------------|--------|
| ¿Dónde pierdo dinero? | FC variance, descuentos, merma, caja diff | Cost + Cash |
| ¿Qué comprar mañana? | Stock, sales velocity, recipe, lead time | Reorder |
| ¿Cambiar proveedor? | Precio, lead time, fill rate | Supplier score |
| ¿Eliminar plato? | Margen, mix %, popularity | Menu eng. |
| ¿Promocionar qué? | Elasticidad proxy, margen, stock | Promo |
| ¿Sucursal fallando? | Ventas, ticket, FC, SLA vs peer | Branch BM |
| ¿Estación lenta? | Prep time P50/P90 | Kitchen |
| ¿Cocinero capacitar? | Tiempo por usuario estación | KDS user |
| ¿Mesero vende menos? | Sales/employee, upsell rate | POS |
| ¿Promo funciona? | Lift vs baseline | Promo |
| ¿Dead stock? | Días sin venta | Inventory |
| ¿Mayor utilidad? | Contribution margin | Recipe cost |
| ¿Más personal? | Ventas/hora, occupancy | Labor+sales |
| ¿Clientes churn? | Recencia | CRM |
| ¿Producir mañana? | Forecast × recipe | Prod plan |
| ¿Se agota inventario? | Days of cover | Reorder |
| ¿Qué hacer hoy? | Scorecard alertas | Copilot |

---

# 5. KPIs canónicos (solo los que mueven dinero)

**Ingresos:** ventas, ticket, mix categoría, promo lift, rotación mesa  
**Costos:** food cost %, beverage %, waste $, discount $, cash variance  
**Ops:** ticket time cocina, % on-time, stockouts  
**Gente:** ventas/mesero, horas (futuro), tips  
**Unidad:** ventas/branch, FC/branch, ranking  

**Fuera del core v1:** EBITDA completo, si no hay OPEX cargado (evitar KPI falso).

---

# 6. Forecast (diseño)

| Modelo | Input | Output | Fase |
|--------|-------|--------|------|
| Naive estacional | ventas 8–12 sem | demanda día/hora | P2 |
| Reorder point | velocity + lead time + safety | qty sugerida PO | P2 |
| 86 predictor | stock + velocity | riesgo agotado | P2 |
| Occupancy | histórico mesas | staffing hint | P2–P3 |

ML avanzado solo cuando haya ≥6 meses datos limpios multi-branch.

---

# 7. Alertas & benchmarks

- Thresholds configurables por Company  
- Peer benchmark: branch vs mediana Company  
- Franchise benchmark: local vs marca  

---

# 8. Jobs requeridos (hoy no existen)

1. `BiNightlyAggregationJob`  
2. `AlertEvaluationJob` (cada 5–15 min)  
3. `ForecastJob` diario  
4. `BackupJob` real (ops)

---

# 9. Anti-patrones a evitar

- Duplicar AdvancedReports en BI  
- KPIs sin dueño de dato  
- Forecast vacío en UI (como hoy GrowthForecasts)  
- Export stub
