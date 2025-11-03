# Sistema de Inventario Completo - RestBar

## 📋 Resumen

Sistema completo de gestión de inventario que permite:
- **Trackear stock disponible** por producto (global y por estación)
- **Asignar productos a diferentes estaciones** basándose en disponibilidad de stock
- **Reducir stock automáticamente** cuando se agrega un producto a una orden
- **Restaurar stock** cuando se cancela una orden o pago
- **Verificar disponibilidad** antes de agregar items a una orden
- **Dirigir productos a estaciones** según stock disponible y prioridades

## 🏗️ Arquitectura Implementada

### 1. Modelos

#### ProductStockAssignment
Asignación de stock de un producto a una estación específica:
- `ProductId`: Producto asignado
- `StationId`: Estación donde está el stock
- `Stock`: Cantidad disponible en esta estación
- `MinStock`: Stock mínimo para alertas
- `Priority`: Prioridad de asignación (mayor = más prioridad)
- `IsActive`: Si la asignación está activa
- Multi-tenant: `CompanyId`, `BranchId`

#### Product - Campos de Inventario
- `Stock`: Stock global del producto
- `MinStock`: Stock mínimo para alertas
- `TrackInventory`: Si se controla inventario
- `AllowNegativeStock`: Permitir stock negativo

### 2. Servicios Implementados

#### ProductService - Métodos de Inventario
✅ **GetAvailableStockAsync(productId, branchId?)**
- Obtiene stock total (global + asignaciones por estación)
- Retorna `-1` si no se controla inventario (stock ilimitado)

✅ **GetStockInStationAsync(productId, stationId, branchId?)**
- Obtiene stock disponible en una estación específica
- Si no hay asignación, retorna stock global

✅ **FindBestStationForProductAsync(productId, requiredQuantity, branchId?)**
- Encuentra la mejor estación basándose en:
  1. Prioridad de asignación (mayor primero)
  2. Stock disponible (mayor primero)
  3. Stock suficiente para cantidad requerida

✅ **ReduceStockAsync(productId, quantity, stationId?, branchId?)**
- Reduce stock de un producto
- Si hay `stationId`, reduce stock de esa estación
- Si no hay estación, reduce stock global
- Valida stock suficiente (excepto si `AllowNegativeStock = true`)

✅ **RestoreStockAsync(productId, quantity, stationId?, branchId?)**
- Restaura stock al cancelar una orden
- Restaura en la misma estación donde se redujo

✅ **HasStockAvailableAsync(productId, quantity, branchId?)**
- Verifica si hay stock suficiente
- Retorna `true` si stock es ilimitado (`-1`)

#### ProductStockAssignmentService - CRUD Completo
✅ **GetAllAsync(branchId?)**: Obtiene todas las asignaciones
✅ **GetByIdAsync(id)**: Obtiene una asignación por ID
✅ **GetByProductIdAsync(productId, branchId?)**: Obtiene asignaciones de un producto
✅ **GetByStationIdAsync(stationId, branchId?)**: Obtiene asignaciones de una estación
✅ **CreateAsync(assignment)**: Crea una nueva asignación
✅ **UpdateAsync(id, assignment)**: Actualiza una asignación
✅ **DeleteAsync(id)**: Elimina una asignación

### 3. Controllers Implementados

#### ProductController - Endpoints de Inventario
✅ **GET /Product/GetAvailableStock**
- Obtiene stock disponible total de un producto
- Retorna: `{ success, stock, isUnlimited }`

✅ **GET /Product/GetStockInStation**
- Obtiene stock disponible en una estación específica
- Retorna: `{ success, stock }`

✅ **GET /Product/CheckStockAvailability**
- Verifica disponibilidad de stock para una cantidad
- Retorna: `{ success, hasStock, availableStock, isUnlimited }`

✅ **GET /Product/FindBestStation**
- Encuentra la mejor estación para un producto
- Retorna: `{ success, stationId, stationName, stockInStation }`

