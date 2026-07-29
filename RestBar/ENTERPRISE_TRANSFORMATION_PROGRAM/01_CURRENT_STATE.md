# 01 — CURRENT STATE

**Programa:** RestBar Enterprise Transformation  
**Fecha:** 2026-07-29  
**Fuente:** Código vivo + certificaciones + mapa funcional (sin modificaciones)

---

# 1. Arquitectura actual

| Capa | Stack | Estado |
|------|-------|--------|
| Web | ASP.NET Core 8 MVC + Controllers API | Estable |
| Datos | PostgreSQL + EF Core (`RestBarContext`) | Estable |
| Realtime | SignalR `OrderHub` (`/orderHub`) | Certificado |
| Auth | Cookies + claims (`CompanyId`, `BranchId`, `UserRole`) | Estable |
| Multitenant | Company → Branch → recursos | Certificado 51/51 |
| Jobs | **Ninguno** (no BackgroundService/Hangfire) | Gap |
| Billing SaaS | Middleware suspensión tenant | Parcial |

**Patrón:** Controllers → Services → DbContext. Filtros tenant **manuales** (no global query filters EF).

---

# 2. Mapa de módulos (evidencia código)

## Completos / maduros (≥75%)

| Módulo | % | Artefactos clave |
|--------|---|------------------|
| POS Órdenes | 90 | `OrderController`, `OrderService`, Views/Order |
| KDS / Estaciones | 85–88 | `StationOrders`, `KitchenService`, routing prep steps |
| Pagos (parcial/mixto/refund) | 80 | `PaymentController`, `PaymentService`, PaymentView |
| Productos / Categorías / Stock estación | 85 | Product*, ProductStockAssignment |
| Company / Branch / SuperAdmin | 80 | SuperAdmin + Company/Branch |
| Multitenant aislamiento | 82 | Certificación MT |

## Parciales (35–70%)

| Módulo | % | Qué falta |
|--------|---|-----------|
| Inventario | 55 | PO, proveedor, merma estructurada, caducidad |
| Recetas (BOM) | 40 | API `RecipeController`; **sin UI** |
| Reportes ventas | 70 | APIs reales; **export PDF/Excel stub** |
| Advanced Reports | 60 | Queries reales; Supplier stub; export stub |
| Descuentos / PriceSchedule | 45 | Servicios existen; vistas AdvancedSettings incompletas |
| Turnos (Shift) | 35 | API Start/End/Handoff; **sin UI ni caja** |
| Tips / Comisiones | 30 | Modelos + `AllocateTipsAsync`; sin admin UI |
| Merge/Split mesas | 50 | TableService + TableMergeLink |
| Settings avanzados | 40 | Index/SystemSettings; muchas vistas **faltan** |
| Clientes | 20 | Modelo + `CustomerService` + LoyaltyPoints; **sin controller** |
| Invoice | 15 | `InvoiceService` registrado; **sin controller** |
| Modifiers | 10 | Solo service |

## Ausentes / huérfanos (≤15%)

| Módulo | Evidencia |
|--------|-----------|
| **Proveedores** | JS `supplier-management.js` → `/Supplier/*` **404**; sin Controller/Views |
| **Órdenes de compra** | No dominio PO; `CreatePurchase` = ajuste stock |
| **Caja / arqueo** | No CashRegister; Shift ≠ caja |
| **Precuenta** | No API/UI dedicada |
| **Fiscal completo** | Invoice sin superficie HTTP |
| **Combos** | No entidad/UI |
| **Happy Hour motor** | PriceSchedule parcial; sin UX comercial |
| **Loyalty / CRM UI** | Solo campo LoyaltyPoints |
| **Delivery / Reservas** | No |
| **BI predictivo** | `GrowthForecasts` vacío |
| **IA Copilot** | No |
| **Backup real** | `ExecuteBackupAsync` simula Delay |

---

# 3. Procesos de negocio — cobertura

| Proceso | Estado |
|---------|--------|
| Abrir mesa → tomar pedido → cocina → cobro | **Administra** |
| Routing multi-estación / bar | **Administra** |
| Pagos parciales / dividir cuenta | **Administra** |
| Stock por estación + transferencia | **Parcial** |
| Receta → descuento inventario al vender | **Parcial** (sin UX receta) |
| Cierre de caja / arqueo | **No** |
| Compra a proveedor → recepción → costo | **No** |
| Food cost / prime cost | **No** |
| Precuenta / factura fiscal | **No / stub** |
| Promos / combos / HH | **No / parcial pricing** |
| Labor / nómina | **No** |
| Franquicia (royalty, pack reportes) | **No** (MT sí) |
| Forecast / IA | **No** |

---

# 4. Scorecard de partida (auditoría enterprise)

**Madurez comercial: 41/100** · Veredicto: **Listo para Pilotos**

| Dimensión | Score |
|-----------|-------|
| Valor operativo | 68 |
| Valor financiero | 28 |
| BI / Analytics | 22 |
| Competitividad | 32 |
| SaaS | 25 |
| Cadenas internacionales | 18 |

---

# 5. Activos a **extender** (no reinventar)

1. `InventoryOperationsService` → recepción PO, merma, food cost  
2. `InventoryMovement` + tipos Purchase/Waste → ledger compras  
3. `DiscountPolicy` + `PriceScheduleService` → motor promos/HH  
4. `Recipe` / `RecipeLine` → UI + food cost  
5. `Shift` → extender a **caja** (no crear turno paralelo)  
6. `Invoice` / `InvoiceService` → fiscal + precuenta  
7. `Customer` + LoyaltyPoints → CRM mínimo  
8. `TenantSubscriptionMiddleware` → billing SaaS  
9. `OrderHub` → alertas Executive Command Center  
10. `AdvancedReportsService` → alimentar dashboard/BI (quitar stubs)

---

# 6. Deuda de producto (complejidad sin valor)

| Ítem | Acción recomendada |
|------|-------------------|
| UserController vs UserManagementController | Unificar |
| Payment vs PaymentView menú | Un solo entry point |
| Category vs ProductCategory | Deprecar legacy |
| Reports vs AdvancedReports overlap | Fusionar superficie |
| SupplierAnalysis UI sin dominio | Implementar dominio o ocultar |
| AdvancedSettings sin Views | Completar o colapsar AJAX |
| Seed AllowAnonymous | Solo Development |
| Invoice/Customer/Modifier sin HTTP | Activar o no exponer en DI público |

---

# 7. Conclusión de estado

RestBar es un **POS+KDS multitenant certificado** con cimientos enterprise (recetas, movimientos, shifts, invoices en schema) **incompletos en superficie de producto**.

La transformación correcta **no es agregar 30 módulos nuevos**: es **activar y completar** los building blocks existentes, luego cerrar los **table stakes** (caja, compras, fiscal, ingresos) y recién después BI/IA.
