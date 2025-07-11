// Order Management

let currentOrder = { items: [], total: 0, tableId: null };

// Variables para el CRUD
let isEditMode = false;
let originalItems = [];
let modifiedItems = [];
let newItems = [];
let deletedItems = [];

// Modificar la función handleTableClick para incluir SignalR
async function handleTableClick(tableId, tableNumber, status) {
    console.log('[Frontend] handleTableClick iniciado');
    console.log('[Frontend] tableId recibido:', tableId);
    console.log('[Frontend] tableId type:', typeof tableId);
    console.log('[Frontend] tableNumber:', tableNumber);
    console.log('[Frontend] status:', status);
    
    // Asignar el tableId recibido a currentOrder antes de cualquier proceso
    if (!currentOrder) currentOrder = {};
    currentOrder.tableId = tableId;
    
    console.log('[Frontend] currentOrder.tableId asignado:', currentOrder.tableId);
    
    // Siempre intentar cargar la orden existente primero
    const existingOrder = await loadExistingOrder(tableId);
    
    // Unirse a grupos de SignalR
    await joinSignalRGroups(tableId, currentOrder.orderId);
    
    // Si hay una orden existente, refrescar usando forceRefreshOrder para asegurar agrupación correcta
    if (existingOrder && existingOrder.hasActiveOrder) {
        await forceRefreshOrder();
        // Mostrar opciones como antes (opcional)
        let messageTitle = 'Orden Existente Encontrada';
        let messageText = 'Esta mesa tiene una orden activa. ¿Qué deseas hacer?';
        let confirmButtonText = 'Agregar a orden existente';
        const allItemsReady = currentOrder.items.every(item => item.status === 'Ready');
        const hasReadyItems = currentOrder.items.some(item => item.status === 'Ready');
        if (allItemsReady) {
            messageTitle = 'Orden Completa - Items Listos';
            messageText = 'Todos los items de esta orden están listos. Puedes agregar más productos a la misma orden o crear una nueva.';
            confirmButtonText = 'Agregar más items';
        } else if (hasReadyItems) {
            messageTitle = 'Orden en Progreso';
            messageText = 'Algunos items están listos y otros en preparación. Puedes agregar más productos a la misma orden.';
            confirmButtonText = 'Agregar más items';
        }
        const result = await Swal.fire({
            title: messageTitle,
            html: showOrderSummary(currentOrder),
            icon: 'info',
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonText: confirmButtonText,
            denyButtonText: 'Nueva orden',
            cancelButtonText: 'Cancelar',
            width: '600px',
            customClass: {
                confirmButton: 'btn btn-primary',
                denyButton: 'btn btn-success',
                cancelButton: 'btn btn-secondary'
            }
        });
        if (result.isConfirmed) {
            await Swal.fire({
                title: 'Orden Cargada',
                text: 'Puedes agregar más productos a la orden existente. Los items listos se mantendrán.',
                icon: 'success',
                timer: 2000,
                showConfirmButton: false
            });
        } else if (result.isDenied) {
            await startNewOrder(tableId, tableNumber);
        }
    } else {
        await startNewOrder(tableId, tableNumber);
    }
}

