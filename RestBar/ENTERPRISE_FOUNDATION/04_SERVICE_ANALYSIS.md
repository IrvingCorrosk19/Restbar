# 04 — SERVICE ANALYSIS

---

# 1. Inventario de services

| Service | Cohesión | Tamaño | Acción foundation |
|---------|----------|--------|-------------------|
| OrderService | Baja | Crítico | Plan extracción; facade estable |
| OrderItemService | Media | OK | Mantener |
| KitchenService | Media | Overlap Order | Unificar queries KDS a largo plazo |
| PaymentService | Alta | OK | Extender tips/caja hooks después |
| SplitPaymentService | Alta | OK | Mantener |
| InventoryOperationsService | Alta | Pequeño | **Extender** PO receive / waste cost |
| ProductService | Media | Grande | OK |
| AdvancedReportsService | Baja | Grande | Separar exporters; quitar stubs visibles |
| SalesReportService | Alta | OK | Reusar en CC |
| InvoiceService | N/A | Huérfano | Wire fiscal después; no borrar |
| CustomerService | N/A | Huérfano HTTP | Wire CRM |
| ModifierService | N/A | Huérfano | Wire o documentar dead |
| DiscountPolicyService | Alta | OK | Base promo |
| PriceScheduleService | Media | Misnamed | Renombrar conceptualmente a PricingEngine |
| NotificationService | Media | Sin UI | Base alertas CC |
| BackupSettingsService | Stub execute | | Marcar feature flag |
| Auth / User / Company / Branch | OK | | Mantener |
| ProductCategoryService | Legacy | | Deprecar |

**Controllers sin service:** Shift, Recipe, InventoryMovement, StockTransfer → usan DbContext directo.  
**Acción:** Introducir services delgados cuando se construya Cash/PO (no ahora).

---

# 2. Controllers demasiado grandes

| Controller | Acción |
|------------|--------|
| OrderController | Dividir partial classes o Areas: OrderPos / OrderKitchen / OrderApi |
| PaymentViewController | Extraer query service |
| SeedController | Solo Development; mover seeders a Infrastructure |
| AdvancedReportsController | OK si service se parte |

---

# 3. Unificar vs Separar

| Unificar | Separar |
|----------|---------|
| Menú Payment → PaymentView | Cash domain ≠ Payment API |
| User admin UIs (plan) | Kitchen query ≠ Order mutate |
| Reports entry points | BI read models ≠ OLTP services |
| Category única | Pricing ≠ Inventory |

---

# 4. Contratos públicos a congelar

No cambiar firmas HTTP certificadas:

- Order send-to-kitchen / pay / cancel / split  
- Kitchen station orders  
- Multitenant login isolation  

Nuevos módulos = **nuevas rutas** `/api/cash`, `/api/purchasing`, etc.
