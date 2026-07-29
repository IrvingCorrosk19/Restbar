# 03 — VALOR FINANCIERO (Fase 6 parcial + ROI)

---

# KPIs FINANCIEROS — VALIDACIÓN

| KPI | Existe | Fuente / Gap |
|-----|--------|--------------|
| Ventas diarias/mensuales/anuales | ✅/⚠️ | GetDailySales, DashboardStats |
| Ventas por empresa/sucursal | ✅ | GetBranchSales, multitenant |
| Ventas por salón/piso | ⚠️ | Áreas/mesas; no reporte "piso" dedicado |
| Ventas por estación/cocina/bar | ⚠️ | GetStationPerformance |
| Ventas por categoría/producto | ✅ | GetCategorySales, GetTopProducts |
| Ventas por mesero/cajero/turno | ⚠️/❌ | GetEmployeeSales; turno/shift parcial |
| Ticket promedio/máx/mín | ⚠️ | Promedio sí; min/max no dedicado |
| Rotación mesas/salón | ⚠️ | GetTableUtilization |
| Tiempos cocina/bar/entrega/cobro | ⚠️/❌ | Estación parcial; entrega/cobro no E2E |
| **Food Cost** | ❌ | Sin costo compra integrado |
| **Beverage Cost** | ❌ | Idem |
| **Prime Cost** | ❌ | Sin labor cost integrado |
| Margen bruto producto/categoría | ✅ | AdvancedReports profitability |
| Margen neto / EBITDA | ❌ | Sin OPEX/labor |
| Costo desperdicio/devoluciones/cancelaciones | ⚠️/❌ | Cancel log; sin costo agregado |
| Costo descuentos/promociones | ⚠️ | GetDiscounts |
| Costo por proveedor/receta/ingrediente | ⚠️/❌ | Recetas sí; proveedor stub |

**Cobertura KPIs financieros verificables: ~30%**

---

# IMPACTO FINANCIERO NETO

| Palanca | Dirección | Magnitud |
|---------|-----------|----------|
| Reducción errores cocina/cobro | Ahorro | Media |
| Incremento ventas activo | Ingreso | **Nula hoy** |
| Control food cost | Ahorro | **Nula** |
| Control caja | Ahorro/fraude | **Nula** |
| Multitenant (cadena) | Ahorro IT | Alta en 5+ locales |

**¿Genera suficiente valor financiero para pagar?**  
- **Piloto 1–5 locales:** marginalmente **sí** si dolor = cocina.  
- **Cadena 20+ con CFO exigiendo prime cost:** **no**.