async function loadExistingOrder(tableId) {
    try {
        console.log('[Frontend] loadExistingOrder iniciado - tableId:', tableId);
        
        const response = await fetch(`/Order/GetActiveOrder?tableId=${tableId}`);
        if (response.ok) {
            const result = await response.json();
            console.log('[Frontend] Resultado completo de GetActiveOrder:', result);
            console.log('[Frontend] result.orderId:', result.orderId);
            console.log('[Frontend] result.hasActiveOrder:', result.hasActiveOrder);
            
            if (result.hasActiveOrder) {
                console.log('[Frontend] Items recibidos del servidor:', result.items);
                
                // ✅ LOGGING DETALLADO DE ITEMS RECIBIDOS
                console.log('[Frontend] === DETALLE DE ITEMS RECIBIDOS DEL SERVIDOR ===');
                if (result.items) {
                    result.items.forEach((item, index) => {
                        console.log(`[Frontend] Item ${index + 1}:`, {
                            id: item.id,
                            productId: item.productId,
                            quantity: item.quantity,
                            status: item.status,
                            isGuid: item.id && item.id.length === 36
                        });
                    });
                }
                console.log('[Frontend] === FIN DETALLE DE ITEMS ===');
                
                // Hay una orden activa, cargarla
                currentOrder = {
                    orderId: result.orderId || null,
                    tableId: tableId,
                    items: result.items.map(item => ({
                        id: item.id,
                        productId: item.productId,
                        productName: item.productName,
                        price: item.unitPrice,
                        quantity: item.quantity,
                        status: item.status,
                        kitchenStatus: item.kitchenStatus,
                        preparedAt: item.preparedAt,
                        preparedByStation: item.preparedByStation,
                        notes: item.notes,
                        isFromBackend: true // ✅ PARÁMETRO PARA IDENTIFICAR ITEMS DEL BACKEND
                    })),
                    total: result.totalAmount || 0,
                    status: result.status || null
                };
                
                console.log('[Frontend] currentOrder configurado:', currentOrder);
                console.log('[Frontend] currentOrder.orderId:', currentOrder.orderId);
                console.log('[Frontend] Items en currentOrder:', currentOrder.items);
                
                updateOrderUI();
                highlightSelectedTable(tableId);
                enableConfirmButton();
                
                // Cambiar el texto del botón según el estado
                const sendButton = document.getElementById('sendToKitchen');
                console.log('[Frontend] Estado de orden recibido:', result.status);
                
                if (result.status === 'SentToKitchen' || result.status === 'Preparing') {
                    sendButton.textContent = 'Agregar a Cocina';
                    console.log('[Frontend] ✅ Botón configurado como "Agregar a Cocina" para orden en SentToKitchen');
                } else {
                    sendButton.textContent = 'Enviar a Cocina';
                    console.log('[Frontend] Botón configurado como "Enviar a Cocina" para orden en estado:', result.status);
                }
                
                // Verificar que el estado sea el esperado
                if (result.status === 'SentToKitchen') {
                    console.log('[Frontend] ✅ Orden correctamente en estado SentToKitchen');
                } else {
                    console.log('[Frontend] ⚠️ Orden en estado inesperado:', result.status);
                }
                
                // Verificar y actualizar el estado de la mesa si es necesario
                // Nota: Esta funcionalidad se maneja automáticamente vía SignalR
                
                console.log('[Frontend] Orden existente cargada exitosamente');
                
                // Unirse al grupo de SignalR para esta orden
                if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
                    await signalRConnection.invoke("JoinOrderGroup", currentOrder.orderId);
                    console.log('[Frontend] Unido al grupo de SignalR para la orden:', currentOrder.orderId);
                }
                
                // Actualizar información de pagos
                if (typeof updatePaymentInfo === 'function') {
                    await updatePaymentInfo();
                }
                
                return result;
            } else {
                console.log('[Frontend] No hay orden activa');
                return null;
            }
        } else {
            throw new Error('Error al obtener orden activa');
        }
    } catch (error) {
        console.error('[Frontend] Error al cargar orden existente:', error);
        Swal.fire('Error', 'No se pudo cargar la orden existente', 'error');
        return null;
    }
}

async function startNewOrder(tableId, tableNumber) {
    console.log('[Frontend] startNewOrder iniciado');
    console.log('[Frontend] tableId recibido:', tableId);
    console.log('[Frontend] tableNumber recibido:', tableNumber);
    
    currentOrder = { 
        items: [], 
        total: 0, 
        tableId: tableId, 
        orderId: null, 
        status: null 
    };
    
    console.log('[Frontend] currentOrder configurado:', currentOrder);
    console.log('[Frontend] currentOrder.tableId después de configurar:', currentOrder.tableId);
    
    updateOrderUI();
    highlightSelectedTable(tableId);
    enableConfirmButton();
    
    // Resetear el texto del botón
    const sendButton = document.getElementById('sendToKitchen');
    sendButton.textContent = 'Confirmar Pedido';
    
    // Limpiar información de pagos para nueva orden
    if (typeof updatePaymentInfo === 'function') {
        await updatePaymentInfo();
    }
    
    console.log('[Frontend] Nueva orden iniciada:', currentOrder);
}

