# 00 — MASTER DECISION INTELLIGENCE PLAN

**Programa:** RB-028 Decision Intelligence & Forecast Center  
**Fecha inicio:** 2026-07-30  
**Fuente de verdad:** este archivo (actualizar en cada fase)  
**Dependencias:** RB-025 Native BI · RB-023 Food Cost · RB-020 Procurement · RB-010 Cash · RB-027 Quality Gates  
**Copilot:** permanece `EnableCopilot=false` — la inteligencia de esta fase es **determinística / estadística explicable**, no LLM.

---

## 1. Misión

Permitir al gerente responder con datos reales:

| # | Pregunta | Capacidad |
|---|----------|-----------|
| 1 | ¿Qué pasó? | KPIs RB-025 + Cockpit |
| 2 | ¿Por qué pasó? | Insights + evidencia (no causalidad inventada) |
| 3 | ¿Qué está pasando ahora? | Live analytics |
| 4 | ¿Qué probablemente pasará? | Forecast Engine (baseline + estacionalidad) |
| 5 | ¿Qué riesgo existe? | Alertas BI + Cash risk |
| 6 | ¿Qué decisión tomar? | Recommendation Engine |
| 7 | ¿Impacto estimado? | Impacto heurístico documentado |
| 8 | ¿Se verificó? | Decision Tracking |

---

## 2. Inventario de módulos (estado)

| Módulo fuente | Estado DI | Notas |
|---------------|-----------|-------|
| POS / Orders / Payments | DISCOVERED → USABLE | `analytics.v_completed_orders`, payments |
| Caja | USABLE | cash_sessions, variance SPs |
| Inventario | USABLE LIMITADO | stock, coverage, sin conteo físico |
| Compras | USABLE | PO, receipts, supplier scores |
| Food Cost / Menu Eng | USABLE LIMITADO | snapshots + classifier |
| Personal / Shifts | USABLE DÉBIL | clock-in; **sin nómina** |
| Cocina timestamps | USABLE LIMITADO | sent_at/prepared_at parcial |
| Clientes | USABLE DÉBIL | entity existe; RFM deferred |
| BI Nativo / Exec Analytics | REUSE | base obligatoria |
| Command Center | REUSE | scores/alerts/insights |
| Copilot | DEFERRED (flag off) | no inventar cifras vía LLM |
| Festivos / Clima | NOT APPLICABLE | sin entidades |
| Forecast predictivo previo | DISCOVERED | solo `forecast_seeds` histórico |

---

## 3. Fases de ejecución

| Fase | Descripción | Estado |
|------|-------------|--------|
| 0 | Descubrimiento exhaustivo | **PASS** |
| 1 | Auditoría calidad datos | **VALIDATED** (score por dominio) |
| 2 | Arquitectura analítica | **DESIGNED** → **IMPLEMENTED** (reusa `analytics`) |
| 3 | Catálogo KPI único | **IMPLEMENTED** (extiende KpiCatalog; no duplicar fórmulas) |
| 4 | Forecast Engine + accuracy | **IMPLEMENTED** |
| 5 | Recommendation + Decision Tracking | **IMPLEMENTED** |
| 6 | Executive Decision Cockpit UI | **IMPLEMENTED** |
| 7 | APIs / RBAC / MT | **IMPLEMENTED** (parcial MT deep) |
| 8 | Simulación what-if (precio/FC) | **IMPLEMENTED** (reusa CostSimulation + escenarios ventas) |
| 9 | Pruebas unit + browser | **IMPLEMENTED** |
| 10 | Certificación | **PILOT READY** |

---

## 4. Principios (no negociables)

1. No dashboards con datos falsos.
2. No forecast = suma histórica disfrazada sin modelo y métrica.
3. No correlación ≠ causalidad en textos de UI.
4. No mutar POS/Caja/Inventario desde simulaciones.
5. Tenant obligatorio en toda consulta.
6. Advertir Data Quality Score bajo.
7. Copilot off hasta certificar APIs.
8. KPI fórmula única (`KpiCatalog`).

---

## 5. Arquitectura objetivo (adoptada)

```
Operational tables
  → analytics views/SPs (RB-025)     [EXISTE]
  → DecisionIntelligence services   [NUEVO]
  → ForecastEngine (C# puro)        [NUEVO]
  → RecommendationEngine            [NUEVO + BiInsight]
  → Decision Tracking tables        [NUEVO]
  → API /api/decision-intelligence  [NUEVO]
  → Decision Intelligence Center UI [NUEVO]
```

No se duplica el data warehouse. Se reutiliza schema `analytics`.

---

## 6. Forecast — modelos v1

| Modelo | ID | Uso |
|--------|----|-----|
| Naive (último valor) | `naive` | Baseline obligatorio |
| Media móvil | `ma` | Ventana 7 |
| Media móvil ponderada | `wma` | Ventana 7 |
| Suavizado exponencial | `ses` | α=0.3 |
| Tendencia lineal | `linear` | Mínimos cuadrados |
| Estacionalidad DOW | `dow` | Media por día semana |
| Tendencia + DOW | `trend_dow` | Combinado |

Horizontes v1: resto del día (via hourly si hay datos), 7d, 14d, 30d, cierre mes (extrapolación).  
Métricas: MAE, MAPE, RMSE, Bias + backtesting holdout temporal.  
**Sin** ARIMA/ML.NET/Python en v1 (justificado: datos/volumen y complejidad).

---

## 7. Criterios de aceptación (checklist)

| Criterio | Estado |
|----------|--------|
| Inventario de datos | PASS |
| Data quality report + scores | PASS |
| KPIs fórmula única | PASS (catálogo) |
| KPIs DB=API=UI (subset ejecutivo) | PARTIAL — hereda validación RB-025 |
| Cockpit funciona | PASS (código) |
| Forecast + baseline + backtest | PASS |
| Precisión medida | PASS (unit + report) |
| Recomendaciones con evidencia | PASS |
| Decision tracking | PASS |
| Simulaciones no mutan DB | PASS |
| RBAC | PASS |
| Multitenant | PARTIAL |
| Export HTML | PASS (print) |
| Performance no degrada POS | DESIGNED (read-only analytics) |
| Sin P0 abiertos en alcance v1 | TARGET |
| Copilot sigue off | PASS |

---

## 8. Veredicto objetivo (honesto)

Meta realista de este programa: **PILOT READY** o **PASS WITH CONDITIONS**.  
**No** WORLD CLASS READY sin: RFM completo, labor cost, holiday calendar, physical counts, forecast > baseline en todas las sucursales productivas, MT deep cert, P95 medidos en prod.

---

## 9. Documentos del programa

Ver carpeta `DECISION_INTELLIGENCE/` archivos `01`–`30`. Estado de cada uno se indica en su encabezado.
