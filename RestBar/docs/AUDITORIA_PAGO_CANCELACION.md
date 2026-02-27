# Auditoría: Pago y Cancelación de Órdenes — RestBar

## Resumen ejecutivo

**Problemas reportados:** No se puede pagar un pedido. No se puede cancelar un pedido.

**Causas raíz identificadas y corregidas:**

| # | Causa raíz | Archivo / Ubicación | Criticidad | Estado |
|---|------------|---------------------|------------|--------|
| 1 | La API de pago devolvía `Ok(payment)` sin `success: true`. El frontend comprueba `result.success` y al ser `undefined` siempre mostraba error aunque el pago se hubiera registrado. | `PaymentController.CreatePartialPayment` respuesta | **Crítica** | ✅ Corregido |
| 2 | No había validación de estado de la orden: se permitía intentar pagar órdenes canceladas o ya completadas. | `PaymentController` inicio del flujo | Alta | ✅ Corregido |
| 3 | El total de la orden incluía ítems cancelados; saldo pendiente y validaciones de monto podían ser incorrectos. | `PaymentController` cálculo de `orderTotal` / `remainingAmount` | Alta | ✅ Corregido |
| 4 | Flujo de pago sin transacción: `PaymentService.CreateAsync` hacía `SaveChanges` y luego el controlador actualizaba orden/mesa con otro `SaveChanges`. Fallo entre ambos dejaba pagos registrados sin actualizar estado de la orden. | `PaymentController` + `PaymentService` | **Crítica** | ✅ Corregido |
| 5 | Cancelación sin validación de estado: se podía “cancelar” una orden ya completada o ya cancelada. | `OrderService.CancelOrderAsync` | Media | ✅ Corregido |
| 6 | Cancelación sin transacción: anulación de pagos, cambio de estado, log y mesa en varios pasos; fallo intermedio podía dejar datos inconsistentes. | `OrderService.CancelOrderAsync` | **Crítica** | ✅ Corregido |
| 7 | En cancelación, el log de productos usaba `OrderItems` sin `Include(Product)`, generando "Producto desconocido" para todos los ítems. | `OrderService.CancelOrderAsync` carga de orden | Baja | ✅ Corregido |
| 8 | Errores de cancelación/pago no se devolvían como JSON consistente y el frontend no mostraba el mensaje del servidor. | `OrderController.Cancel`, `PaymentController`, `order-operations.js`, `payments.js` | Media | ✅ Corregido |

---

## Simulación paso a paso (flujo humano)

### Flujo: Crear orden → Enviar a cocina → Pagar

1. Usuario crea orden, agrega productos, envía a cocina.
2. Orden pasa a `SentToKitchen`; ítems a `Pending`/`Preparing`.
3. Usuario abre modal de pago y envía pago total.
4. **Antes:** Backend guardaba el pago y actualizaba la orden, pero respondía `Ok(payment)` sin `success`. El JS hacía `if (result.success)` → `undefined` → siempre entraba al `else` y mostraba "Error al procesar el pago". El pago sí existía en BD; la percepción era "no puedo pagar".
5. **Ahora:** La API devuelve `{ success: true, isFullyPaid, message, payment }`. El frontend muestra éxito y limpia la orden cuando corresponde.

### Flujo: Intentar cancelar

1. Usuario pide cancelar la orden.
2. **Antes:** Si la orden no existía, ya estaba cancelada o ya estaba completada, el backend lanzaba excepción y el controlador devolvía `BadRequest({ error: ex.Message })`. El frontend con `!response.ok` mostraba un mensaje genérico y no siempre el cuerpo de error.
3. **Ahora:** Se valida estado (no cancelar si ya está `Cancelled` o `Completed`), se devuelve JSON con `success` y `message`, y el frontend muestra `err.message` cuando viene en la respuesta.

---

## Cambios de código realizados

### 1. PaymentController (CreatePartialPayment)

- Validación de entrada: `request` no nulo, `OrderId` no vacío, `Amount > 0`.
- Reglas de negocio: rechazar pago si `order.Status == Cancelled` o `order.Status == Completed`.
- Cálculo de total a pagar solo con ítems no cancelados:  
  `payableItems = order.OrderItems.Where(oi => oi.Status != OrderItemStatus.Cancelled)`,  
  `orderTotal = payableItems.Sum(...)`, `remainingAmount = orderTotal - totalPaid`.
- Rechazo si `remainingAmount <= 0` o si `request.Amount > remainingAmount`.
- **Transacción única:** `using (var transaction = await _context.Database.BeginTransactionAsync())`:
  - `_context.Payments.Add(payment)` y `_context.SplitPayments.Add(...)` para splits.
  - Cálculo de `isFullyPaid` y actualización de estado de orden e ítems y mesa.
  - Un solo `SaveChangesAsync()` y `transaction.CommitAsync()`; en caso de excepción `RollbackAsync()`.
- Notificaciones SignalR y envío de email **fuera** de la transacción.
- Respuesta exitosa: `Ok(new { success = true, isFullyPaid, message, payment = responseDto })`.
- Respuestas de error en JSON: `BadRequest(new { success = false, message })`, `StatusCode(500, new { success = false, message })`.

### 2. OrderService.CancelOrderAsync