// Función para forzar la actualización de la orden
async function forceRefreshOrder() {
    if (currentOrder && currentOrder.orderId) {
        console.log('[Frontend] Forzando actualización de orden:', currentOrder.orderId);
        await refreshOrderStatus(currentOrder.orderId);
    } else {
        console.log('[Frontend] No hay orden activa para actualizar');
        Swal.fire('Info', 'No hay una orden activa para actualizar', 'info');
    }
}

// Refrescar estado de la orden desde el servidor
async function refreshOrderStatus(orderId) {
    try {
        console.log('[Frontend] refreshOrderStatus iniciado para orderId:', orderId);
        
        const response = await fetch(`/Order/GetOrderStatus/${orderId}`);
        console.log('[Frontend] Respuesta del servidor - Status:', response.status);
        
        if (response.ok) {
            const result = await response.json();
            console.log('[Frontend] Datos completos recibidos del servidor:', result);
            
            if (result.success) {
                console.log('[Frontend] Items recibidos del servidor:', result.items);
                
                // Actualizar la orden actual con los datos del servidor
                currentOrder.orderId = result.orderId;
                currentOrder.status = result.status;
                currentOrder.total = result.totalAmount;
                currentOrder.items = result.items.map(item => ({
                    id: item.id,
                    productId: item.productId,
                    productName: item.productName,
                    price: item.unitPrice,
                    quantity: item.quantity,
                    status: item.status,
                    kitchenStatus: item.kitchenStatus,
                    preparedAt: item.preparedAt,
                    preparedByStation: item.preparedByStation,
                    notes: item.notes || '',
                    isFromBackend: true // ✅ PARÁMETRO PARA IDENTIFICAR ITEMS DEL BACKEND
                }));
                
                console.log('[Frontend] currentOrder actualizado:', currentOrder);
                console.log('[Frontend] Items en currentOrder después de actualizar:', currentOrder.items);
                
                // Actualizar la UI con los nuevos datos
                updateOrderUI();
                
                // Actualizar información de pagos
                if (typeof updatePaymentInfo === 'function') {
                    await updatePaymentInfo();
                }
                
                // Mostrar notificación de actualización
                Swal.fire({
                    title: 'Orden Actualizada',
                    text: 'Se ha actualizado el estado de la orden desde el servidor',
                    icon: 'success',
                    timer: 2000,
                    showConfirmButton: false
                });
                
                console.log('[Frontend] refreshOrderStatus completado exitosamente');
            } else {
                console.error('[Frontend] Error en respuesta del servidor:', result.error);
                Swal.fire('Error', 'Error al obtener datos del servidor: ' + result.error, 'error');
            }
        } else if (response.status === 404) {
            // Orden no existe, limpiar UI
            console.warn('[Frontend] La orden ya no existe en el servidor. Limpiando UI.');
            currentOrder = { items: [], total: 0, tableId: null, orderId: null, status: null };
            updateOrderUI();
            Swal.fire({
                title: 'Orden eliminada',
                text: 'La orden ha sido eliminada o cancelada',
                icon: 'info',
                timer: 2500,
                showConfirmButton: false
            });
        } else {
            console.error('[Frontend] Error HTTP al obtener estado de orden:', response.status);
            const errorText = await response.text();
            console.error('[Frontend] Error text:', errorText);
            Swal.fire('Error', 'Error HTTP: ' + response.status, 'error');
        }
    } catch (error) {
        console.error('[Frontend] Error al refrescar estado de orden:', error);
        Swal.fire('Error', 'Error al refrescar orden: ' + error.message, 'error');
    }
}