#### ProductStockAssignmentController - Gestión de Asignaciones
✅ **GET /ProductStockAssignment/Index**: Vista de asignaciones
✅ **GET /ProductStockAssignment/GetAssignments**: Obtiene asignaciones (por producto o estación)
✅ **POST /ProductStockAssignment/Create**: Crea una asignación
✅ **PUT /ProductStockAssignment/Update**: Actualiza una asignación
✅ **DELETE /ProductStockAssignment/Delete**: Elimina una asignación

#### OrderController - Verificación de Stock
✅ **GET /Order/CheckItemStockAvailability**
- Verifica disponibilidad antes de agregar item a orden
- Retorna:
  - `hasStock`: Si hay stock suficiente
  - `availableStock`: Stock disponible total
  - `bestStationId`: Mejor estación para asignar
  - `bestStationName`: Nombre de la estación
  - `stockInStation`: Stock en esa estación

### 4. Integración en OrderService

✅ **AddOrUpdateOrderWithPendingItemsAsync()** - Lógica mejorada:
1. **Verifica stock disponible** antes de crear items
2. **Encuentra mejor estación** basada en stock y prioridad
3. **Asigna estación al OrderItem** (`PreparedByStationId`)
4. **Reduce stock** después de agregar item exitosamente
   - Si hay estación asignada → reduce stock de esa estación
   - Si no hay estación → reduce stock global

✅ **CancelOrderAsync()** - Restauración de stock:
1. **Itera sobre todos los items** de la orden cancelada
2. **Restaura stock** en la misma estación donde se redujo
3. **Usa `PreparedByStationId`** para restaurar en la estación correcta

### 5. Flujo de Trabajo Completo

#### Al Crear una Orden:
```
1. Usuario agrega producto a orden
   ↓
2. Sistema verifica stock disponible
   ↓
3. Sistema encuentra mejor estación (si controla inventario)
   ↓
4. Sistema crea OrderItem con estación asignada
   ↓
5. Sistema reduce stock (global o por estación)
   ↓
6. Sistema guarda orden
```

#### Al Cancelar una Orden:
```
1. Usuario cancela orden
   ↓
2. Sistema itera sobre todos los items
   ↓
3. Sistema restaura stock en la misma estación
   ↓
4. Sistema actualiza estado de orden y mesa
```

## 📊 Endpoints Disponibles

### Consultar Stock
- `GET /Product/GetAvailableStock?productId={guid}&branchId={guid?}`
- `GET /Product/GetStockInStation?productId={guid}&stationId={guid}&branchId={guid?}`
- `GET /Product/CheckStockAvailability?productId={guid}&quantity={decimal}&branchId={guid?}`
- `GET /Product/FindBestStation?productId={guid}&requiredQuantity={decimal}&branchId={guid?}`

### Verificar Disponibilidad antes de Agregar a Orden
- `GET /Order/CheckItemStockAvailability?productId={guid}&quantity={decimal}&orderId={guid?}`

### Gestionar Asignaciones de Stock
- `GET /ProductStockAssignment/GetAssignments?productId={guid?}&stationId={guid?}`
- `POST /ProductStockAssignment/Create` (Body: `ProductStockAssignment`)
- `PUT /ProductStockAssignment/Update/{id}` (Body: `ProductStockAssignment`)
- `DELETE /ProductStockAssignment/Delete/{id}`

## 🎯 Casos de Uso

### Caso 1: Producto con Stock en Múltiples Estaciones
**Escenario:**
- Producto: "Pizza Margherita"
- Estación A (Cocina Principal): Stock = 10, Prioridad = 5
- Estación B (Cocina Express): Stock = 5, Prioridad = 3
- Orden requiere: 3 unidades

**Resultado:**
- Sistema asigna a Estación A (mayor prioridad y stock suficiente)
- Se reduce stock de Estación A: 10 → 7

### Caso 2: Producto con Stock Insuficiente
**Escenario:**
- Producto: "Hamburguesa Clásica"
- Estación A: Stock = 2
- Estación B: Stock = 1
- Orden requiere: 5 unidades
- `AllowNegativeStock = false`

