# 15 — PROCUREMENT COMMAND CENTER

---

# Preguntas (<5 segundos)

| Pregunta | Widget / fuente |
|----------|-----------------|
| ¿Qué comprar hoy? | Products donde Stock ≤ MinStock |
| ¿A quién? | Recommended supplier ranking |
| ¿Cuánto? | MinStock − Stock (+ pack size) |
| ¿Quién falla? | Score < 50 / OTIF bajo |
| ¿Quién sube precios? | PriceHistory Δ% 30d |
| ¿Qué aumentó costo? | Top Product.Cost Δ |
| ¿Atrasadas? | PO Sent past expected_delivery |
| ¿Incompletas? | PartiallyReceived |
| ¿Devolver? | Receipt lines Damaged/Rejected abiertas |
| ¿Negociar? | Price ↑ AND volume alto |

---

# Layout

1. KPI strip: Spend hoy · POs abiertos · Recepciones pendientes · Ahorro vs LastPrice  
2. Alertas  
3. Reorder suggestions (top 10)  
4. Supplier críticos  
5. Órdenes en tránsito  

AsNoTracking + proyección anónima + cache memoria 30s por BranchId.

---

# SignalR

Groups: `procurement_{branchId}`, `procurement_company_{companyId}`  
Events: PoStatusChanged, ReceiptCompleted, CostChanged, SupplierScoreChanged