// Actualizar estado de la orden
function updateOrderStatus(newStatus) {
    if (currentOrder) {
        currentOrder.status = newStatus;
        updateOrderUI();
        
        // Mostrar notificación
        Swal.fire({
            title: 'Estado de Orden Actualizado',
            text: `La orden cambió a: ${getStatusDisplayText(newStatus)}`,
            icon: 'info',
            timer: 3000,
            showConfirmButton: false
        });
    }
}

// Actualizar estado de un item específico
function updateOrderItemStatus(itemId, newStatus) {
    if (currentOrder && currentOrder.items) {
        const item = currentOrder.items.find(i => i.id === itemId);
        if (item) {
            item.status = newStatus;
            updateOrderUI();
            
            // Mostrar notificación
            Swal.fire({
                title: 'Item Actualizado',
                text: `${item.productName} está ahora: ${getStatusDisplayText(newStatus)}`,
                icon: 'success',
                timer: 3000,
                showConfirmButton: false
            });
        }
    }
}

// Manejar orden cancelada
function handleOrderCancelled() {
    Swal.fire({
        title: 'Orden Cancelada',
        text: 'La orden ha sido cancelada',
        icon: 'warning',
        confirmButtonText: 'OK'
    }).then(() => {
        // Limpiar la orden actual
        currentOrder = { items: [], total: 0, tableId: null };
        updateOrderUI();
        clearOrderUI();
    });
}

// Función para limpiar solo los items nuevos sin afectar la orden existente
function clearNewItemsOnly() {
    console.log('[Frontend] clearNewItemsOnly iniciado');
    console.log('[Frontend] currentOrder antes de limpiar items nuevos:', currentOrder);
    
    // Preservar orderId, status y tableId
    const orderId = currentOrder.orderId;
    const status = currentOrder.status;
    const tableId = currentOrder.tableId;
    
    // Mantener solo los items que NO son Pending (items existentes)
    const existingItems = currentOrder.items.filter(item => item.status !== 'Pending');
    
    // Recalcular el total solo con los items existentes
    const total = existingItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    
    // Actualizar currentOrder manteniendo solo los items existentes
    currentOrder = { 
        items: existingItems, 
        total: total, 
        tableId: tableId,
        orderId: orderId,
        status: status
    };
    
    console.log('[Frontend] currentOrder después de limpiar items nuevos:', currentOrder);
    console.log('[Frontend] Items preservados:', existingItems.length);
    
    // Limpiar controles de cantidad
    document.querySelectorAll('.quantity').forEach(q => {
        q.textContent = '0';
        q.style.display = 'none';
    });
    
    // Ocultar botones de incremento/decremento
    document.querySelectorAll('[id^="decrease-"]').forEach(btn => {
        btn.style.display = 'none';
    });
    document.querySelectorAll('[id^="increase-"]').forEach(btn => {
        btn.style.display = 'none';
    });
    
    // Limpiar tarjetas de productos
    document.querySelectorAll('.product-card').forEach(card => {
        card.classList.remove('selected-product');
        card.style.backgroundColor = '';
        card.style.border = '';
        card.style.transform = '';
        card.style.boxShadow = '';
    });
    
    // Actualizar UI de la orden
    updateOrderUI();
    
    // Mantener el botón habilitado si hay una orden activa
    if (currentOrder.orderId) {
        enableConfirmButton();
        const sendButton = document.getElementById('sendToKitchen');
        if (currentOrder.status === 'SentToKitchen' || currentOrder.status === 'Preparing' || currentOrder.status === 'Ready') {
            sendButton.textContent = 'Agregar a Cocina';
        } else {
            sendButton.textContent = 'Enviar a Cocina';
        }
    } else {
        disableConfirmButton();
    }
    
    console.log('[Frontend] clearNewItemsOnly completado exitosamente');
}

