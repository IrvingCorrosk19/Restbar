namespace RestBar.Domain.Analytics;

/// <summary>Official KPI catalog — single source of truth for formulas and availability.</summary>
public static class KpiCatalog
{
    public static IReadOnlyList<KpiDefinition> All { get; } = Build();

    public static KpiDefinition? Get(string code) => All.FirstOrDefault(k => k.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    private static List<KpiDefinition> Build() =>
    [
        K("EXE.SALES_GROSS", "Ventas brutas", "Suma total_amount órdenes Completed",
            "SUM(orders.total_amount) WHERE status=Completed", "orders", "money", "Analytics.Executive", KpiAvailability.Available),
        K("EXE.SALES_NET", "Ventas netas", "Ventas menos descuentos de orden",
            "SUM(total_amount) (discount already reflected in total when applied at close)", "orders", "money", "Analytics.Executive",
            KpiAvailability.AvailableWithLimitations, "Depende de cómo POS aplica discount_amount vs total_amount"),
        K("EXE.TAX", "Impuestos", "Impuestos cobrados", "—", "—", "money", "Analytics.Executive",
            KpiAvailability.NotAvailable, "No hay fact tax total consolidado confiable por orden en analytics"),
        K("EXE.DISCOUNTS", "Descuentos", "discount_amount en órdenes del periodo",
            "SUM(orders.discount_amount)", "orders.discount_amount", "money", "Analytics.Sales", KpiAvailability.Available),
        K("EXE.TIPS", "Propinas", "Suma tip_amount pagos no anulados",
            "SUM(payments.tip_amount) WHERE NOT is_voided", "payments.tip_amount", "money", "Analytics.Sales", KpiAvailability.Available),
        K("EXE.REFUNDS", "Reembolsos", "Reembolsos de caja/pagos",
            "cash_sessions.total_refunds + payment_refunds", "cash/payments", "money", "Analytics.Cash",
            KpiAvailability.AvailableWithLimitations, "Preferir cash_sessions.total_refunds en periodo de sesiones"),
        K("EXE.AVG_TICKET", "Ticket promedio", "Ventas / órdenes completed",
            "revenue / completed_count", "analytics.sp_sales_summary", "money", "Analytics.Executive", KpiAvailability.Available),
        K("EXE.ORDER_COUNT", "Número de pedidos", "Órdenes completed en periodo",
            "COUNT completed", "orders", "count", "Analytics.Executive", KpiAvailability.Available),
        K("EXE.GUESTS", "Comensales", "Número de clientes/covers",
            "—", "—", "count", "Analytics.Executive", KpiAvailability.RequiresModelChange, "No existe cover_count en Order"),
        K("EXE.GROSS_PROFIT", "Utilidad bruta estimada", "Revenue - COGS estimado",
            "sp_product_profitability.gross_profit", "order_items + cost snapshot", "money", "Analytics.Profitability",
            KpiAvailability.AvailableWithLimitations, "COGS = COALESCE(theoretical_unit_cost, average_cost, cost)"),
        K("EXE.GROSS_MARGIN", "Margen bruto %", "gross_profit / revenue * 100",
            "sp_product_profitability.gross_margin_pct", "same", "percent", "Analytics.Profitability",
            KpiAvailability.AvailableWithLimitations, "Estimado"),
        K("EXE.FOOD_COST", "Food Cost %", "Desde food_cost_snapshots",
            "sp_food_cost_summary", "food_cost_snapshots", "percent", "Analytics.Profitability",
            KpiAvailability.AvailableWithLimitations, "Requiere snapshots generados"),
        K("EXE.INV_VALUE", "Valor inventario", "stock * cost",
            "sp_inventory_health.stock_value_estimate", "products", "money", "Analytics.Inventory", KpiAvailability.Available),
        K("EXE.WASTE", "Merma", "waste_events.total_cost",
            "sp_waste_analysis / executive", "waste_events", "money", "Analytics.Inventory", KpiAvailability.Available),
        K("EXE.CASH_VAR", "Diferencia de caja", "SUM(variance) sesiones",
            "sp_cash_summary.total_variance", "cash_sessions", "money", "Analytics.Cash", KpiAvailability.Available),
        K("EXE.PURCHASES", "Compras del período", "SUM(PO.total)",
            "sp_purchase_summary.po_total", "purchase_orders", "money", "Analytics.Purchases", KpiAvailability.Available),
        K("EXE.CANCELLED", "Pedidos cancelados", "COUNT status=Cancelled",
            "sp_sales_summary.cancelled_count", "orders", "count", "Analytics.Operations", KpiAvailability.Available),
        K("EXE.PREP_TIME", "Tiempo prep promedio", "prepared_at - sent_at",
            "sp_kitchen_performance.avg_prep_minutes", "order_items", "minutes", "Analytics.Operations",
            KpiAvailability.AvailableWithLimitations, "Solo ítems con timestamps poblados"),
        K("EXE.TABLE_OCC", "Ocupación mesas", "mesas Occupied / activas (live)",
            "tables.status", "tables", "ratio", "Analytics.Operations", KpiAvailability.Available),
        K("EXE.SALES_HOUR", "Venta por hora", "group by hour(closed_at)",
            "sp_sales_by_hour", "orders", "money", "Analytics.Sales", KpiAvailability.Available),
        K("EXE.SALES_BRANCH", "Venta por sucursal", "sp_sales_by_branch",
            "sp_sales_by_branch", "orders", "money", "Analytics.Executive", KpiAvailability.Available),
        K("EXE.SALES_EMP", "Venta por empleado", "sp_waiter_performance",
            "orders.user_id", "orders", "money", "Analytics.Sales", KpiAvailability.Available),
        K("EXE.PERIOD_COMP", "Comparación período anterior", "sp_period_comparison",
            "sp_period_comparison", "orders", "mixed", "Analytics.Executive", KpiAvailability.Available),

        K("SAL.BY_PRODUCT", "Ventas por producto", "sp_sales_by_product", "order_items", "order_items", "money", "Analytics.Sales", KpiAvailability.Available),
        K("SAL.BY_CATEGORY", "Ventas por categoría", "sp_sales_by_category", "categories", "categories", "money", "Analytics.Sales", KpiAvailability.Available),
        K("SAL.BY_FLOOR", "Ventas por piso", "área via mesa", "tables.area_id", "areas", "money", "Analytics.Sales",
            KpiAvailability.AvailableWithLimitations, "Disponible vía join; no SP dedicado aún en UI"),
        K("SAL.BY_TABLE", "Ventas por mesa", "sp_table_turnover.revenue", "orders.table_id", "tables", "money", "Analytics.Sales", KpiAvailability.Available),
        K("SAL.BY_WAITER", "Ventas por mesero", "sp_waiter_performance", "orders.user_id", "users", "money", "Analytics.Sales", KpiAvailability.Available),
        K("SAL.BY_CASHIER", "Ventas por cajero", "payments.processed_by_user_id", "payments", "payments", "money", "Analytics.Sales",
            KpiAvailability.AvailableWithLimitations, "Agregación vía pagos; no SP dedicado en v1 UI"),
        K("SAL.BY_PAYMETHOD", "Ventas por método de pago", "sp_sales_by_payment", "payments.method", "payments", "money", "Analytics.Sales",
            KpiAvailability.AvailableWithLimitations, "method es string, sin catálogo maestro"),
        K("SAL.NEG_MARGIN", "Productos margen negativo", "margin_estimate < 0", "sp_sales_by_product", "order_items", "money", "Analytics.Profitability", KpiAvailability.Available),

        K("PRF.MENU_ENG", "Menu engineering", "Star/Plowhorse/Puzzle/Dog", "sp_menu_engineering", "order_items", "class", "Analytics.Profitability",
            KpiAvailability.AvailableWithLimitations, "Clasificación por mediana de qty y margen estimado"),
        K("PRF.PLATE_COST", "Costo por plato", "Food Cost module recipes", "recipes", "recipes", "money", "Analytics.Profitability",
            KpiAvailability.AvailableWithLimitations, "Usar módulo Food Cost; no recalcular aquí"),

        K("INV.STOCK", "Stock actual", "products.stock", "products", "products", "qty", "Analytics.Inventory", KpiAvailability.Available),
        K("INV.RESERVED", "Stock reservado", "—", "—", "—", "qty", "Analytics.Inventory", KpiAvailability.NotAvailable, "No existe reserva de stock"),
        K("INV.WAREHOUSE", "Inventario por bodega", "—", "—", "—", "qty", "Analytics.Inventory", KpiAvailability.RequiresModelChange, "Station actúa como ubicación, no warehouse master"),
        K("INV.TURNOVER", "Rotación", "sp_inventory_turnover", "inventory_movements", "movements", "ratio", "Analytics.Inventory", KpiAvailability.Available),
        K("INV.COVERAGE", "Cobertura días", "sp_inventory_coverage", "stock/consumo", "movements", "days", "Analytics.Inventory", KpiAvailability.Available),
        K("INV.NO_MOVE", "Sin movimiento", "consumo=0 en ventana", "sp_inventory_turnover", "movements", "count", "Analytics.Inventory", KpiAvailability.Available),
        K("INV.EXPIRY", "Próximos a vencer", "goods_receipt_lines.expiry_date", "receipts", "receipts", "date", "Analytics.Inventory",
            KpiAvailability.AvailableWithLimitations, "Solo líneas de recepción, no lotes de inventario"),
        K("INV.COUNT_ACC", "Exactitud vs conteo físico", "—", "—", "—", "percent", "Analytics.Inventory",
            KpiAvailability.NotAvailable, "No hay módulo de conteo físico enterprise"),

        K("PUR.PRICE_VAR", "Variación precios proveedor", "sp_supplier_price_variation", "price_history", "price_history", "percent", "Analytics.Purchases", KpiAvailability.Available),
        K("PUR.OTIF", "Cumplimiento proveedor", "supplier_scores.otif_score", "supplier_scores", "scores", "score", "Analytics.Purchases",
            KpiAvailability.AvailableWithLimitations, "Depende de scores calculados"),

        K("CASH.OPEN", "Aperturas", "sessions_opened", "sp_cash_summary", "cash_sessions", "count", "Analytics.Cash", KpiAvailability.Available),
        K("CASH.CLOSE", "Cierres", "sessions_closed", "sp_cash_summary", "cash_sessions", "count", "Analytics.Cash", KpiAvailability.Available),
        K("CASH.VAR_DETAIL", "Diferencias por sesión", "sp_cash_variance", "cash_sessions.variance", "cash_sessions", "money", "Analytics.Cash", KpiAvailability.Available),

        K("OPS.ACTIVE", "Pedidos activos", "status not Completed/Cancelled", "orders", "orders", "count", "Analytics.Operations", KpiAvailability.Available),
        K("OPS.STATION", "Rendimiento estación", "sp_station_performance", "order_items", "stations", "mixed", "Analytics.Operations", KpiAvailability.Available),
        K("OPS.TABLE_TURN", "Rotación mesas", "sp_table_turnover", "orders", "tables", "count", "Analytics.Operations", KpiAvailability.Available),
    ];

    private static KpiDefinition K(
        string code, string name, string desc, string formula, string source, string unit, string perm,
        KpiAvailability avail, string? limitation = null, string? range = null, string? interp = null)
        => new(code, name, desc, formula, source, unit, perm, avail, limitation, range, interp);
}

public static class AnalyticsReportCatalog
{
    public static IReadOnlyList<AnalyticsReportDefinition> All { get; } =
    [
        R("executive-summary", "Resumen Ejecutivo", "KPIs dueño para decidir hoy", "Ejecutivos", "Analytics.Executive", "analytics.sp_executive_summary", true, KpiAvailability.Available),
        R("branch-comparison", "Comparación de Sucursales", "Rendimiento por branch", "Ejecutivos", "Analytics.CrossBranch", "analytics.sp_sales_by_branch", true, KpiAvailability.Available),
        R("period-results", "Resultados del Período", "Ventas + comparación", "Ejecutivos", "Analytics.Executive", "analytics.sp_period_comparison", true, KpiAvailability.Available),
        R("sales-trend", "Ventas por Período", "Tendencia temporal", "Ventas", "Analytics.Sales", "analytics.sp_sales_trend", true, KpiAvailability.Available),
        R("sales-hour", "Ventas por Hora", "Demanda horaria", "Ventas", "Analytics.Sales", "analytics.sp_sales_by_hour", true, KpiAvailability.Available),
        R("sales-product", "Ventas por Producto", "Mix y margen estimado", "Ventas", "Analytics.Sales", "analytics.sp_sales_by_product", true, KpiAvailability.Available),
        R("sales-category", "Ventas por Categoría", "Mix por categoría", "Ventas", "Analytics.Sales", "analytics.sp_sales_by_category", true, KpiAvailability.Available),
        R("sales-waiter", "Ventas por Mesero", "Productividad meseros", "Ventas", "Analytics.Sales", "analytics.sp_waiter_performance", true, KpiAvailability.Available),
        R("payment-methods", "Métodos de Pago", "Composición de cobros", "Ventas", "Analytics.Sales", "analytics.sp_sales_by_payment", true, KpiAvailability.AvailableWithLimitations, "method string"),
        R("profitability-product", "Rentabilidad por Producto", "Margen estimado", "Rentabilidad", "Analytics.Profitability", "analytics.sp_sales_by_product", true, KpiAvailability.AvailableWithLimitations, "COGS estimado"),
        R("food-cost", "Food Cost", "Snapshots FC", "Rentabilidad", "Analytics.Profitability", "analytics.sp_food_cost_summary", true, KpiAvailability.AvailableWithLimitations, "Requiere snapshots"),
        R("menu-engineering", "Menu Engineering", "Star/Plowhorse/Puzzle/Dog", "Rentabilidad", "Analytics.Profitability", "analytics.sp_menu_engineering", true, KpiAvailability.AvailableWithLimitations),
        R("waste", "Impacto de Desperdicio", "Top merma", "Rentabilidad", "Analytics.Inventory", "analytics.sp_waste_analysis", true, KpiAvailability.Available),
        R("inventory-health", "Estado de Inventario", "Críticos y valor", "Inventario", "Analytics.Inventory", "analytics.sp_inventory_health", false, KpiAvailability.Available),
        R("inventory-turnover", "Rotación", "Consumo vs stock", "Inventario", "Analytics.Inventory", "analytics.sp_inventory_turnover", true, KpiAvailability.Available),
        R("inventory-coverage", "Cobertura", "Días de stock", "Inventario", "Analytics.Inventory", "analytics.sp_inventory_coverage", true, KpiAvailability.Available),
        R("purchases-supplier", "Compras por Proveedor", "Gasto proveedores", "Compras", "Analytics.Purchases", "analytics.sp_supplier_performance", true, KpiAvailability.Available),
        R("purchase-summary", "Órdenes de Compra", "PO del periodo", "Compras", "Analytics.Purchases", "analytics.sp_purchase_summary", false, KpiAvailability.Available),
        R("price-variation", "Variación de Precios", "Cambios en price_history", "Compras", "Analytics.Purchases", "analytics.sp_supplier_price_variation", true, KpiAvailability.Available),
        R("cash-summary", "Resumen de Caja", "Sesiones y totales", "Caja", "Analytics.Cash", "analytics.sp_cash_summary", false, KpiAvailability.Available),
        R("cash-variance", "Diferencias de Caja", "Detalle por sesión", "Caja", "Analytics.Cash", "analytics.sp_cash_variance", true, KpiAvailability.Available),
        R("kitchen", "Rendimiento de Cocina", "Tiempos prep", "Operaciones", "Analytics.Operations", "analytics.sp_kitchen_performance", false, KpiAvailability.AvailableWithLimitations),
        R("stations", "Rendimiento por Estación", "Throughput estación", "Operaciones", "Analytics.Operations", "analytics.sp_station_performance", true, KpiAvailability.Available),
        R("waiters", "Rendimiento por Mesero", "Alias ventas mesero", "Operaciones", "Analytics.Operations", "analytics.sp_waiter_performance", true, KpiAvailability.Available),
        R("table-turnover", "Rotación de Mesas", "Uso de mesas", "Operaciones", "Analytics.Operations", "analytics.sp_table_turnover", true, KpiAvailability.Available),
    ];

    private static AnalyticsReportDefinition R(
        string key, string title, string desc, string cat, string perm, string src, bool chart,
        KpiAvailability avail, string? lim = null)
        => new(key, title, desc, cat, perm, src, chart, avail, lim);

    public static AnalyticsReportDefinition? Get(string key) =>
        All.FirstOrDefault(r => r.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
}
