# 📊 TABLAS AFECTADAS AL SOLICITAR UNA ORDEN

## 🔍 MÉTODO PRINCIPAL
`OrderService.AddOrUpdateOrderWithPendingItemsAsync()`

---

## ✅ TABLAS QUE SE MODIFICAN (INSERT/UPDATE/DELETE)

### 1. 📋 **`orders`** (Tabla Principal)
**OPERACIÓN:** `INSERT` (nueva orden) o `UPDATE` (orden existente)

**CAMPOS MODIFICADOS:**
- `id` - GUID generado (si es nueva)
- `OrderNumber` - Número único generado secuencialmente
- `TableId` - ID de la mesa asociada
- `UserId` - ID del usuario que crea la orden
- `OrderType` - Tipo de orden (dine_in, take_out, delivery)
- `Status` - Estado de la orden:
  - Nueva: `SentToKitchen`
  - Existente: Puede cambiar según estado actual
- `OpenedAt` - Fecha/hora de apertura (si es nueva)
- `TotalAmount` - Total calculado después de agregar items
- `CompanyId` - ID de la compañía (del usuario)
- `BranchId` - ID de la sucursal (del usuario)
- `CreatedAt` - Fecha de creación (si es nueva)
- `UpdatedAt` - Fecha de actualización (si es existente)
- `CreatedBy` - Usuario que crea (si es nueva)
- `UpdatedBy` - Usuario que actualiza (si es existente)

**CONDICIONES:**
- Si no existe orden activa → **INSERT**
- Si existe orden activa → **UPDATE** (cambia estado y actualiza campos)

---

### 2. 📦 **`order_items`** (Items de la Orden)
**OPERACIÓN:** `INSERT` (múltiples items)

**CAMPOS MODIFICADOS:**
- `id` - GUID generado por cada item
- `OrderId` - ID de la orden (relación FK)
- `ProductId` - ID del producto solicitado
- `Quantity` - Cantidad solicitada
- `UnitPrice` - Precio unitario del producto (del producto actual)
- `Discount` - Descuento aplicado (si hay)
- `Notes` - Notas del item (si hay)
- `Status` - Estado del item:
  - `Pending` (por defecto)
  - O el estado especificado en el DTO
- `KitchenStatus` - Estado en cocina: `Pending` (por defecto)
- `CompanyId` - ID de la compañía (heredado de la orden)
- `BranchId` - ID de la sucursal (heredado de la orden)
- `CreatedAt` - Fecha de creación
- `CreatedBy` - Usuario que crea
- `UpdatedAt` - Fecha de actualización (igual a CreatedAt inicialmente)
- `UpdatedBy` - Usuario que crea (igual a CreatedBy inicialmente)

**NOTA:**
- Se inserta **UN REGISTRO** por cada item en el DTO
- Se validan duplicados antes de insertar
- Se verifica que el producto exista antes de insertar

---

### 3. 🪑 **`tables`** (Mesas)
**OPERACIÓN:** `UPDATE` (solo el estado)

**CAMPOS MODIFICADOS:**
- `Status` - Estado de la mesa:
  - `EnPreparacion` - Si hay items pendientes/preparándose
  - `ParaPago` - Si todos los items están listos
  - `Servida` - Si hay items listos pero no todos
  - `Ocupada` - Si no hay items listos
  - `Disponible` - Solo si no hay órdenes activas (en cancelación)

**CONDICIONES:**
- Solo se actualiza si la orden tiene una mesa asociada (`TableId` != null)
- El estado depende de los estados de los items de la orden

---

## 📖 TABLAS QUE SE CONSULTAN (SELECT - Solo Lectura)

### 4. 👤 **`users`** (Usuarios)
**OPERACIÓN:** `SELECT` (solo lectura)

**CONSULTA:**
```sql
SELECT u.*, b.* 
FROM users u
LEFT JOIN branches b ON u.branch_id = b.id
WHERE u.id = @userId
```

**PROPÓSITO:**
- Obtener `CompanyId` y `BranchId` del usuario actual
- Si no se encuentra, se intenta desde Claims

**CAMPOS LEÍDOS:**
- `id` - ID del usuario
- `branch_id` - ID de la sucursal del usuario
- `branch.company_id` - ID de la compañía (desde la sucursal)

---

### 5. 🏷️ **`products`** (Productos)
**OPERACIÓN:** `SELECT` (solo lectura)

**CONSULTA:**
```sql
SELECT * FROM products WHERE id = @productId
```

**PROPÓSITO:**
- Obtener el precio actual del producto
- Validar que el producto existe
- Obtener información del producto (nombre, etc.)

**CAMPOS LEÍDOS:**
- `id` - ID del producto
- `Price` - Precio unitario (se copia a `UnitPrice` del item)
- `Name` - Nombre del producto (para logs)
- Otros campos para validación

---

### 6. 💰 **`payments`** (Pagos)
**OPERACIÓN:** `SELECT` (solo lectura - si hay orden existente)

**CONSULTA:**
```sql
SELECT SUM(amount) 
FROM payments 
WHERE order_id = @orderId AND is_voided = false
```

**PROPÓSITO:**
- Verificar si la orden tiene pagos parciales
- Validar estado de la orden antes de agregar items
- Determinar si se pueden agregar items a una orden con pagos