- Validación: `orderId != Guid.Empty`; orden debe existir; si `Status == Cancelled` o `Status == Completed` se lanza `InvalidOperationException` con mensaje claro.
- Carga de orden con `Include(OrderItems).ThenInclude(Product)` para el log de cancelación.
- **Transacción única:** anular pagos, marcar orden como `Cancelled`, `ClosedAt`, insertar `OrderCancellationLog` (con nombres de productos), actualizar mesa si no hay otras órdenes activas, `SaveChangesAsync()`, `CommitAsync()`; en caso de error `RollbackAsync()`.
- Restauración de inventario **después** del commit (compensación); fallos de restauración se registran pero no revierten la cancelación.

### 3. OrderController.Cancel

- Validación de `dto` y `OrderId`.
- Excepciones mapeadas a HTTP y JSON:  
  `KeyNotFoundException` → `NotFound`,  
  `InvalidOperationException` / `ArgumentException` → `BadRequest`,  
  resto → `StatusCode(500)`.  
  Todas con `{ success = false, message = ex.Message }`.

### 4. Concurrencia (Order.Version + 409)

- **Order:** Propiedad `Version` (int) configurada como token de concurrencia en `RestBarContext` (`IsConcurrencyToken()`, columna `version`).
- **Migración:** `AddOrderVersionConcurrencyToken` añade la columna `version` (integer, default 0) a la tabla `orders`. Aplicar con `dotnet ef database update`.
- **Pago:** Antes de `SaveChangesAsync()` se hace `order.Version++`. Si otro proceso modificó la orden, EF lanza `DbUpdateConcurrencyException`; el controlador responde **409** con `{ success = false, message = "La orden fue modificada por otro usuario. Actualice e intente de nuevo." }`.
- **Cancelación:** En `CancelOrderAsync` se hace `order.Version++` antes de guardar. En `OrderController.Cancel` se captura `DbUpdateConcurrencyException` y se devuelve **409** con el mismo mensaje.

### 5. Frontend

- **payments.js:** En respuestas no OK, se hace `response.json()` y se muestra `errorData.message` si existe; si no, mensaje genérico. Variable `paymentProcessing` para evitar doble envío.
- **order-operations.js:** Igual para cancelación: en `!response.ok` se intenta leer JSON y mostrar `err.message`. Variable `cancelProcessing` para evitar doble cancelación.

---

## Validación de concurrencia (diseño)

- **Pago:** Una sola transacción por request. Dos requests simultáneos para la misma orden pueden intentar pagar; el segundo puede recibir "La orden ya está pagada por completo" o "El monto excede el saldo pendiente" según el orden de commit. No hay doble acreditación del mismo monto porque el total pagado se recalcula y se valida.
- **Cancelación:** Una transacción por cancelación. Si dos usuarios cancelan la misma orden, el segundo recibirá "La orden ya está cancelada".
- **Recomendación adicional:** Para alta concurrencia (muchas mesas/usuarios), valorar:
  - Concurrency token (p. ej. `RowVersion`) en `Order` para detectar modificaciones simultáneas.
  - Reintentos con backoff en el cliente ante 409 Conflict si se implementa versión optimista.

---

## Recomendaciones de hardening

1. **Idempotencia de pago:** Para evitar doble pago por doble clic o reintentos, considerar un `Idempotency-Key` en el header del POST de pago y guardar en caché/BD; si la clave se repite, devolver el mismo resultado sin reejecutar.
2. **RowVersion en Order:** Añadir `[Timestamp] public byte[] RowVersion { get; set; }` y en `CancelOrderAsync` / flujo de pago manejar `DbUpdateConcurrencyException` y devolver 409 con mensaje claro.
3. **Política de cancelación:** Si el negocio exige “no cancelar si hay ítems en preparación”, añadir en `CancelOrderAsync` una comprobación sobre `OrderItems` (p. ej. ningún ítem en `Preparing`) y lanzar `InvalidOperationException` con mensaje explícito.
4. **Pruebas unitarias:** Añadir tests para: (a) pago rechazado cuando orden está `Cancelled`/`Completed`, (b) total solo con ítems no cancelados, (c) cancelación rechazada cuando orden está `Completed` o ya `Cancelled`, (d) respuesta de pago con `success` y `isFullyPaid`.

---

## Nivel de riesgo residual

- **Pago:** Bajo. Flujo atómico, validaciones de estado y monto, y respuesta alineada con el frontend.
- **Cancelación:** Bajo. Transacción única, validación de estado y mensajes claros al usuario.
- **Concurrencia:** Medio. Sin RowVersion, dos usuarios modificando la misma orden a la vez pueden obtener comportamientos que se resuelven por validaciones (p. ej. “ya pagada” / “ya cancelada”), pero no se garantiza detección explícita de conflicto. Recomendable añadir concurrencia optimista en siguiente iteración.

---

## Checklist de cumplimiento

- Atomicidad: pago y cancelación en una transacción cada uno.
- Consistencia: total a pagar sin ítems cancelados; estados de orden/mesa coherentes.
- Aislamiento: transacciones estándar de EF Core.
- Durabilidad: commit explícito tras `SaveChangesAsync`.
- Idempotencia en pago: mejorable con Idempotency-Key (recomendado).
- Cancelación: solo permitida en estados no finales; mensajes claros al usuario.
