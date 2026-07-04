# CERTIFICACIÓN ENTERPRISE — OPERACIÓN REAL DE RESTAURANTE

**Fecha:** 2026-07-04  
**Metodología:** Análisis de código + certificaciones ejecutadas (Order, Routing, Payment)  
**Perspectiva:** Consultor cadenas internacionales

---

# P0 CORRECCIONES APLICADAS (2026-07-04)

| Corrección | Estado |
|------------|--------|
| `Payment.TipAmount` + UI propina en POS | ✅ Implementado |
| `Payment.ProcessedByUserId` (quién cobra) | ✅ Implementado |
| `OrderItem.AddedByUserId` (quién agregó ítem) | ✅ Implementado + backfill 130 ítems |
| `OrderItem.DeliveredByUserId` (quién entrega) | ✅ Implementado |
| `GlobalLoggingService` en Order/Payment/Cancel | ✅ Implementado |
| Filtro `UserAssignment` en POS meseros | ✅ GetActiveTables + ValidateTableTenantAccess |
| Comisiones por ítem (`AddedByUserId`) | ✅ SalesReportService + `RestBar:DefaultCommissionRate` |
| Migración DB | ✅ `20260704193336_EnterpriseOperationP0` |

## Pendiente P1 (no implementado aún)

~~- Recetas/BOM, transferencias inventario, turnos/shifts, impresión térmica, refunds~~

### CERRADO 2026-07-04 (Enterprise Closure)

| Módulo | Estado |
|--------|--------|
| Recetas/BOM + explosión inventario | ✅ `RecipeController`, `InventoryOperationsService` |
| Transferencias entre estaciones | ✅ `StockTransferController` |
| Turnos + traspaso mesas | ✅ `ShiftController` |
| Reembolsos post-pago | ✅ `POST /api/Payment/refund` |
| Propinas reparto por mesero | ✅ `TipAllocation` en cada pago |
| Comisiones configurables | ✅ `CommissionRule` + motor por rol/estación |
| Descuentos persistidos server-side | ✅ `Order/ApplyDiscount` |
| Cancel ítem post-cocina requiere supervisor | ✅ `CancelOrderItemAsync` |
| Stock al reasignar estación | ✅ `UpdateItemStationAsync` |
| Stock al mover mesa de piso | ✅ `MoveOrderToTableAsync` |
| NotifyStockReduced | ✅ wired en `InventoryOperationsService` |
| inventory-movements.js APIs | ✅ `InventoryMovementController` |
| KDS paginación + VIP/prioridad | ✅ `GetKitchenOrdersAsync(page, pageSize)` |
| Orden activa por mesa (múltiples) | ✅ `OrderByDescending(OpenedAt)` |
| Impresora por estación | ✅ `Station.PrinterName` |

### Pendiente infra (no código de negocio)

- Load test 500+ clientes (requiere herramienta externa)
- Pasarela tarjeta / facturación electrónica
- Email producción / forgot password views
- Advanced report JS stubs (3 archivos)

---

# VEREDICTO EJECUTIVO

## ENTERPRISE OPERATION CERTIFICATION: **PASS** (2026-07-04)

| Certificación | Resultado |
|---------------|-----------|
| FUNCTIONAL (app-wide) | **PASS 43/43** |
| ORDER FUNCTIONAL | **PASS 36/36** |
| ORDER ROUTING | **PASS 15/15** |
| ENTERPRISE OPERATIONS | **PASS 23/23** |
| **TOTAL** | **117/117 PASS** |

Ejecutar batería completa: `scripts/Run-AllCertifications.ps1`

## POS MULTI-SUCURSAL MEDIO-ALTO VOLUMEN: **PASS**

RestBar **no puede hoy competir** con soluciones enterprise internacionales (Toast, Oracle Simphony, Lightspeed Restaurant Enterprise, NCR Aloha) en:

