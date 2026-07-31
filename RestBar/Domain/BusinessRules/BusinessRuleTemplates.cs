namespace RestBar.Domain.BusinessRules;

/// <summary>System templates from RB-029 discovery (migratable thresholds).</summary>
public static class BusinessRuleTemplates
{
    public sealed record TemplateDef(string Code, string Name, string Category, string Description, string FlowJson);

    public static IReadOnlyList<TemplateDef> All { get; } =
    [
        new("STOCK_CRITICAL", "Stock crítico", "Inventory",
            "Si lowStockCount > 0 → alerta + recomendación",
            """{"logic":"AND","conditions":[{"field":"inventory.lowStockCount","op":"gt","value":0}],"actions":[{"type":"CreateAlert","params":{"severity":"Medium","code":"LOW_STOCK","message":"Hay productos en stock crítico"}},{"type":"CreateRecommendation","params":{"code":"INV.LOW_STOCK","action":"Priorizar OC para SKUs críticos"}}]}"""),

        new("OVERSTOCK", "Sobreinventario", "Inventory",
            "Si cobertura máxima > 45 días → alerta",
            """{"logic":"AND","conditions":[{"field":"inventory.maxCoverageDays","op":"gt","value":45}],"actions":[{"type":"CreateAlert","params":{"severity":"Low","code":"OVERSTOCK","message":"Cobertura elevada — posible capital inmovilizado"}}]}"""),

        new("REORDER_SUGGEST", "Compra automática sugerida", "Procurement",
            "Si needsReorder → recomendación + tarea",
            """{"logic":"AND","conditions":[{"field":"inventory.needsReorder","op":"eq","value":true}],"actions":[{"type":"CreateRecommendation","params":{"code":"INV.REORDER","action":"Generar solicitud de compra sugerida"}},{"type":"CreateTask","params":{"ownerRole":"inventarista","title":"Revisar sugerencia de reposición"}}]}"""),

        new("CASH_VARIANCE", "Diferencia de caja", "Cash",
            "Si |variance| > 50 → investigación (no fraude automático)",
            """{"logic":"AND","conditions":[{"field":"cash.varianceAbs","op":"gt","value":50}],"actions":[{"type":"CreateAlert","params":{"severity":"High","code":"CASH_VARIANCE","message":"Diferencia de caja supera umbral. Revisar arqueo — no asumir fraude."}},{"type":"CreateRecommendation","params":{"code":"CASH.VARIANCE","action":"Abrir investigación de caja"}}]}"""),

        new("FOOD_COST_HIGH", "Food Cost elevado", "FoodCost",
            "Si FC% >= 35 → alerta + recomendación",
            """{"logic":"AND","conditions":[{"field":"foodcost.actualPct","op":"gte","value":35}],"actions":[{"type":"CreateAlert","params":{"severity":"High","code":"FC_HIGH","message":"Food Cost por encima de 35%"}},{"type":"CreateRecommendation","params":{"code":"FC.HIGH","action":"Revisar waste, recetas y costos"}}]}"""),

        new("SALES_BELOW_FORECAST", "Ventas bajo forecast", "Sales",
            "Si vs forecast < -15% → alerta gerente",
            """{"logic":"AND","conditions":[{"field":"sales.vsForecastPct","op":"lt","value":-15}],"actions":[{"type":"CreateAlert","params":{"severity":"High","code":"SALES_BELOW_FCST","message":"Ventas por debajo del forecast (>15%)"}},{"type":"CreateNotification","params":{"role":"manager","message":"Ventas bajo forecast — revisar mix y tráfico"}}]}"""),

        new("KITCHEN_DELAY", "Tiempo excesivo en cocina", "Kitchen",
            "Si delayedOrders > 0 → alerta",
            """{"logic":"AND","conditions":[{"field":"kitchen.delayedOrders","op":"gt","value":0}],"actions":[{"type":"CreateAlert","params":{"severity":"Medium","code":"KDS_DELAY","message":"Hay pedidos retrasados en cocina"}}]}"""),

        new("SUPPLIER_FAIL", "Proveedor incumplido", "Procurement",
            "Si overdue PO > 0 → alerta",
            """{"logic":"AND","conditions":[{"field":"procurement.overduePoCount","op":"gt","value":0}],"actions":[{"type":"CreateAlert","params":{"severity":"High","code":"PO_OVERDUE","message":"Órdenes de compra atrasadas"}},{"type":"CreateRecommendation","params":{"code":"PUR.OVERDUE","action":"Contactar proveedor / reprogramar"}}]}"""),

        new("NO_SALES_30D", "Producto sin ventas", "Sales",
            "Si deadSkuCount > 0 → sugerir promo/retiro",
            """{"logic":"AND","conditions":[{"field":"inventory.deadSkuCount","op":"gt","value":0}],"actions":[{"type":"CreateRecommendation","params":{"code":"INV.DEAD","action":"Revisar promoción o retiro de SKUs sin movimiento"}}]}"""),

        new("CUSTOMER_VIP", "Cliente VIP", "Customers",
            "Requiere facts RFM; si vipCount > 0 notifica",
            """{"logic":"AND","conditions":[{"field":"customers.vipCount","op":"gt","value":0}],"actions":[{"type":"CreateNotification","params":{"role":"manager","message":"Actividad de clientes VIP"}}]}"""),

        new("CUSTOMER_INACTIVE", "Cliente inactivo", "Customers",
            "Requiere RFM; plantilla lista",
            """{"logic":"AND","conditions":[{"field":"customers.inactiveCount","op":"gt","value":0}],"actions":[{"type":"CreateRecommendation","params":{"code":"CRM.INACTIVE","action":"Recuperación manual de inactivos"}}]}"""),

        new("OPS_RISK", "Riesgo operativo", "Operations",
            "openOrders > 20 AND delayed > 0",
            """{"logic":"AND","conditions":[{"field":"ops.openOrders","op":"gt","value":20},{"field":"kitchen.delayedOrders","op":"gt","value":0}],"actions":[{"type":"CreateAlert","params":{"severity":"High","code":"OPS_RISK","message":"Carga operativa con retrasos"}},{"type":"WriteAudit","params":{"event":"ops_risk_triggered"}}]}"""),

        new("SALES_DROP", "Caída de ventas vs ayer", "Sales",
            "Migración de BiInsight SalesDrop ≥ 20%",
            """{"logic":"AND","conditions":[{"field":"sales.dropPercent","op":"gte","value":20}],"actions":[{"type":"CreateAlert","params":{"severity":"High","code":"SALES_DROP","message":"Ventas bajaron vs ayer ≥20%"}},{"type":"CreateRecommendation","params":{"code":"SAL.DROP","action":"Revisar top productos, promos y ocupación"}}]}"""),
    ];
}
