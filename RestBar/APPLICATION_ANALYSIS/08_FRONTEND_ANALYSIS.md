# 08 — Frontend Analysis

**Sistema:** RestBar  
**Fecha:** 2026-07-04

---

## 1. Paradigma Frontend

RestBar utiliza un paradigma **Server-Rendered MVC enriquecido con JavaScript**, NO es un SPA.

| Aspecto | Implementación |
|---------|----------------|
| Renderizado | Razor Views (server-side) |
| Interactividad | jQuery + fetch/AJAX |
| Estilos | Bootstrap 5 + CSS custom |
| Validación | Mixta (unobtrusive + manual JS) |
| Estado | Variables globales JS (no framework de estado) |
| Real-time | SignalR client |

---

## 2. Inventario de Vistas (65 archivos)

### 2.1 Por Controlador

| Carpeta Views | Archivos | Layout |
|--------------|----------|--------|
| AdvancedReports/ | 10 | _Layout |
| AdvancedSettings/ | 2 | _Layout |
| Area/ | 1 (Index) | _Layout |
| Audit/ | 2 (Index, Details) | _Layout |
| Auth/ | 3 (Login, Profile, AccessDenied) | _LoginLayout |
| Branch/ | 1 | _Layout |
| Category/ | 2 (Index, Edit) | _Layout |
| Company/ | 1 | _Layout |
| Home/ | 2 (Index, Privacy) | _Layout |
| Inventory/ | 1 | _Layout |
| Order/ | 7 (Index + 5 partials + StationOrders) | _OrderLayout / _KitchenLayout |
| Payment/ | 1 (huérfana) | — |
| PaymentView/ | 1 | _Layout |
| Product/ | 1 | _Layout |
| ProductStockAssignment/ | 1 | _Layout |
| Reports/ | 1 | _Layout |
| Seed/ | 1 | _Layout |
| Station/ | 5 (Index, Create, Edit, Delete, Details) | _Layout |
| SuperAdmin/ | 8 | _Layout |
| Table/ | 1 | _Layout |
| User/ | 2 | _Layout |
| UserAssignment/ | 1 | _Layout |
| UserManagement/ | 2 | _Layout |
| Shared/ | 7 (4 layouts + Error + partials) | — |

### 2.2 Partials (Order/Index)

| Partial | Propósito |
|---------|-----------|
| `_TableSelection.cshtml` | Grid de mesas con estados |
| `_Categories.cshtml` | Tabs de categorías |
| `_Products.cshtml` | Grid de productos |
| `_OrderSummary.cshtml` | Resumen de orden actual |
| `_SignalRStatus.cshtml` | Indicador de conexión SignalR |

---

## 3. Layouts

| Layout | Archivo | Características |
|--------|---------|----------------|
| `_Layout` | Views/Shared/_Layout.cshtml | Navbar completo, dropdowns Config/Operaciones, badge company/branch |
| `_OrderLayout` | Views/Shared/_OrderLayout.cshtml | Chrome mínimo, tema oscuro, optimizado para POS |
| `_KitchenLayout` | Views/Shared/_KitchenLayout.cshtml | Fullscreen, sin navegación, optimizado para KDS |
| `_LoginLayout` | Views/Shared/_LoginLayout.cshtml | Centrado, sin navbar, fondo branded |

### ViewComponents

**Ninguno encontrado.** La composición UI se hace via layouts + partials + inline scripts.

---

## 4. Inventario JavaScript (28 módulos)

### 4.1 Módulos Globales (cargados en _Layout)

| Archivo | Propósito |
|---------|-----------|
| `site.js` | Bootstrap dropdown init, navbar UX |
| `responsive-notifications.js` | SweetAlert2 defaults (toast, mobile) |
| `dev-cache-buster.js` | Cache busting en localhost |
| `separate-accounts-simple.js` | Modal split-bill ligero |

### 4.2 Módulos POS (`js/order/` — 17 archivos)