// Vaciar orden
async function clearOrder() {
    console.log('[Frontend] clearOrder iniciado');
    console.log('[Frontend] currentOrder:', currentOrder);
    
    const result = await Swal.fire({
        title: '¿Vaciar orden?',
        text: '¿Estás seguro de que deseas vaciar toda la orden?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, vaciar',
        cancelButtonText: 'Cancelar'
    });

    console.log('[Frontend] Resultado de confirmación clearOrder:', result);
    if (result.isConfirmed) {
        console.log('[Frontend] Usuario confirmó vaciar orden');
        
        try {
            // Simplemente limpiar la interfaz sin cancelar la orden en el backend
            console.log('[Frontend] Llamando limpiarUIYEstadoLocal...');
            limpiarUIYEstadoLocal();
            
            console.log('[Frontend] Mostrando mensaje de éxito...');
            await Swal.fire({
                title: 'Orden Vaciada',
                text: 'La orden ha sido vaciada de la interfaz',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            });
            
            console.log('[Frontend] clearOrder completado exitosamente');
        } catch (error) {
            console.error('[Frontend] Error en clearOrder:', error);
            console.error('[Frontend] Error name:', error.name);
            console.error('[Frontend] Error message:', error.message);
            console.error('[Frontend] Error stack:', error.stack);
            
            Swal.fire('Error', 'No se pudo vaciar la orden', 'error');
        }
    } else {
        console.log('[Frontend] Usuario canceló la operación de vaciar');
    }
}

function limpiarUIYEstadoLocal() {
    console.log('[Frontend] limpiarUIYEstadoLocal iniciado');
    console.log('[Frontend] currentOrder antes de limpiar:', currentOrder);
    
    // Preservar orderId y status si existe una orden activa
    const orderId = currentOrder.orderId;
    const status = currentOrder.status;
    const tableId = currentOrder.tableId;
    
    console.log('[Frontend] Valores preservados:');
    console.log('[Frontend]   - orderId:', orderId);
    console.log('[Frontend]   - status:', status);
    console.log('[Frontend]   - tableId:', tableId);
    
    // Limpiar solo los items y total
    currentOrder = { 
        items: [], 
        total: 0, 
        tableId: tableId,
        orderId: orderId,
        status: status
    };
    
    console.log('[Frontend] currentOrder después de limpiar:', currentOrder);
    
    try {
        console.log('[Frontend] Limpiando selección de mesas...');
        // Limpiar selección de mesas
        document.querySelectorAll('.mesa-btn').forEach(btn => btn.classList.remove('active'));
        
        console.log('[Frontend] Limpiando tarjetas de productos...');
        // Limpiar tarjetas de productos
        document.querySelectorAll('.product-card').forEach(card => {
            card.classList.remove('selected-product');
            card.style.backgroundColor = '';
            card.style.border = '';
            card.style.transform = '';
            card.style.boxShadow = '';
        });
        
        console.log('[Frontend] Limpiando controles de cantidad...');
        // Limpiar controles de cantidad
        document.querySelectorAll('.quantity').forEach(q => {
            q.textContent = '0';
            q.style.display = 'none';
        });
        
        console.log('[Frontend] Ocultando botones de incremento/decremento...');
        // Ocultar botones de incremento/decremento
        document.querySelectorAll('[id^="decrease-"]').forEach(btn => {
            btn.style.display = 'none';
        });
        document.querySelectorAll('[id^="increase-"]').forEach(btn => {
            btn.style.display = 'none';
        });
        
        console.log('[Frontend] Actualizando UI de la orden...');
        // Actualizar UI de la orden
        updateOrderUI();
        
        console.log('[Frontend] Deshabilitando botón de confirmar...');
        disableConfirmButton();
        
        console.log('[Frontend] Reseteando texto del botón...');
        // Resetear texto del botón
        const sendButton = document.getElementById('sendToKitchen');
        sendButton.textContent = 'Confirmar Pedido';
        
        console.log('[Frontend] limpiarUIYEstadoLocal completado exitosamente');
        console.log('[Frontend] Orden limpiada completamente:', currentOrder);
    } catch (error) {
        console.error('[Frontend] Error en limpiarUIYEstadoLocal:', error);
        console.error('[Frontend] Error name:', error.name);
        console.error('[Frontend] Error message:', error.message);
        console.error('[Frontend] Error stack:', error.stack);
        throw error;
    }
}

window.currentOrder = currentOrder; 