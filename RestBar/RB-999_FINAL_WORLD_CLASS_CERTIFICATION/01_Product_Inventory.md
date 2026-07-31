# 01 — Product Inventory (Definitive)

**RB-999** · Evidence baseline: FULL_BROWSER cert + RB-010/020/023/025/026 · Commit `bf61cee`  
**Rule:** only capabilities backed by executable code / tests / docs.

Maturity: **Enterprise** | **Profesional** | **Básico** | **Experimental** | **Incompleto** | **Huérfano** | **No funcional**

| Dominio | Capacidad | Madurez | Evidencia |
|---------|-----------|---------|-----------|
| Operación | POS mesa → productos → cocina | Profesional | OrderController, ORD-E2E PASS |
| Operación | Tipos de orden / notas / cantidades | Profesional | Order JS + services |
| Operación | División de cuenta (Person) | Básico/Incompleto | PersonController + JS; tests incompletos |
| Operación | Cambio de mesa | Básico | MoveToTable API |
| Operación | Turnos / handoff | Básico | ShiftController POST-only |
| Cocina | KDS kitchen/bar + SignalR | Profesional | StationOrders, KDS-* PASS |
| Cocina | Routing por estación/stock | Profesional | Station + ProductStockAssignment |
| Caja | Sesión, apertura, arqueo, X/Z, hash chain | Profesional→Enterprise | RB-010, CASH-* PASS |
| Caja | Paid-in/out API | Profesional | CashMovement API |
| Pagos | Parcial / API refund | Profesional/Incompleto | api/Payment; tender UI profundo incompleto |
| Inventario | Stock, movimientos, low stock, kardex vía movements | Profesional | Inventory + INV-* PASS |
| Inventario | Transferencias stock | Básico | StockTransfer |
| Inventario | Bodegas WMS / conteo físico | **No funcional** | No módulo |
| Compras | Proveedores, PO, receive, dashboard | Profesional | RB-020, PO-* PASS (E2E receive profundo incompleto) |
| Compras | Solicitud/aprobación formal UI | Incompleto | Modelos existen; UI parcial |
| Food Cost | Recetas, snapshots, waste, menu eng, simulate | Profesional | RB-023, FC-* PASS |
| Analytics | Centro Ejecutivo, KPIs, exports CSV/XLSX/HTML | Profesional | RB-025, AN-* PASS |
| Analytics | Power BI / warehouse analítico | **No funcional** | Nativo only |
| Reportes | AdvancedReports + exports ClosedXML | Profesional | RPT-* PASS |
| Reportes | Reports clásico ExportPdf/Excel | **No funcional** (stub) | ReportsController TODO |
| Admin | Company, Branch, Area, Table, Station, Product, Category, Users | Profesional | ADM-* PASS |
| Seguridad | Cookie auth, policies, rate limit login, headers CSP | Profesional | RB-026 |
| Seguridad | CSRF JSON APIs | Incompleto | Gap documentado |
| SaaS | Multi-company/branch, suspensión middleware | Básico→Profesional | TenantSubscriptionMiddleware |
| SaaS | Billing/planes/portal cliente | **No funcional** | No UI billing |
| Integraciones | Webhooks, processors tarjeta nativos | **No funcional** | No |
| Configuración | AdvancedSettings (tax, currency, hours) | Básico | Controllers existen |
| Auditoría | Audit UI + audit events cash/PO/FC | Profesional | /Audit |
| Productividad | Meseros, stations, Command Center | Básico→Profesional | Feature flags |
| Inteligencia | Copilot | Experimental/Deshabilitado | EnableCopilot=false |
| Escalabilidad | Docker single-node, health, retry EF | Básico | RB-026; no 5k load |
| Offline POS | — | **No funcional** | Browser online |
| Hardware | Impresoras/cajón nativo | **No funcional**/Básico print HTML | — |
| Reservas / CRM / Combos / Promos | — | **No funcional** | No código |
| Dark Mode / i18n | — | **No funcional** | ES hardcode |

## Resumen comercial del inventario

RestBar es un **POS + back-office integrado** (caja, inventario, compras, food cost, analytics nativo) fuerte para operación de restaurante **conectado**. No es un ecosistema de pagos/hardware/offline de clase Toast/Oracle.
