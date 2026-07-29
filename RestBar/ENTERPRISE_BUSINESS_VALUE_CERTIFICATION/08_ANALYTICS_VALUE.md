# 08 — VALOR ANALÍTICO (Fases 6 completa + 8)

---

# KPIs OPERATIVOS — COBERTURA

| KPI | Estado |
|-----|--------|
| Ventas por dimensión (empresa→producto) | ✅ ~70% |
| Tiempos cocina/bar | ⚠️ ~40% |
| Rotación mesas | ⚠️ ~50% |
| Food/Beverage/Prime Cost | ❌ 0% |
| Márgenes | ⚠️ ~35% (producto, no operación) |
| Costos por proveedor/ingrediente | ❌ |

**Cobertura KPI total verificada: ~32%**

---

# INTELIGENCIA DE NEGOCIO / PREDICTIVO (Fase 8)

| Capacidad | ¿Existe? | Evidencia |
|-----------|----------|-----------|
| Pronóstico ventas | ❌ | GrowthForecasts vacío en TrendAnalysis |
| Pronóstico compras | ❌ | Sin módulo compras |
| Pronóstico inventario | ❌ | Solo alertas umbral |
| Predicción agotados | ⚠️ | Reglas simples, no ML |
| Predicción desperdicio | ❌ | |
| Predicción ocupación/flujo | ❌ | |
| Predicción horarios pico | ⚠️ | Histórico manual GetDailySales |
| Predicción rentabilidad/crecimiento | ❌ | |

**RestBar hoy es sistema descriptivo (BI básico), no predictivo.**

### Para convertirse en sistema predictivo (orden ROI):

1. Data warehouse eventos (órdenes, tiempos, inventario, labor)
2. Forecast ventas por sucursal/día/hora (series temporales)
3. Reorder point automático ligado a PO
4. Alertas proactivas gerente (dashboard)
5. ML opcional fase 2 (demanda, staffing)

**Score BI/Analytics: 22/100**