| Archivo | Clase/Función principal | Responsabilidad |
|---------|--------------------------|----------------|
| `utilities.js` | formatCurrency, safeJsonParse | Helpers compartidos |
| `order-management.js` | currentOrder (state) | Estado y tracking de orden |
| `order-ui.js` | renderOrderItems, updateQtyButtons | Renderizado UI de ítems |
| `order-operations.js` | sendToKitchen, cancelOrder | Operaciones de orden |
| `tables.js` | loadTables, selectTable | Selección y estado de mesas |
| `categories.js` | loadCategories, addToOrder | Catálogo y agregar productos |
| `signalr.js` | initializeSignalR, joinGroups | Conexión y eventos SignalR |
| `payments.js` | openPaymentModal, processPayment | Modal y lógica de pagos |
| `discounts.js` | openDiscountModal | Aplicar descuentos |
| `dynamic-status.js` | DynamicStatusManager | Colores/animaciones de estado |
| `stock-updates.js` | initializeStockSignalR | SignalR secundario para stock |
| `separate-accounts.js` | (completo, no cargado) | Split bill completo |
| `debug-signalr.js` | debug helpers | Diagnóstico dev |
| `test-*.js` (3) | test helpers | Solo desarrollo |

### 4.3 Otros Módulos

| Archivo | Cargado por | Estado |
|---------|------------|--------|
| `payment-management.js` | PaymentView/Index | ✅ Activo |
| `product-stock-assignment.js` | ProductStockAssignment/Index | ✅ Activo |
| `advanced-reports.js` | AdvancedReports/Index | ✅ Activo |
| `inventory-management.js` | Inventory/Index | ✅ Activo |
| `inventory-movements.js` | — | ❌ No conectado |
| `inventory-analysis.js` | AdvancedReports/InventoryAnalysis | ✅ Activo |
| `accounting.js` | — | ❌ Huérfano |
| `supplier-management.js` | — | ❌ Huérfano |

### 4.4 JS Faltantes (referenciados pero no existen)

| Archivo esperado | Vista que lo referencia |
|-----------------|------------------------|
| `customer-analysis.js` | AdvancedReports/CustomerAnalysis |
| `sales-analysis.js` | AdvancedReports/SalesAnalysis |
| `operational-analysis.js` | AdvancedReports/OperationalAnalysis |

---

## 5. CSS Custom (9 archivos)

| Archivo | Uso |
|---------|-----|
| `site.css` | Estilos globales |
| `order.css` | Pantalla POS |
| `inventory.css` | Módulo inventario |
| `payment-management.css` | Dashboard de pagos |
| `accounting.css` | Sin uso (huérfano) |
| `separate-accounts.css` | Split bill modal |
| `signalr-notifications.css` | Indicador conexión SignalR |
| `responsive-notifications.css` | SweetAlert2 responsive |
| `modern-kitchen.css` | KDS styling |

---

## 6. Librerías Externas

### En wwwroot/lib (vendor)

| Librería | Versión aprox. |
|----------|---------------|
| Bootstrap | 5.x |
| jQuery | 3.x |
| jQuery Validation | 1.x |
| jQuery Validation Unobtrusive | 3.x |

### Via CDN (no en wwwroot)

| Librería | CDN | Usado en |
|----------|-----|----------|
| DataTables | cdn.datatables.net | Tablas admin |
| SweetAlert2 | cdn.jsdelivr.net | Alertas/modales |
| Font Awesome | cdnjs.cloudflare.com | Iconos |
| Google Fonts (Inter) | fonts.googleapis.com | Tipografía |
| SignalR Client | cdnjs/unpkg | order/signalr.js, StationOrders |

---

## 7. Consumo de APIs (Frontend → Backend)

### 7.1 Patrón General

La mayoría de pantallas admin usan **SPA-lite**:
1. Vista Razor renderiza HTML shell + modal templates
2. JavaScript carga datos via `fetch()` a endpoints JSON del controller
3. DOM se actualiza dinámicamente
4. SweetAlert2 para confirmaciones y errores

### 7.2 POS (Order/Index) — Flujo de APIs

```
tables.js        → GET /Order/GetActiveTables
categories.js    → GET /Order/GetActiveCategories
                 → GET /Order/GetProductsByCategory/{id}
                 → GET /Order/CheckItemStockAvailability
order-operations → POST /Order/AddItems
                 → POST /Order/SendToKitchen
                 → POST /Order/Cancel
payments.js      → POST /api/Payment/partial
                 → GET /api/Payment/order/{id}/summary
                 → DELETE /api/Payment/{id}
signalr.js       → WebSocket /orderHub
```

### 7.3 KDS (StationOrders) — Flujo

```
Inline JS        → GET orders via page model
signalr.js       → WebSocket /orderHub (station group)
                 → POST /Order/MarkItemReady
onreconnected    → GET /api/kitchen/current (snapshot)
```