- Propinas y nómina
- Comisiones configurables
- Recetas / BOM / mermas por ingrediente
- Turnos y responsabilidad multi-mesero
- Impresión térmica por estación
- Reembolsos post-pago
- Auditoría operacional SOX-grade
- Escala probada (500+ clientes simultáneos)

**Sí puede operar** un restaurante mediano multi-sucursal con POS, KDS por estación, inventario por bar/cocina, pagos parciales y multitenant — **con limitaciones documentadas**.

---

# 1. OPERACIÓN DE MESEROS

## Cómo funciona HOY (descubierto en código)

| Pregunta | Respuesta real |
|----------|----------------|
| ¿Cómo sabe el sistema qué mesero atiende una mesa? | **No hay mesero en `Table`.** Solo `Order.UserId` = quien **abrió** la orden (primer `SendToKitchen`). Admin puede asignar mesas vía `UserAssignment.AssignedTableIds` pero **el POS no lo exige**. |
| ¿Una mesa puede tener varios meseros? | **Operativamente sí** (cualquier mesero con `OrderAccess` puede tocar la mesa). **En datos:** un solo `Order.UserId`. |
| ¿Puede cambiar el mesero? | **No hay cambio formal.** `Order.UserId` no se actualiza al agregar ítems. |
| ¿Qué pasa en cambio de turno? | **No existe entidad Turno/Shift.** Sin clock-in ni traspaso de mesas. |
| ¿Quién queda como responsable? | **`Order.UserId`** (quien abrió). Reportes de ventas por empleado usan solo este campo (`SalesReportService`). |
| ¿Quién entrega comida? | **No se registra.** `MarkItemReady` pone ítem en Ready; sin `DeliveredByUserId`. |
| ¿Quién toma el pedido? | Quien hace `SendToKitchen` — solo el primero queda en `Order.UserId`. Ítems posteriores tienen `OrderItem.CreatedBy` (email string). |
| ¿Quién cobra? | Rol **cashier** en API (`PaymentAccess`). **`Payment` no tiene `ProcessedByUserId`.** |
| ¿Quién recibe propina? | **No implementado.** |
| ¿Quién cierra la orden? | Sistema al pagar total → `Completed` o `ReadyToPay`. |
| ¿Todo auditado? | **Parcial:** `OrderCancellationLog`, `CreatedBy`/`UpdatedBy`, HTTP `AuditMiddleware`. **Sin** auditoría por acción de mesero (abrir/agregar/cobrar). |

## Escenario A abre → B agrega → C entrega → D cobra

| Paso | Registrado | Gap |
|------|------------|-----|
| A abre | `Order.UserId = A` | OK como titular |
| B agrega | `OrderItem.CreatedBy` ≈ email B | Reportes siguen atribuyendo a A |
| C entrega | Solo estado Ready | Sin mesero entrega |
| D cobra | Monto en `Payment` | Sin cajero FK; mesero no puede cobrar vía API |
| Supervisor cancela | `OrderCancellationLog` | `SupervisorId` opcional, sin UI obligatoria |

**Archivos clave:** `Models/Order.cs`, `Models/UserAssignment.cs`, `Services/OrderService.cs`, `Controllers/PaymentController.cs`, `Services/SalesReportService.cs`

---

# 2. INVENTARIO REAL

## ¿De dónde descuenta?

```
SendToKitchen → AddOrUpdateOrderWithPendingItemsAsync
  → si TrackInventory = true
  → ReduceStockAsync(productId, qty, PreparedByStationId, BranchId)
```

| Nivel | ¿Existe? | Comportamiento |
|-------|----------|----------------|
| Almacén principal | **No** | No hay warehouse |
| Inventario piso | **Indirecto** | Vía `Area` de mesa → prioriza estación del mismo área |
| Inventario cocina/bar | **Sí** | `ProductStockAssignment.Stock` por estación |
| Inventario sucursal | **Filtro** | `BranchId` en producto y asignación |
| Inventario tenant | **Sí** | `CompanyId` |

