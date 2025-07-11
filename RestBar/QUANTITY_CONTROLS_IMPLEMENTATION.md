# ✅ CONTROLES DE CANTIDAD PARA ITEMS PENDING

## 🎯 **OBJETIVO**
Implementar controles en el frontend para agregar o eliminar `OrderItem` con estado `Pending` mientras la orden esté en estado `SentToKitchen`.

## 🔧 **FUNCIONALIDAD IMPLEMENTADA**

### **1. Condiciones de Modificación**

#### **Items Modificables:**
- ✅ **Estado del Item**: `Pending`
- ✅ **Estado de la Orden**: `SentToKitchen`
- ✅ **Acciones Permitidas**: Aumentar cantidad, disminuir cantidad, eliminar

#### **Items No Modificables:**
- ❌ **Items en estado**: `Preparing`, `Ready`, `Served`
- ❌ **Órdenes en estado**: `Pending`, `ReadyToPay`, `Completed`, `Cancelled`

### **2. Controles Visuales Implementados**

#### **Para Items Modificables:**
```html
<div class="quantity-controls d-flex align-items-center">
    <button class="btn btn-sm btn-outline-secondary" onclick="decreaseQuantity('itemId')" title="Disminuir">
        <i class="fas fa-minus"></i>
    </button>
    <span class="mx-2 fw-bold">2</span>
    <button class="btn btn-sm btn-outline-secondary" onclick="increaseQuantity('itemId')" title="Aumentar">
        <i class="fas fa-plus"></i>
    </button>
</div>
```

#### **Para Items No Modificables:**
```html
<span class="fw-bold">2</span>
```

### **3. Funciones JavaScript Implementadas**

#### **increaseQuantity(itemId)**
```javascript
// ✅ Función para aumentar cantidad de un item específico
async function increaseQuantity(itemId) {
    // Verificar condiciones de modificación
    if (item.status !== 'Pending' || currentOrder.status !== 'SentToKitchen') {
        Swal.fire('Atención', 'Solo se pueden modificar items pendientes en órdenes enviadas a cocina', 'warning');
        return;
    }
    
    // Llamar al endpoint para actualizar cantidad
    const response = await fetch('/Order/UpdateItemQuantityInOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            orderId: currentOrder.orderId,
            productId: item.productId,
            quantity: newQuantity,
            status: item.status
        })
    });
    
    // Actualizar UI y mostrar notificación
}
```

#### **decreaseQuantity(itemId)**
```javascript
// ✅ Función para disminuir cantidad de un item específico
async function decreaseQuantity(itemId) {
    // Verificar condiciones de modificación
    if (item.status !== 'Pending' || currentOrder.status !== 'SentToKitchen') {
        Swal.fire('Atención', 'Solo se pueden modificar items pendientes en órdenes enviadas a cocina', 'warning');
        return;
    }
    
    // Confirmar eliminación si cantidad será 0
    if (item.quantity === 1) {
        const result = await Swal.fire({
            title: '¿Eliminar Item?',
            text: `¿Estás seguro de que quieres eliminar ${item.productName} del pedido?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, Eliminar',
            cancelButtonText: 'Cancelar'
        });
        
        if (!result.isConfirmed) return;
    }
    
    // Llamar al endpoint para actualizar cantidad
    const response = await fetch('/Order/UpdateItemQuantityInOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            orderId: currentOrder.orderId,
            productId: item.productId,
            quantity: newQuantity,
            status: item.status
        })
    });
    
    // Manejar eliminación completa si cantidad es 0
    if (result.orderDeleted) {
        await clearOrder();
        Swal.fire('Orden Eliminada', 'La orden quedó vacía y fue eliminada', 'info');
        return;
    }
    
    // Actualizar UI y mostrar notificación
}
```

### **4. Estilos CSS Implementados**

#### **Controles de Cantidad:**
```css
.quantity-controls {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
}

.quantity-controls .btn {
    width: 32px;
    height: 32px;
    padding: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    font-size: 0.8rem;
    transition: all 0.2s ease;
}

.quantity-controls .btn:hover {
    transform: scale(1.1);
    box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}

.quantity-controls .btn:active {
    transform: scale(0.95);
}
```

#### **Responsive Design:**
```css
@media (max-width: 768px) {
    .quantity-controls {
        gap: 0.25rem;
    }
    
    .quantity-controls .btn {
        width: 28px;
        height: 28px;
        font-size: 0.7rem;
    }
    
    .quantity-controls .fw-bold {
        font-size: 1rem;
        min-width: 25px;
    }
}
```

## 🔄 **FLUJO DE FUNCIONAMIENTO**

### **1. Renderizado de Items**
```javascript
renderItemRow() → Verificar condiciones → Mostrar controles apropiados
```

### **2. Aumentar Cantidad**
```javascript
increaseQuantity() → Verificar condiciones → Llamar API → Actualizar UI → Notificar
```

### **3. Disminuir Cantidad**
```javascript
decreaseQuantity() → Verificar condiciones → Confirmar eliminación → Llamar API → Actualizar UI → Notificar
```

### **4. Eliminación Completa**
```javascript
decreaseQuantity() → Cantidad = 0 → API elimina item → Orden vacía → clearOrder()
```

## 📊 **ESTADOS Y COMPORTAMIENTOS**

| Estado Orden | Estado Item | Controles | Comportamiento |
|-------------|------------|-----------|----------------|
| `SentToKitchen` | `Pending` | ✅ + / - | Modificable |
| `SentToKitchen` | `Preparing` | ❌ Solo vista | No modificable |
| `SentToKitchen` | `Ready` | ❌ Solo vista | No modificable |
| `Pending` | `Pending` | ❌ Solo vista | No modificable |
| `ReadyToPay` | `Pending` | ❌ Solo vista | No modificable |

## 🎛️ **VALIDACIONES IMPLEMENTADAS**

### **Frontend:**
- ✅ **Verificación de estado de orden**: Solo `SentToKitchen`
- ✅ **Verificación de estado de item**: Solo `Pending`
- ✅ **Confirmación de eliminación**: Si cantidad será 0
- ✅ **Manejo de errores**: Try-catch con notificaciones

### **Backend:**
- ✅ **Endpoint existente**: `/Order/UpdateItemQuantityInOrder`
- ✅ **Validaciones de negocio**: Implementadas en el servicio
- ✅ **Manejo de orden vacía**: Eliminación automática

## 🚀 **BENEFICIOS IMPLEMENTADOS**

### **1. Flexibilidad**
- ✅ **Agregar items**: Mientras la orden esté en `SentToKitchen`
- ✅ **Modificar cantidades**: De items `Pending` existentes
- ✅ **Eliminar items**: Con confirmación de seguridad

### **2. Experiencia de Usuario**
- ✅ **Controles intuitivos**: Botones + y - claros
- ✅ **Feedback visual**: Notificaciones de éxito/error
- ✅ **Confirmaciones**: Para acciones destructivas

### **3. Integridad de Datos**
- ✅ **Validaciones**: En frontend y backend
- ✅ **Estados consistentes**: Verificación de condiciones
- ✅ **Manejo de errores**: Recuperación graceful

## 🎯 **RESULTADO FINAL**

El sistema ahora permite:

- ✅ **Modificar cantidades** de items `Pending` en órdenes `SentToKitchen`
- ✅ **Agregar nuevos items** a órdenes existentes
- ✅ **Eliminar items** con confirmación de seguridad
- ✅ **Interfaz intuitiva** con controles visuales claros
- ✅ **Validaciones robustas** en frontend y backend

**Los controles de cantidad están completamente implementados y funcionales** ✅ 