---

## 8. SignalR en Frontend

### Conexiones

| Pantalla | Script | Hub URL | Groups |
|----------|--------|---------|--------|
| Order/Index | signalr.js + stock-updates.js | /orderHub | order_*, table_*, orders, stock_updates |
| StationOrders | signalr.js + inline | /orderHub | station_{type}, kitchen |

### Eventos manejados en cliente

| Evento | Handler | Efecto UI |
|--------|---------|-----------|
| OrderStatusChanged | signalr.js | Actualizar badge estado |
| OrderItemStatusChanged | signalr.js | Actualizar ítem en KDS/POS |
| OrderItemUpdated | signalr.js | Refrescar resumen |
| OrderCancelled | signalr.js | Limpiar UI, liberar mesa |
| OrderCompleted | signalr.js | Cerrar orden, liberar mesa |
| TableStatusChanged | tables.js | Actualizar color mesa |
| NewOrder | signalr.js | Notificación nueva orden |
| KitchenUpdate | signalr.js | Refrescar KDS |
| PaymentProcessed | payments.js | Actualizar estado pago |

### Reconexión

- `signalr.js`: handlers `onreconnecting` / `onreconnected` con re-join de grupos
- `StationOrders`: fetch `/api/kitchen/current` post-reconexión
- `stock-updates.js`: conexión separada con `withAutomaticReconnect()`

---

## 9. Validación en Cliente

| Pantalla | Mecanismo |
|----------|-----------|
| Station Create/Edit | jQuery Unobtrusive Validation |
| Category Edit | jQuery Unobtrusive Validation |
| SuperAdmin forms | asp-validation-for (sin partial de scripts ⚠) |
| UserManagement Create | asp-validation-for + setCustomValidity password |
| User/Index | HTML5 required + validación manual JS |
| Company, Table, Station Index | SweetAlert si campos vacíos |
| Inventory | validateMovement() custom |
| Payments | validateSplitPayments() |
| POS (Order) | Validación mínima (stock check server-side) |

**Patrón dominante:** Validación manual con SweetAlert, no framework de validación unificado.

---

## 10. Experiencia de Usuario (UX)

### 10.1 Fortalezas

| Aspecto | Detalle |
|---------|---------|
| POS optimizado | Layout dedicado, flujo mesa→producto→orden |
| KDS fullscreen | Pantalla limpia para cocina |
| Real-time | SignalR mantiene sincronización entre POS y KDS |
| Dashboard por rol | Cards filtradas evitan acceso a módulos no autorizados |
| SweetAlert2 | Confirmaciones elegantes, responsive |
| Cultura es-PA | Fechas y moneda localizadas |

### 10.2 Debilidades

| Aspecto | Detalle |
|---------|---------|
| Navbar roto | Link "Cocina" apunta a ruta inexistente |
| Sin loading states consistentes | Algunos fetch sin spinner |
| JS faltantes | 3 reportes avanzados sin su JS |
| Dual user management | User/UserManagement vs UserManagement/Index confuso |
| Sin offline support | Requiere conexión constante |
| Sin PWA | No instalable como app |
| Inline scripts | Muchas vistas con `<script>` inline (mantenibilidad) |
| Sin componentes reutilizables | No hay ViewComponents ni partials compartidos para modales CRUD |

---

## 11. Reutilización de Componentes

| Componente | Reutilización |
|-----------|--------------|
| `_Layout` | Todas las pantallas admin |
| `_ValidationScriptsPartial` | Solo Station y Category |
| SweetAlert2 wrapper | responsive-notifications.js (global) |
| formatCurrency | utilities.js (importado en order modules) |
| Modales CRUD | Duplicados inline en cada Index.cshtml |
| DataTables init | Repetido en múltiples vistas |

**No hay:** Design system, component library, shared modal component.

---

## 12. Responsividad

| Pantalla | Responsive |
|----------|-----------|
| Dashboard | Bootstrap grid (cards) |
| POS | Optimizado para tablet/desktop |
| KDS | Desktop/monitor cocina |
| Admin CRUD | DataTables responsive |
| Login | Centrado, mobile-friendly |
| SweetAlert2 | responsive-notifications.js adapta para mobile |

---

*Análisis de frontend completo. Sin modificaciones al sistema.*