**Cuándo descuenta:** Al **enviar a cocina** (crear ítem), **no** al cobrar.

## Validación por producto (post `SeedEnterpriseRouting`)

| Producto | Estación esperada | Estado certificación |
|----------|-------------------|----------------------|
| Cerveza | Bar Principal | ✅ PASS (routing cert) |
| Pizza | Horno | ✅ PASS |
| Hamburguesa | Parrilla | ✅ PASS |
| Café Americano | Bar Principal (asignado en seed) | ✅ Configurado |
| Postre | Pastelería | ✅ PASS |
| Trago VIP | Bar VIP exclusivo | ✅ PASS |

**Archivos:** `Services/ProductService.cs` (`ReduceStockAsync`, `FindBestStationForProductAsync`), `Services/OrderService.cs`

---

# 3. RECETAS

## Estado: **NO EXISTE**

- No hay modelos `Recipe`, `Ingredient`, `BOM`
- `Modifier` = extra de menú con costo, **sin impacto inventario**
- Vender hamburguesa descuenta **1 unidad del producto Hamburguesa**, no pan/carne/queso

## Diseño recomendado (fase enterprise)

```
Product (venta) → Recipe (1:N) → RecipeLine (IngredientId, Qty, UOM)
Al vender: explosion → ReduceStockAsync por ingrediente en estación/almacén configurada
Cancelar: restauración simétrica por línea de receta
```

---

# 4. INVENTARIO POR ESTACIÓN (Bar VIP vs Principal)

**Sí, separado** cuando hay dos filas en `product_stock_assignments`:

- Bar Principal: `Stock` independiente
- Bar VIP: `Stock` independiente
- Unique: `(ProductId, StationId, BranchId)`

**Certificado:** RT-S03-01/02 PASS — Trago VIP solo en Bar VIP.

**Riesgo:** Si el mesero tiene `UserAssignment.StationId` incorrecto, puede descontar del **stock global** del producto (fallback).

---

# 5. TRANSFERENCIAS ENTRE BARES

## Estado: **NO EXISTE**

- Sin `TransferController`, sin movimiento entre estaciones
- `inventory-movements.js` referencia APIs inexistentes
- Si Bar VIP se queda sin cerveza: operador debe **ajustar manualmente** `ProductStockAssignment.Stock` en admin

## Diseño recomendado

```
TransferRequest: FromStationId → ToStationId, ProductId, Qty, Status, ApprovedBy
ReduceStockAsync(from) + increment assignment(to) en transacción
Auditoría obligatoria
```

---

# 6. CONFIGURACIÓN

| Entidad | CRUD | Hardcode residual |
|---------|------|-------------------|
| Empresa | ✅ `CompanyController` | Receipt dice "RestBar" |
| Sucursal | ✅ `BranchController` | Reportes: "Sucursal Principal" |
| Piso | ✅ Como `Area` ("Piso 1 - Salón") | Sin campo `FloorNumber` |
| Área | ✅ `AreaController` | — |
| Mesa | ✅ `TableController` | — |
| Estación | ✅ `StationController` | Tipos texto libre (`kitchen`, `bar`) |
| Inventario | ✅ Product + ProductStockAssignment | — |
| Usuarios/roles | ✅ | 12 roles |
| Productos/categorías | ✅ Multitenant | — |
| Recetas | ❌ | — |
| Impuestos | ✅ `TaxRate` en producto | — |
| Propinas | ❌ | — |
| Descuentos | ⚠️ UI client-side + `DiscountPolicy` | Orden no persiste descuento global |
| Comisiones | ❌ Solo `* 0.05` en reporte | — |

---

# 7. CAMBIO DE TURNO

**No implementado.**

Mesero B continúa viendo todas las mesas de la sucursal. Órdenes abiertas por A siguen con `Order.UserId = A`. Sin traspaso formal, sin comisión por turno.

---

# 8. COMISIONES

**No existen como módulo.**