**CONDICIONES:**
- Solo se consulta si hay una orden existente
- Si hay pagos parciales, se registra en logs pero no se cancelan

---

### 7. 📊 **`orders`** (Consulta para verificar existente)
**OPERACIÓN:** `SELECT` (solo lectura - búsqueda inicial)

**CONSULTA:**
```sql
SELECT * FROM orders 
WHERE table_id = @tableId 
  AND status IN ('SentToKitchen', 'ReadyToPay', 'Preparing')
```

**PROPÓSITO:**
- Buscar si ya existe una orden activa para la mesa
- Determinar si se crea nueva orden o se actualiza existente

---

## 🔄 OPERACIONES ADICIONALES (Después de SaveChanges)

### 8. 📡 **SignalR Notifications** (No es tabla, pero se ejecuta)
**OPERACIÓN:** Notificaciones en tiempo real

**NOTIFICACIONES ENVIADAS:**
- `NotifyOrderStatusChanged()` - Cambio de estado de orden
- `NotifyKitchenUpdate()` - Actualización general de cocina
- `NotifyTableStatusChanged()` - Cambio de estado de mesa

---

## 📝 RESUMEN DE OPERACIONES POR TABLA

| Tabla | Operación | Tipo | Cantidad | Condición |
|-------|-----------|------|----------|-----------|
| `orders` | INSERT/UPDATE | Modificación | 1 | Nueva orden o existente |
| `order_items` | INSERT | Modificación | N (cantidad de items) | Siempre (por cada item) |
| `tables` | UPDATE | Modificación | 1 | Si hay mesa asociada |
| `users` | SELECT | Lectura | 1 | Para obtener CompanyId/BranchId |
| `products` | SELECT | Lectura | N (cantidad de items) | Por cada item para obtener precio |
| `payments` | SELECT | Lectura | 0 o 1 consulta | Solo si hay orden existente |
| `orders` | SELECT | Lectura | 1 | Búsqueda inicial de orden existente |

---

## 🔍 FLUJO DETALLADO DE OPERACIONES

### Paso 1: Verificar Orden Existente
```
SELECT orders WHERE table_id = X AND status IN (...)
```

### Paso 2: Obtener Datos del Usuario (si es nueva orden)
```
SELECT users + branches WHERE user_id = Y
```

### Paso 3A: Si es Nueva Orden
```
INSERT INTO orders (id, OrderNumber, TableId, UserId, ...)
```

### Paso 3B: Si es Orden Existente
```
SELECT payments WHERE order_id = Z (verificar pagos)
UPDATE orders SET status = 'SentToKitchen', UpdatedAt = NOW(), ...
```

### Paso 4: Procesar Items
```
FOR EACH item:
  SELECT products WHERE id = item.ProductId
  INSERT INTO order_items (id, OrderId, ProductId, Quantity, UnitPrice, ...)
END FOR
```

### Paso 5: Recalcular Total
```
UPDATE orders SET TotalAmount = (SELECT SUM(...) FROM order_items WHERE OrderId = X)
```

### Paso 6: Actualizar Estado de Mesa
```
SELECT order_items WHERE OrderId = X (para verificar estados)
UPDATE tables SET Status = 'EnPreparacion' WHERE id = Y
```

### Paso 7: Guardar Cambios
```
SaveChangesAsync() - Ejecuta todos los INSERT/UPDATE pendientes
```

### Paso 8: Notificaciones SignalR
```
NotifyOrderStatusChanged()
NotifyKitchenUpdate()
NotifyTableStatusChanged()
```

---

## ⚠️ CONSIDERACIONES IMPORTANTES

1. **Transacciones:**
   - Todas las operaciones se ejecutan en una sola transacción (`SaveChangesAsync`)
   - Si falla cualquier operación, se hace rollback completo

2. **Validaciones:**
   - Se validan duplicados de items antes de insertar
   - Se valida que los productos existan
   - Se valida estado de orden antes de agregar items

3. **Multi-tenancy:**
   - `CompanyId` y `BranchId` se heredan de la orden a los items
   - Se obtienen del usuario actual o de Claims

4. **Auditoría:**
   - Todos los registros tienen `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
   - Se establecen automáticamente usando `BaseTrackingService`

5. **Inventario:**
   - **NOTA:** Actualmente NO se reduce inventario al solicitar orden
   - La restauración de inventario solo se hace al cancelar orden

---

## ✅ RESUMEN EJECUTIVO

**TABLAS MODIFICADAS DIRECTAMENTE:**
1. ✅ `orders` - 1 registro (INSERT o UPDATE)
2. ✅ `order_items` - N registros (INSERT, uno por item)
3. ✅ `tables` - 1 registro (UPDATE del estado)

**TABLAS CONSULTADAS:**
4. 📖 `users` - Para obtener CompanyId/BranchId
5. 📖 `products` - Para obtener precio y validar existencia
6. 📖 `payments` - Para verificar pagos parciales (si hay orden existente)
7. 📖 `orders` - Para buscar orden existente

**OPERACIONES ADICIONALES:**
8. 📡 SignalR - Notificaciones en tiempo real

**TOTAL:** 3 tablas modificadas + 4 tablas consultadas + notificaciones SignalR

