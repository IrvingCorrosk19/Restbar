# Análisis del Flujo de Ventas, Compras y Selección de Órdenes a Estaciones

## Fecha: 2025-01-XX

---

## 📋 ÍNDICE

1. [Flujo General de Ventas](#flujo-general-de-ventas)
2. [Creación de Órdenes](#creación-de-órdenes)
3. [Selección de Estaciones](#selección-de-estaciones)
4. [Procesamiento de Pagos](#procesamiento-de-pagos)
5. [Actualización de Estados](#actualización-de-estados)
6. [Problemas Identificados](#problemas-identificados)
7. [Mejoras Sugeridas](#mejoras-sugeridas)

---

## 🔄 FLUJO GENERAL DE VENTAS

### Diagrama de Flujo

```
┌─────────────┐
│  Selección  │
│    Mesa      │
└──────┬───────┘
       │
       ▼
┌─────────────┐
│  Agregar    │
│  Productos  │
└──────┬───────┘
       │
       ▼
┌─────────────┐      ┌──────────────┐
│  Enviar a   │─────▶│  Selección   │
│   Cocina    │      │  Estación    │
└──────┬───────┘      └──────┬───────┘
       │                      │
       │                      ▼
       │              ┌──────────────┐
       │              │  Asignación  │
       │              │  de Estación │
       │              └──────┬───────┘
       │                     │
       ▼                     ▼
┌─────────────┐      ┌──────────────┐
│  Reducir    │      │  Notificar   │
│  Inventario │      │   Cocina     │
└──────┬───────┘      └──────┬───────┘
       │                     │
       └──────────┬──────────┘
                  │
                  ▼
         ┌─────────────┐
         │  Preparación│
         │  en Cocina  │
         └──────┬──────┘
                │
                ▼
         ┌─────────────┐
         │  Items      │
         │  Listos     │
         └──────┬──────┘
                │
                ▼
         ┌─────────────┐
         │  Procesar   │
         │   Pago      │
         └──────┬──────┘
                │
                ▼
         ┌─────────────┐
         │  Actualizar │
         │   Estados   │
         └─────────────┘
```

---

## 🛒 CREACIÓN DE ÓRDENES

### 1. Frontend: Selección de Mesa y Productos

**Archivo:** `wwwroot/js/order/order-operations.js`

```javascript
// Usuario selecciona mesa
currentOrder = {
    tableId: selectedTableId,
    items: [],
    total: 0
};

// Usuario agrega productos
currentOrder.items.push({
    id: Guid.NewGuid(),
    productId: productId,
    quantity: quantity,
    notes: notes,
    discount: 0
});
```

### 2. Frontend: Envío a Cocina

**Flujo según Rol:**

#### A. Usuario ADMIN/SUPERADMIN
```javascript
// 1. Verificar rol
const userRole = await getCurrentUserRole();

// 2. Si es admin, mostrar modal de selección de estación
if (userRole === 'admin' || userRole === 'superadmin') {
    await showStationSelectionModal(); // Modal con dropdown de estaciones
    return;
}

// 3. Usuario selecciona estación manualmente
// 4. Se envía SelectedStationId en el DTO
```

#### B. Usuario SALONERO (Waiter)
```javascript
// Flujo automático sin modal
await sendOrderToKitchen(null); // SelectedStationId = null
```

### 3. Backend: Procesamiento de Orden

**Archivo:** `Services/OrderService.cs` - Método `AddOrUpdateOrderWithPendingItemsAsync()`

#### Paso 1: Crear o Actualizar Orden
```csharp
// Verificar si existe orden activa para la mesa
var existingOrder = await _context.Orders
    .FirstOrDefaultAsync(o => o.TableId == dto.TableId && 
        o.Status != OrderStatus.Cancelled && 
        o.Status != OrderStatus.Completed);

if (existingOrder == null) {
    // Crear nueva orden
    order = new Order {
        Id = Guid.NewGuid(),
        OrderNumber = await GenerateOrderNumberAsync(),
        TableId = dto.TableId,
        Status = OrderStatus.SentToKitchen,
        CompanyId = companyId,
        BranchId = branchId
    };
} else {
    // Actualizar orden existente
    order.Status = OrderStatus.SentToKitchen; // Si hay nuevos items
}
```

#### Paso 2: Procesar Items Individualmente
```csharp
foreach (var itemDto in dto.Items) {
    // 1. Validar producto existe
    var product = await _productService.GetByIdAsync(itemDto.ProductId);
    
    // 2. Verificar stock disponible
    if (product.TrackInventory) {
        var hasStock = await _productService.HasStockAvailableAsync(
            product.Id, itemDto.Quantity, order.BranchId);
        if (!hasStock) {
            throw new InvalidOperationException("Stock insuficiente");
        }
    }
    
    // 3. ASIGNAR ESTACIÓN (ver sección siguiente)
    Guid? assignedStationId = AssignStation(dto, userId, product, itemDto);
    
    // 4. Crear OrderItem
    var newItem = new OrderItem {
        Id = itemDto.Id,
        ProductId = itemDto.ProductId,
        Quantity = itemDto.Quantity,
        PreparedByStationId = assignedStationId, // ✅ Estación asignada
        Status = OrderItemStatus.Pending,
        KitchenStatus = KitchenStatus.Pending
    };
    
    // 5. Reducir stock
    if (product.TrackInventory && assignedStationId.HasValue) {
        await _productService.ReduceStockAsync(
            product.Id, itemDto.Quantity, assignedStationId.Value, order.BranchId);
    }
}
```

---

## 🎯 SELECCIÓN DE ESTACIONES

### Prioridad de Asignación (3 Niveles)

**Archivo:** `Services/OrderService.cs` - Líneas 1671-1752

#### **PRIORIDAD 1: Selección Manual por Admin** ⭐ (Máxima Prioridad)

```csharp
if (dto.SelectedStationId.HasValue) {
    assignedStationId = dto.SelectedStationId.Value;
    Console.WriteLine($"✅ Usando estación seleccionada manualmente por admin");
}
```

**Cuándo se usa:**
- Usuario es `admin` o `superadmin`
- Usuario seleccionó estación en el modal del frontend
- Se envía `SelectedStationId` en el `SendOrderDto`

**Ventajas:**
- Control total del administrador
- Permite redirigir órdenes a estaciones específicas
- Útil para balancear carga de trabajo

---

#### **PRIORIDAD 2: Estación del Salonero (Waiter)** 🍽️

```csharp
// Obtener estación del salonero desde UserAssignment
if (userId.HasValue) {
    var userAssignment = await _userAssignmentService.GetActiveByUserIdAsync(userId.Value);
    if (userAssignment != null && userAssignment.StationId.HasValue) {
        waiterStationId = userAssignment.StationId.Value;
        
        // Validar stock en la estación del salonero
        if (product.TrackInventory) {
            var stationStock = await _productService.GetStockInStationAsync(
                product.Id, waiterStationId.Value, order.BranchId);
            
            if (stationStock < itemDto.Quantity && !product.AllowNegativeStock) {
                // ⚠️ ADVERTENCIA: Stock insuficiente pero continúa
                Console.WriteLine($"⚠️ Stock insuficiente en estación del salonero");
            }
        }
        
        assignedStationId = waiterStationId;
    }
}
```

**Cuándo se usa:**
- Usuario es `waiter` (salonero)
- Usuario tiene `UserAssignment` activo con `StationId`
- No hay `SelectedStationId` (admin no seleccionó manualmente)

**Ventajas:**
- Órdenes van directamente a la estación del salonero asignado
- Flujo automático sin intervención
- Mejor organización por áreas

**Problema Identificado:**
- ⚠️ Si el producto no tiene stock en la estación del salonero, se asigna igualmente (solo advertencia)
- ⚠️ No hay validación estricta de stock antes de asignar

---

#### **PRIORIDAD 3: Lógica del Producto (Fallback)** 🔄

```csharp
// Si el salonero no tiene estación asignada
if (!waiterStationId.HasValue) {
    // Encontrar la mejor estación basada en stock disponible
    var bestStationId = await _productService.FindBestStationForProductAsync(
        product.Id, itemDto.Quantity, order.BranchId);
    
    if (!bestStationId.HasValue && product.TrackInventory && !product.AllowNegativeStock) {
        throw new InvalidOperationException(
            $"No hay estación disponible con stock suficiente para {product.Name}");
    }
    
    assignedStationId = bestStationId;
}
```

**Cuándo se usa:**
- No hay `SelectedStationId` (admin no seleccionó)
- Salonero no tiene estación asignada en `UserAssignment`
- Fallback automático

**Lógica de `FindBestStationForProductAsync()`:**

1. **Producto NO controla inventario:**
   - Usa la primera asignación de `ProductStockAssignment` disponible
   - No valida stock

2. **Producto controla inventario:**
   - Busca asignaciones con stock suficiente (`Stock >= requiredQuantity`)
   - Ordena por `Priority` (mayor = mejor)
   - Si hay empate, usa la que tiene más stock
   - Si no hay stock suficiente pero `AllowNegativeStock = true`, usa la primera asignación
   - Si no hay stock suficiente y `AllowNegativeStock = false`, retorna `null` (error)

**Ventajas:**
- Asignación inteligente basada en stock disponible
- Considera prioridad de estaciones
- Fallback automático cuando no hay estación del salonero

**Problema Identificado:**
- ⚠️ Si no hay stock suficiente y `AllowNegativeStock = false`, lanza excepción (puede interrumpir toda la orden)

---

## 💳 PROCESAMIENTO DE PAGOS

### Flujo de Pago

**Archivo:** `Controllers/PaymentController.cs` - Método `CreatePartialPayment()`

#### Paso 1: Validaciones
```csharp
// 1. Validar orden existe
var order = await _orderService.GetOrderWithDetailsAsync(request.OrderId);

// 2. Validar montos
var totalPaid = await _paymentService.GetTotalPaymentsByOrderAsync(request.OrderId);
var orderTotal = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
var remainingAmount = orderTotal - totalPaid;

if (request.Amount > remainingAmount) {
    return BadRequest("El monto excede el saldo pendiente");
}

// 3. Validar pagos compartidos
if (request.IsShared) {
    if (request.Method != "Compartido") {
        return BadRequest("Pago compartido debe tener método 'Compartido'");
    }
    // Validar suma de splits = monto total
}
```

#### Paso 2: Crear Pago
```csharp
var payment = new Payment {
    Id = Guid.NewGuid(),
    OrderId = request.OrderId,
    Amount = request.Amount,
    Method = request.Method,
    IsShared = request.IsShared,
    PaidAt = DateTime.UtcNow
};

await _paymentService.CreateAsync(payment);

// Crear splits si es pago compartido
if (request.IsShared && request.SplitPayments != null) {
    foreach (var split in request.SplitPayments) {
        await _splitPaymentService.CreateAsync(new SplitPayment {
            PaymentId = payment.Id,
            PersonName = split.PersonName,
            Amount = split.Amount,
            Method = split.Method
        });
    }
}
```

#### Paso 3: Actualizar Estados (Ver sección siguiente)

---

## 🔄 ACTUALIZACIÓN DE ESTADOS

### Estados de Orden

**Archivo:** `Controllers/PaymentController.cs` - Líneas 193-337

#### Lógica de Actualización según Pago

```csharp
var hasPendingItems = order.OrderItems.Any(oi => 
    oi.Status == OrderItemStatus.Pending || 
    oi.Status == OrderItemStatus.Preparing);
    
var hasReadyItems = order.OrderItems.Any(oi => 
    oi.Status == OrderItemStatus.Ready);
    
var allItemsReadyOrServed = order.OrderItems.All(oi => 
    oi.Status == OrderItemStatus.Ready || 
    oi.Status == OrderItemStatus.Served);

if (isFullyPaid) {
    // PAGO COMPLETO
    if (allItemsReadyOrServed) {
        // Todos los items están listos o servidos
        order.Status = OrderStatus.Completed;
        table.Status = TableStatus.Disponible;
        
        // Marcar items listos como servidos
        foreach (var item in order.OrderItems.Where(oi => oi.Status == OrderItemStatus.Ready)) {
            item.Status = OrderItemStatus.Served;
        }
    } else if (hasPendingItems || hasReadyItems) {
        // Hay items pendientes o listos
        order.Status = OrderStatus.ReadyToPay;
        table.Status = TableStatus.ParaPago;
        
        // Marcar items listos como servidos
        foreach (var item in order.OrderItems.Where(oi => oi.Status == OrderItemStatus.Ready)) {
            item.Status = OrderItemStatus.Served;
        }
    } else {
        // Todos servidos pero orden no completada
        order.Status = OrderStatus.Completed;
        table.Status = TableStatus.Disponible;
    }
} else {
    // PAGO PARCIAL
    if (hasPendingItems || hasReadyItems) {
        order.Status = OrderStatus.ReadyToPay;
        if (table.Status != TableStatus.EnPreparacion) {
            table.Status = TableStatus.ParaPago;
        }
    } else if (allItemsReadyOrServed) {
        order.Status = OrderStatus.Served;
        table.Status = TableStatus.ParaPago;
        
        // Marcar items listos como servidos
        foreach (var item in order.OrderItems.Where(oi => oi.Status == OrderItemStatus.Ready)) {
            item.Status = OrderItemStatus.Served;
        }
    }
}
```

### Estados de Mesa

| Estado de Mesa | Descripción | Cuándo se Asigna |
|----------------|-------------|------------------|
| `Disponible` | Mesa libre | Orden completada y pagada |
| `Ocupada` | Mesa con orden activa | Orden creada |
| `EnPreparacion` | Items en preparación | Hay items `Pending` o `Preparing` |
| `ParaPago` | Lista para pagar | Items listos o pago parcial |
| `Servida` | Items servidos | Items en estado `Ready` |

---

## ⚠️ PROBLEMAS IDENTIFICADOS

### 1. **Validación de Stock en Estación del Salonero** 🔴

**Problema:**
```csharp
// Línea 1716-1720
if (stationStock < itemDto.Quantity && !product.AllowNegativeStock) {
    Console.WriteLine($"⚠️ Stock insuficiente en estación del salonero");
    // ⚠️ NO LANZA ERROR - Continúa con la asignación
}
```

**Impacto:**
- Se asigna estación sin stock suficiente
- Puede causar problemas en la preparación
- No hay validación estricta

**Solución Sugerida:**
```csharp
if (stationStock < itemDto.Quantity && !product.AllowNegativeStock) {
    // Opción 1: Lanzar error
    throw new InvalidOperationException(
        $"Stock insuficiente en estación del salonero. Disponible: {stationStock}, Requerido: {itemDto.Quantity}");
    
    // Opción 2: Fallback a otra estación
    assignedStationId = await _productService.FindBestStationForProductAsync(
        product.Id, itemDto.Quantity, order.BranchId);
}
```

---

### 2. **Manejo de Errores en Asignación de Estación** 🟡

**Problema:**
```csharp
// Línea 1735-1739
if (!bestStationId.HasValue && product.TrackInventory && !product.AllowNegativeStock) {
    throw new InvalidOperationException(
        $"No hay estación disponible con stock suficiente para {product.Name}");
}
```

**Impacto:**
- Si falla la asignación de un item, falla toda la orden
- No permite crear orden parcial

**Solución Sugerida:**
- Permitir crear items sin estación asignada (estado especial)
- Notificar al admin para asignación manual
- O permitir `AllowNegativeStock` temporalmente

---

### 3. **Falta de Validación de Estación Seleccionada Manualmente** 🟡

**Problema:**
```csharp
// Línea 1678-1681
if (dto.SelectedStationId.HasValue) {
    assignedStationId = dto.SelectedStationId.Value;
    // ⚠️ NO VALIDA si la estación existe o está activa
}
```

**Impacto:**
- Admin puede seleccionar estación inexistente
- No valida si la estación está activa
- No valida si el producto puede prepararse en esa estación

**Solución Sugerida:**
```csharp
if (dto.SelectedStationId.HasValue) {
    var station = await _context.Stations
        .FirstOrDefaultAsync(s => s.Id == dto.SelectedStationId.Value && s.IsActive);
    
    if (station == null) {
        throw new InvalidOperationException("La estación seleccionada no existe o no está activa");
    }
    
    // Validar que el producto tenga asignación en esa estación
    var hasAssignment = await _context.ProductStockAssignments
        .AnyAsync(psa => psa.ProductId == product.Id && 
                        psa.StationId == station.Id && 
                        psa.IsActive);
    
    if (!hasAssignment && product.TrackInventory) {
        throw new InvalidOperationException(
            $"El producto {product.Name} no tiene asignación en la estación {station.Name}");
    }
    
    assignedStationId = station.Id;
}
```

---

### 4. **Reducción de Stock Antes de Confirmar Orden** 🔴

**Problema:**
```csharp
// Línea 1792-1800
if (product.TrackInventory && assignedStationId.HasValue) {
    await _productService.ReduceStockAsync(
        product.Id, itemDto.Quantity, assignedStationId.Value, order.BranchId);
}
```

**Impacto:**
- Stock se reduce inmediatamente al crear la orden
- Si la orden se cancela, el stock no se restaura automáticamente
- No hay reserva temporal de stock

**Solución Sugerida:**
- Implementar reserva temporal de stock
- Restaurar stock si la orden se cancela
- O reducir stock solo cuando el item se marca como "Preparing"

---

### 5. **Falta de Logging Estructurado** 🟡

**Problema:**
- Uso de `Console.WriteLine` en lugar de `ILogger`
- Logs no estructurados
- Difícil de filtrar y analizar

**Solución Sugerida:**
- Migrar a `LoggingHelper` (ya implementado en otros servicios)
- Agregar logs estructurados con contexto
- Niveles apropiados (Information, Warning, Error)

---

## ✅ MEJORAS SUGERIDAS

### 1. **Validación Estricta de Stock en Estación del Salonero**

```csharp
if (waiterStationId.HasValue) {
    if (product.TrackInventory) {
        var stationStock = await _productService.GetStockInStationAsync(
            product.Id, waiterStationId.Value, order.BranchId);
        
        if (stationStock < itemDto.Quantity && !product.AllowNegativeStock) {
            // Fallback a mejor estación disponible
            Console.WriteLine($"⚠️ Stock insuficiente en estación del salonero, buscando alternativa...");
            assignedStationId = await _productService.FindBestStationForProductAsync(
                product.Id, itemDto.Quantity, order.BranchId);
            
            if (!assignedStationId.HasValue) {
                throw new InvalidOperationException(
                    $"No hay stock suficiente para {product.Name} en ninguna estación");
            }
        } else {
            assignedStationId = waiterStationId;
        }
    } else {
        assignedStationId = waiterStationId;
    }
}
```

---

### 2. **Validación de Estación Seleccionada Manualmente**

```csharp
if (dto.SelectedStationId.HasValue) {
    // Validar estación existe y está activa
    var station = await _context.Stations
        .FirstOrDefaultAsync(s => s.Id == dto.SelectedStationId.Value && s.IsActive);
    
    if (station == null) {
        throw new InvalidOperationException(
            "La estación seleccionada no existe o no está activa");
    }
    
    // Validar asignación de producto en estación
    if (product.TrackInventory) {
        var hasAssignment = await _context.ProductStockAssignments
            .AnyAsync(psa => psa.ProductId == product.Id && 
                            psa.StationId == station.Id && 
                            psa.IsActive);
        
        if (!hasAssignment) {
            throw new InvalidOperationException(
                $"El producto {product.Name} no tiene asignación en la estación {station.Name}");
        }
    }
    
    assignedStationId = station.Id;
}
```

---

### 3. **Reserva Temporal de Stock**

```csharp
// En lugar de reducir stock inmediatamente
// Crear reserva temporal
var stockReservation = new StockReservation {
    ProductId = product.Id,
    StationId = assignedStationId.Value,
    Quantity = itemDto.Quantity,
    OrderItemId = newItem.Id,
    ExpiresAt = DateTime.UtcNow.AddMinutes(30) // Reserva por 30 minutos
};

await _context.StockReservations.AddAsync(stockReservation);

// Reducir stock solo cuando se confirma la preparación
// O restaurar si la orden se cancela
```

---

### 4. **Logging Estructurado**

```csharp
// Migrar de Console.WriteLine a LoggingHelper
LoggingHelper.LogInfo(_logger, nameof(OrderService), nameof(AddOrUpdateOrderWithPendingItemsAsync),
    $"Procesando {dto.Items.Count} items para mesa {dto.TableId}");

LoggingHelper.LogSuccess(_logger, nameof(OrderService), nameof(AddOrUpdateOrderWithPendingItemsAsync),
    $"Estación asignada: {assignedStationId.Value} para producto {product.Name}");

LoggingHelper.LogWarning(_logger, nameof(OrderService), nameof(AddOrUpdateOrderWithPendingItemsAsync),
    $"Stock insuficiente en estación del salonero, usando fallback");
```

---

### 5. **Manejo de Errores Mejorado**

```csharp
try {
    // Asignar estación
    assignedStationId = await AssignStationAsync(...);
} catch (InvalidOperationException ex) {
    // Si falla asignación, crear item sin estación (requiere asignación manual)
    LoggingHelper.LogWarning(_logger, nameof(OrderService), nameof(AddOrUpdateOrderWithPendingItemsAsync),
        $"No se pudo asignar estación automáticamente: {ex.Message}");
    
    // Crear item con estado especial
    newItem.PreparedByStationId = null;
    newItem.Status = OrderItemStatus.PendingManualAssignment;
    
    // Notificar al admin
    await _notificationService.CreateAsync(new Notification {
        Type = NotificationType.StationAssignmentRequired,
        Message = $"Se requiere asignación manual de estación para {product.Name}",
        OrderId = order.Id,
        OrderItemId = newItem.Id
    });
}
```

---

## 📊 RESUMEN

### Flujo Actual
1. ✅ Usuario selecciona mesa y productos
2. ✅ Admin puede seleccionar estación manualmente
3. ✅ Salonero usa su estación asignada automáticamente
4. ✅ Fallback a mejor estación basada en stock
5. ⚠️ Validaciones de stock no estrictas en algunos casos
6. ⚠️ Stock se reduce inmediatamente (sin reserva)
7. ✅ Pagos procesados correctamente
8. ✅ Estados actualizados según lógica de negocio

### Prioridades de Mejora
1. 🔴 **ALTA**: Validación estricta de stock en estación del salonero
2. 🔴 **ALTA**: Validación de estación seleccionada manualmente
3. 🟡 **MEDIA**: Reserva temporal de stock
4. 🟡 **MEDIA**: Logging estructurado
5. 🟢 **BAJA**: Manejo de errores mejorado

---

## 📝 NOTAS FINALES

- El flujo general está bien diseñado con 3 niveles de prioridad
- La lógica de asignación de estaciones es inteligente pero necesita validaciones más estrictas
- El procesamiento de pagos es robusto y maneja casos complejos
- Se recomienda implementar las mejoras sugeridas para mayor robustez

---

**Autor:** Análisis Automático del Sistema RestBar  
**Fecha:** 2025-01-XX