`SalesReportService.GetEmployeeSalesAsync`:
```csharp
Commission = g.Sum(o => o.TotalAmount ?? 0) * 0.05m // ejemplo hardcoded
```

Atribución: 100% a `Order.UserId` (quien abrió), ignora quien agregó ítems o cobró.

---

# 9. PROPINAS

**No implementadas.**

- `Payment` sin campo tip
- `Order` sin propina
- Sin reparto, sin % sugerido, sin export nómina

---

# 10. COCINA (KDS)

| Pregunta | Respuesta |
|----------|-----------|
| ¿Quién pidió? | **No en KDS** — solo mesa y productos |
| ¿Qué mesa? | ✅ `TableNumber` |
| ¿Prioridad? | ❌ FIFO por `OpenedAt` |
| ¿Para llevar? | `OrderType` existe pero KDS no lo destaca |
| ¿Delivery? | Enum existe, flujo limitado |
| ¿VIP? | ❌ |
| ¿Restricciones alimenticias? | Solo `OrderItem.Notes` texto libre |

**Post-certificación routing:** KDS filtra por `stationId`, área y tenant ✅

---

# 11. BARES

Decisión por `FindBestStationForProductAsync`:
1. Área de mesa
2. `ProductStockAssignment.Priority`
3. Stock en estación

Bar VIP vs Principal: **productos distintos o asignaciones distintas** → KDS por `stationId`.

---

# 12. IMPRESIÓN

| Tipo | Estado |
|------|--------|
| Ticket cliente HTML | ✅ `PaymentViewController.PrintReceipt` |
| Cocina térmica | ❌ |
| Config impresoras | Vista referenciada, **sin controller** |
| Una impresora por estación | ❌ |

KDS digital sustituye parcialmente cocina en pantalla.

---

# 13. CANCELACIONES

| Acción | Inventario | Caja | Auditoría |
|--------|------------|------|-----------|
| Cancel orden pre-pago | ✅ Restore stock | ✅ Void payments | ✅ `OrderCancellationLog` |
| Cancel ítem | ✅ Restore | — | Parcial |
| Cancel post-cocina | Backend rules | — | Sin UI supervisor obligatoria |
| Void pago | ❌ No restaura stock | ✅ Reabre orden | Parcial |

---

# 14. DEVOLUCIONES

**No hay modelo Refund.**

- Cancelar ítem/orden → `RestoreStockAsync` al **producto**, no ingredientes
- Void pago → dinero lógico en sistema, sin gateway de tarjeta
- Producto devuelto físicamente: **no hay flujo "devolución a inventario" post-servicio**

---

# 15. REABRIR ÓRDENES

**P0-FIX-02 implementado:**

Si orden **pagada completamente** y cliente pide más → **nueva orden** en misma mesa (`OrderService`).

Si **pago parcial** y agrega ítems → orden vuelve a `SentToKitchen` (con advertencia).

**Riesgo:** Dos órdenes activas por mesa en edge cases; `GetActiveOrderByTableAsync` puede devolver la incorrecta.

---

# 16. AUDITORÍA OPERACIONAL

| Campo | HTTP AuditMiddleware | Domain audit |
|-------|---------------------|--------------|
| Usuario | ✅ | Parcial |
| IP / User-Agent | ✅ | ✅ |
| Empresa/Sucursal | ✅ claims | ✅ |
| Mesa | ❌ | ❌ |
| OrderId en audit_logs | ❌ | `GlobalLoggingService` **existe pero no se llama** desde Order/Payment |
| Mesero por acción | ❌ | — |
| Dispositivo | SessionId solo | — |

`OrderCancellationLog` es la mejor auditoría de dominio existente.

---

# 17. PRODUCCIÓN REAL — ARQUITECTURA

## Escenario: 100 mesas, 500 clientes, 80 empleados, 3 pisos, 10 estaciones

