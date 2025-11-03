# 📦 Guía de Configuración de Inventario - RestBar

## 📋 Índice

1. [Configuración Básica de Inventario](#1-configuración-básica-de-inventario)
2. [Configuración de Stock por Estación](#2-configuración-de-stock-por-estación)
3. [Monitoreo y Alertas](#3-monitoreo-y-alertas)
4. [Casos de Uso Comunes](#4-casos-de-uso-comunes)

---

## 1. Configuración Básica de Inventario

### Paso 1: Activar Control de Inventario en un Producto

1. **Navegar a Productos:**
   - Ir a **Productos** en el menú principal
   - O acceder directamente a `/Product/Index`

2. **Crear o Editar un Producto:**
   - Hacer clic en **"Nuevo Producto"** o **"Editar"** en un producto existente

3. **Configurar Inventario:**
   - En la sección **"Configuración de Inventario"** (tarjeta morada/azul)
   - Marcar el checkbox **"Controlar Inventario"**
   - Al marcar, aparecerán los campos de configuración

### Paso 2: Configurar Campos de Inventario

#### **Stock Disponible** 📦
- **Descripción:** Cantidad actual de producto en inventario
- **Ejemplo:** Si tienes 50 pizzas en stock, ingresa `50`
- **Unidad:** Puede ser en unidades, kg, litros, etc. (según el campo "Unidad")
- **Requerido:** Sí, cuando el control de inventario está activo
- **Valor inicial:** Configura la cantidad inicial al crear el producto

#### **Stock Mínimo** ⚠️
- **Descripción:** Nivel mínimo para recibir alertas
- **Ejemplo:** Si configuras `10`, recibirás alerta cuando el stock baje a 10 o menos
- **Opcional:** Puede dejarse vacío si no deseas alertas
- **Recomendación:** Configura entre 20-30% del stock normal

#### **Permitir Stock Negativo** ➖
- **Descripción:** Permite vender productos aunque no haya stock disponible
- **Cuándo activar:**
  - ✅ Productos que se preparan al momento (pizzas, hamburguesas)
  - ✅ Productos con reabastecimiento rápido
  - ✅ Cuando no quieres bloquear ventas por falta de stock
- **Cuándo NO activar:**
  - ❌ Productos perecederos limitados
  - ❌ Productos con inventario crítico
  - ❌ Cuando necesitas control estricto

### Ejemplo de Configuración Básica:

```
Producto: Pizza Margherita
├── Controlar Inventario: ✅ Activado
├── Stock Disponible: 100
├── Stock Mínimo: 20
└── Permitir Stock Negativo: ❌ Desactivado
```

**Resultado:**
- El sistema verificará stock antes de cada venta
- Reducirá automáticamente el stock al crear órdenes
- Mostrará alerta cuando el stock baje a 20 o menos
- No permitirá ventas si el stock es insuficiente

---

## 2. Configuración de Stock por Estación

### ¿Qué es Stock por Estación?

Permite asignar el mismo producto a diferentes estaciones (cocinas) con stock independiente en cada una.

**Ejemplo:**
- Pizza puede tener:
  - 50 unidades en "Cocina Principal"
  - 30 unidades en "Cocina Express"

### Paso 1: Acceder a Asignaciones de Stock

1. Desde **Productos**, hacer clic en el botón **"Asignaciones de Stock"** (botón morado)
2. O navegar directamente a `/ProductStockAssignment/Index`

### Paso 2: Crear Asignación de Stock por Estación

1. **Hacer clic en "Nueva Asignación"**

2. **Completar el formulario:**
   - **Producto:** Seleccionar el producto
   - **Estación:** Seleccionar la estación (cocina/bar/etc.)
   - **Stock:** Cantidad disponible en esta estación
   - **Stock Mínimo:** Nivel mínimo para esta estación (opcional)
   - **Prioridad:** Número mayor = mayor prioridad al asignar órdenes
     - Ejemplo: Cocina Principal = 5, Cocina Express = 3
   - **Estado:** Activa/Inactiva

3. **Guardar la asignación**

### Ejemplo de Configuración por Estación:

```
Producto: Pizza Margherita

Asignación 1: Cocina Principal
├── Stock: 50
├── Stock Mínimo: 10
├── Prioridad: 5
└── Estado: Activa

Asignación 2: Cocina Express
├── Stock: 30
├── Stock Mínimo: 5
├── Prioridad: 3
└── Estado: Activa
```

**Resultado:**
- Al crear una orden, el sistema buscará la mejor estación
- Considerará prioridad y stock disponible
- Si Cocina Principal tiene stock y prioridad mayor, se asignará allí
- Cada estación reduce su propio stock independientemente

---

## 3. Monitoreo y Alertas

### Ver Alertas de Stock Bajo

1. **Acceder a Reportes de Inventario:**
   - Desde **Productos**, hacer clic en **"Reportes de Inventario"** (botón rosa)
   - O navegar a `/Inventory/Index`

2. **Ver sección "Productos con Stock Bajo":**
   - Se muestra automáticamente al cargar la página
   - Indica productos con `Stock <= MinStock`
   - Diferenciación visual:
     - 🟡 **Amarillo:** Stock bajo (entre MinStock y 50% de MinStock)
     - 🔴 **Rojo:** Stock crítico (menos del 50% de MinStock)

### Alertas Automáticas en Órdenes

- Al cargar productos en `/Order/Index`, se muestran alertas automáticamente
- Los productos con stock bajo se marcan visualmente
- Se puede ver el stock disponible en cada tarjeta de producto

---

## 4. Casos de Uso Comunes

### Caso 1: Producto con Stock Único (Sin Estaciones)

**Configuración:**
1. En el producto, activar "Controlar Inventario"
2. Configurar Stock Disponible (ej: 100)
3. Configurar Stock Mínimo (ej: 20)
4. NO crear asignaciones por estación

**Resultado:**
- Stock global compartido
- Se reduce del stock global al crear órdenes
- Se restaura al cancelar órdenes

### Caso 2: Producto con Stock por Múltiples Estaciones

**Configuración:**
1. En el producto, activar "Controlar Inventario"
2. NO configurar Stock Disponible (o dejarlo en 0)
3. Crear asignaciones por estación con stock específico

**Resultado:**
- Cada estación tiene su propio stock
- El sistema asigna automáticamente a la mejor estación
- Se reduce el stock de la estación asignada

### Caso 3: Producto Preparado al Momento (Stock Negativo Permitido)

**Configuración:**
1. Activar "Controlar Inventario"
2. Stock Disponible: 0 o cantidad pequeña
3. **Activar "Permitir Stock Negativo"**

**Resultado:**
- Se permite vender aunque no haya stock
- El stock puede quedar negativo
- Útil para productos que se preparan al momento

### Caso 4: Producto Perecedero (Control Estricto)

**Configuración:**
1. Activar "Controlar Inventario"
2. Stock Disponible: Cantidad limitada (ej: 10)
3. Stock Mínimo: Nivel bajo (ej: 2)
4. **NO activar "Permitir Stock Negativo"**

**Resultado:**
- Control estricto de inventario
- No permite ventas sin stock
- Alertas tempranas de stock bajo

---

## 📊 Flujo de Trabajo Completo

### 1. Configurar Producto Nuevo

```
1. Crear Producto
   ↓
2. Activar "Controlar Inventario"
   ↓
3. Configurar Stock Disponible
   ↓
4. Configurar Stock Mínimo (opcional)
   ↓
5. Decidir si permitir stock negativo
   ↓
6. Guardar Producto
```

### 2. Configurar Stock por Estación (Opcional)

```
1. Ir a "Asignaciones de Stock"
   ↓
2. Crear asignación para Estación A
   ├── Stock: cantidad
   ├── Prioridad: número
   └── Guardar
   ↓
3. Crear asignación para Estación B
   ├── Stock: cantidad
   ├── Prioridad: número
   └── Guardar
```

### 3. Monitorear Inventario

```
1. Ver alertas en "/Inventory/Index"
   ↓
2. Revisar productos con stock bajo
   ↓
3. Reabastecer según necesidad
   ↓
4. Actualizar stock en producto o asignaciones
```

---

## 🔧 Actualización de Stock

### Actualizar Stock Global

1. **Desde Productos:**
   - Editar el producto
   - Modificar el campo "Stock Disponible"
   - Guardar cambios

### Actualizar Stock por Estación

1. **Desde Asignaciones de Stock:**
   - Editar la asignación
   - Modificar el campo "Stock"
   - Guardar cambios

---

## ⚠️ Consideraciones Importantes

### 1. Prioridad de Stock

El sistema busca stock en este orden:
1. Stock por estación (si existe asignación activa)
2. Stock global del producto
3. Permite stock negativo (si está habilitado)

### 2. Reducción Automática

- El stock se reduce **automáticamente** al crear una orden
- Se reduce del stock de la estación asignada (si existe)
- O del stock global (si no hay asignación por estación)

### 3. Restauración Automática

- El stock se restaura **automáticamente** al cancelar una orden
- Se restaura en la misma estación donde se redujo
- O en el stock global si no había estación asignada

### 4. Verificación Antes de Venta

- El sistema verifica stock **antes** de agregar productos a una orden
- Muestra alerta si no hay stock suficiente
- Permite o bloquea según configuración

---

## 📱 Accesos Rápidos

- **Productos:** `/Product/Index`
- **Asignaciones de Stock:** `/ProductStockAssignment/Index`
- **Reportes de Inventario:** `/Inventory/Index`

---

## 💡 Tips y Recomendaciones

1. **Stock Mínimo:**
   - Configura entre 20-30% del stock normal
   - Ajusta según frecuencia de reabastecimiento

2. **Prioridades por Estación:**
   - Usa números mayores para estaciones principales (5, 10)
   - Usa números menores para estaciones secundarias (1, 2, 3)

3. **Stock Negativo:**
   - Úsalo solo para productos que se preparan al momento
   - Evítalo para productos perecederos o de alto costo

4. **Monitoreo Regular:**
   - Revisa alertas diariamente
   - Actualiza stock después de reabastecimientos
   - Usa reportes de consumo para planificar compras

---

## ✅ Checklist de Configuración

Para cada producto nuevo:

- [ ] ¿Necesita control de inventario?
- [ ] Si sí, activar "Controlar Inventario"
- [ ] Configurar Stock Disponible inicial
- [ ] Configurar Stock Mínimo (opcional pero recomendado)
- [ ] Decidir si permitir stock negativo
- [ ] Si usa múltiples estaciones, crear asignaciones
- [ ] Configurar prioridades por estación
- [ ] Verificar que las asignaciones estén activas

---

## 📞 Soporte

Si tienes dudas sobre la configuración:
1. Revisa esta guía
2. Consulta los logs en la consola del navegador
3. Revisa los logs del servidor para mensajes de error

**Funcionalidades disponibles:**
- ✅ Stock global por producto
- ✅ Stock por estación independiente
- ✅ Prioridades de asignación
- ✅ Alertas de stock bajo
- ✅ Verificación antes de venta
- ✅ Reducción/restauración automática
- ✅ Reportes de consumo
