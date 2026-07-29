# 03 — AUDITORÍA DE REPORTES EJECUTIVOS

**Fuentes:** `ReportsController`, `AdvancedReportsController`, `PaymentViewController`, certificación RFS/TC3

---

## Reportes básicos (`/Reports`)

| Reporte | API | UI | Export | Valor decisión |
|---------|-----|-----|--------|----------------|
| Métricas ventas | `GetSalesMetrics` | SalesReport | PDF/Excel **stub** | **Alto** — ingresos, ticket, descuentos, margen |
| Ventas diarias | `GetDailySales` | ✅ | stub | **Alto** |
| Top productos | `GetTopProducts` | ✅ | stub | **Alto** |
| Por categoría | `GetCategorySales` | ✅ | stub | **Medio-Alto** |
| Por empleado | `GetEmployeeSales` | ✅ | stub | **Medio** — comisiones calculadas |
| Por sucursal | `GetBranchSales` | ✅ | stub | **Alto** (multilocal) |
| Descuentos | `GetDiscounts` | ✅ | stub | **Medio** — control fraude parcial |

---

## Reportes avanzados (`/AdvancedReports`)

| Reporte | Backend | UI JS | Valor | Gap |
|---------|---------|-------|-------|-----|
| Rentabilidad producto/categoría | ✅ Real | ✅ | **Alto** | Sin export producción |
| Análisis ventas | ✅ Real | ❌ JS faltante | **Medio** | Página rota |
| Análisis clientes | ✅ Real | ❌ JS faltante | **Bajo-Medio** | CRM limitado |
| Análisis operacional (estación/mesa) | ✅ Real | ❌ JS faltante | **Alto** | Página rota |
| Inventario | ✅ Real | ✅ | **Medio** | Sin enlace a compras |
| Proveedores | **Stub ceros** | ✅ | **Nulo** | Sin módulo compras |
| Tendencias | Parcial histórico | ✅ | **Medio** | Forecast vacío |
| Auditoría | ✅ Real | ✅ | **Medio-Alto** | |
| Salud sistema | ✅ Real | ✅ | **Bajo** (IT) | |

---

## Pagos (`/PaymentView`)

| Vista | Contenido | Rol |
|-------|-----------|-----|
| DashboardStats | Ingresos mes, pendientes | Finanzas/caja |
| Analytics | Métodos pago, tendencia 6 meses | Gerencia |
| Recent/Pending | Listados | Operación |

---

## Por rol ejecutivo

| Rol | Reportes útiles HOY | Información faltante |
|-----|---------------------|----------------------|
| **Gerencia General** | Ventas, sucursales, rentabilidad API | Dashboard único, P&L, forecast |
| **Operaciones** | Estación/mesa (API, UI rota) | Tiempo real wallboard, SLA cocina |
| **Finanzas/CFO** | Métricas ventas, pagos | Caja, food cost formal, fiscal |
| **Compras** | **Ninguno** | PO, proveedor, costo recepción |
| **Inventario** | Stock bajo/agotado | MRP, sugerido compra |
| **Cocina/Chef** | KDS (operativo, no reporte) | Tiempo prep histórico exportable |
| **Bar** | Igual cocina por estación | Costo pour |
| **Caja** | Pagos | Arqueo, diferencias |
| **Meseros** | Ventas por empleado | Propinas dashboard (tips existen en modelo) |
| **Auditoría** | Audit report | Export masivo tenant |
| **Director/Franquicia** | Branch sales multitenant | Consolidado financiero, royalties |
| **Franquicias** | Aislamiento tenant | Licensing, KPI red |

---

## Conclusión reportes

RestBar **genera datos** suficientes para decisiones tácticas (qué vende, quién vende, qué sucursal). **No entrega** paquete ejecutivo listo para junta directiva (export, forecast, food cost, caja).