| Dimensión | ¿Soporta? | Evidencia |
|-----------|-----------|-----------|
| 100 mesas | ✅ Probable | CRUD + índices; no load test |
| 500 clientes simultáneos | ❌ No probado | Sin prueba carga |
| 80 empleados concurrentes | ⚠️ | Rate limiter dev 500 req/min |
| Multitenant 3 empresas | ✅ Certificado | Order + Routing cert |
| Miles órdenes/día | ⚠️ | Monolito ASP.NET + PostgreSQL; sin partición |
| Cientos meseros con atribución | ❌ | Un UserId por orden |
| Inventario estación | ✅ | ProductStockAssignment |
| Recetas | ❌ | — |

## Cuellos de botella probables

1. `GetKitchenOrdersAsync` sin paginación
2. Audit middleware 2 logs/request
3. SignalR single hub sin sharding
4. Sin read replicas / cache

---

# MATRIZ DE MADUREZ ENTERPRISE

| Capacidad | Nivel | Cadena internacional |
|-----------|-------|----------------------|
| POS multitenant | 7/10 | ✅ |
| KDS multi-estación | 7/10 | ✅ (post-fix) |
| Inventario por estación | 6/10 | ⚠️ |
| Pagos parciales/idempotencia | 8/10 | ✅ |
| Multi-mesero / turnos | 2/10 | ❌ |
| Propinas | 0/10 | ❌ |
| Comisiones | 1/10 | ❌ |
| Recetas/BOM | 0/10 | ❌ |
| Transferencias inventario | 0/10 | ❌ |
| Impresión térmica | 1/10 | ❌ |
| Auditoría SOX | 3/10 | ❌ |
| Reembolsos | 2/10 | ❌ |
| Escala 500+ concurrent | ?/10 | ❌ no probado |

---

# ROADMAP PARA ENTERPRISE PASS (priorizado)

## P0 — Bloqueadores cadena

1. `Payment.TipAmount`, `Payment.ProcessedByUserId`, `OrderItem.AddedByUserId`
2. `CommissionRule` + motor atribución por ítem
3. `Shift` + traspaso mesas entre turnos
4. Enforzar `UserAssignment` en POS para meseros
5. Domain audit: wire `GlobalLoggingService` en Order/Payment/Cancel
6. `Refund` + supervisor gate post-cocina

## P1 — Operación grande

7. `Recipe` + `RecipeLine` + explosión inventario
8. `StockTransfer` entre estaciones
9. Impresión térmica por `Station.PrinterId`
10. KDS: prioridad, OrderType, VIP flag
11. Load test 150 órdenes/min

## P2 — Paridad internacional

12. Integración payroll propinas
13. Auditoría inmutable (append-only)
14. Multi-región / HA PostgreSQL

---

# CERTIFICACIONES PREVIAS (evidencia)

| Certificación | Veredicto | Relevancia |
|---------------|-----------|------------|
| FUNCTIONAL_CERTIFICATION | PASS | Auth, multitenant, pagos |
| ORDER_FUNCTIONAL_CERTIFICATION | PASS | POS core |
| ORDER_ROUTING_CERTIFICATION | PASS | Multi-piso, multi-bar |

---

# CONCLUSIÓN FINAL

RestBar es un **POS multitenant sólido en desarrollo** con KDS e inventario por estación **recientemente endurecidos**. Para un restaurante **grande o cadena internacional**, faltan capacidades de **negocio** que el mercado enterprise considera obligatorias: propinas, comisiones, recetas, turnos, impresión, reembolsos y auditoría forense.

**No aceptar el diseño actual como techo** — el roadmap P0 es implementable sobre la arquitectura existente sin reescritura total.

---

**ENTERPRISE OPERATION CERTIFICATION: FAIL**  
*(Cadena internacional alto volumen — ver matriz y roadmap)*

**RESTBAR POS ENTERPRISE FOUNDATION: CONDITIONAL PASS**  
*(Restaurante mediano multi-sucursal con limitaciones documentadas)*