**Resultado:**
- Error: "Stock insuficiente. Disponible: 3, Requerido: 5"
- No se crea el item

### Caso 3: Producto que Permite Stock Negativo
**Escenario:**
- Producto: "Bebida del Día"
- Stock global: 1
- Orden requiere: 5 unidades
- `AllowNegativeStock = true`

**Resultado:**
- Sistema permite la orden
- Stock queda en -4
- Se asigna a estación predeterminada

### Caso 4: Cancelación de Orden con Restauración
**Escenario:**
- Orden cancelada con:
  - Item 1: 3 unidades de Pizza (Estación A)
  - Item 2: 2 unidades de Hamburguesa (Estación B)

**Resultado:**
- Se restaura stock en Estación A: +3
- Se restaura stock en Estación B: +2

## 🔄 Integración con Frontend

### Ejemplo: Verificar Stock antes de Agregar Item

```javascript
async function checkStockBeforeAdd(productId, quantity) {
    try {
        const response = await fetch(
            `/Order/CheckItemStockAvailability?productId=${productId}&quantity=${quantity}`
        );
        const result = await response.json();
        
        if (result.success) {
            if (result.hasStock) {
                console.log(`✅ Stock disponible: ${result.availableStock}`);
                console.log(`🏪 Mejor estación: ${result.bestStationName} (Stock: ${result.stockInStation})`);
                // Proceder a agregar item
            } else {
                alert(`⚠️ Stock insuficiente. Disponible: ${result.availableStock}, Requerido: ${quantity}`);
                // No agregar item
            }
        } else {
            console.error('Error al verificar stock:', result.message);
        }
    } catch (error) {
        console.error('Error:', error);
    }
}
```

## ✅ Verificación del Sistema

### Pasos para Verificar:
1. **Crear producto con inventario:**
   - `TrackInventory = true`
   - `Stock = 10`

2. **Crear orden con el producto:**
   - Cantidad: 5
   - Verificar que el stock se reduce a 5
   - Verificar que se asigna estación correcta

3. **Cancelar la orden:**
   - Verificar que el stock se restaura a 10

4. **Asignar stock a diferentes estaciones:**
   - Estación A: Stock = 5, Prioridad = 5
   - Estación B: Stock = 3, Prioridad = 3
   - Crear orden con cantidad 2
   - Verificar que se asigna a Estación A

## 📚 Referencias

- `Models/Product.cs` - Modelo de producto con campos de inventario
- `Models/ProductStockAssignment.cs` - Modelo de asignación de stock por estación
- `Interfaces/IProductService.cs` - Interface con métodos de inventario
- `Services/ProductService.cs` - Implementación de métodos de inventario
- `Interfaces/IProductStockAssignmentService.cs` - Interface para gestión de asignaciones
- `Services/ProductStockAssignmentService.cs` - Implementación de CRUD de asignaciones
- `Controllers/ProductController.cs` - Endpoints de consulta de stock
- `Controllers/ProductStockAssignmentController.cs` - Endpoints de gestión de asignaciones
- `Controllers/OrderController.cs` - Endpoint de verificación antes de agregar items
- `Services/OrderService.cs` - Integración de inventario en creación de órdenes
- `Models/RestBarContext.cs` - Configuración de entidades en Entity Framework

## 🚀 Próximos Pasos Recomendados

1. **Vista de Gestión de Asignaciones:**
   - Crear vista `/ProductStockAssignment/Index.cshtml`
   - Formulario para crear/editar asignaciones
   - Tabla con stock por estación

2. **Alertas de Stock Bajo:**
   - Notificaciones cuando `Stock < MinStock`
   - Dashboard de inventario con productos con stock bajo

3. **Historial de Movimientos:**
   - Tabla `stock_movements` para trackear todos los cambios
   - Reportes de consumo por producto/estación

4. **Integración Frontend:**
   - Mostrar stock disponible al seleccionar producto
   - Indicar estación asignada en items de orden
   - Alertas visuales de stock bajo

