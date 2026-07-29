# 06 — VALOR GERENCIAL (Fase 5)

**¿Puede RestBar responder automáticamente?**

| Pregunta | ¿Auto? | Módulo / API |
|----------|--------|--------------|
| ¿Qué comprar mañana? | ❌ | Falta PO + forecast compras |
| ¿Qué proveedor conviene? | ❌ | Supplier stub |
| ¿Qué ingredientes se agotarán? | ⚠️ | InventoryAnalysis low stock |
| ¿Qué productos casi no se venden? | ⚠️ | Top products inverso manual |
| ¿Qué productos eliminar? | ⚠️ | Rentabilidad API |
| ¿Qué promocionar? | ❌ | Sin motor promo + HH |
| ¿Plato mayor utilidad? | ✅ | GetProductProfitability |
| ¿Plato con pérdidas? | ⚠️ | Margen negativo si costo cargado |
| ¿Sucursal gana más? | ✅ | GetBranchSales |
| ¿Sucursal pierde? | ⚠️ | Comparar manual; sin P&L |
| ¿Estación cuello de botella? | ⚠️ | GetStationPerformance |
| ¿Cocinero más rápido? | ❌ | Sin KPI por usuario cocina |
| ¿Mesero vende más? | ✅ | GetEmployeeSales |
| ¿Cajero recauda más? | ⚠️ | Parcial por payments |
| ¿Horario más ingresos? | ⚠️ | GetDailySales por hora |
| ¿Día contratar más personal? | ❌ | Sin labor forecast |
| ¿Promociones funcionan? | ❌ | No existen |
| ¿Clientes top / churn? | ⚠️ | GetTopCustomers; CRM débil |

**Score toma de decisiones automática: 35/100**

Gerente **debe exportar a Excel** para la mayoría de decisiones estratégicas.
