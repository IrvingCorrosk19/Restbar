// Order Management

let currentOrder = { items: [], total: 0, tableId: null, orderId: null, status: null };

// Incrementa en cada reset para ignorar refresh async obsoletos (race SignalR vs cancel)
let orderSummaryGeneration = 0;

/**
 * Limpia por completo el Resumen del Pedido y el estado local de la orden activa.
 * @param {{ keepTableId?: boolean, tableId?: string|null }} options
 */
function resetActiveOrderState(options = {}) {
    orderSummaryGeneration++;

    const keepTableId = options.keepTableId === true;
    const preservedTableId = keepTableId
        ? (options.tableId ?? currentOrder?.tableId ?? null)
        : null;

    currentOrder = {
        items: [],
        total: 0,
        tableId: preservedTableId,
        orderId: null,
        status: null,
        discount: 0
    };

    if (typeof initializeDiscount === 'function') {
        initializeDiscount();
    }

    if (typeof clearPaymentSummary === 'function') {
        clearPaymentSummary();
    }

    updateOrderUI();
    clearOrderUI();

    document.querySelectorAll('.product-card').forEach(card => {
        card.classList.remove('selected-product');
        card.style.backgroundColor = '';
        card.style.border = '';
        card.style.transform = '';
        card.style.boxShadow = '';
    });

    document.querySelectorAll('.quantity').forEach(q => {
        q.textContent = '0';
        q.style.display = 'none';
    });
    document.querySelectorAll('[id^="decrease-"], [id^="increase-"]').forEach(btn => {
        btn.style.display = 'none';
    });

    const sendButton = document.getElementById('sendToKitchen');
    if (sendButton) {
        sendButton.textContent = 'Confirmar Pedido';
        sendButton.disabled = false;
    }

    const cancelOrderBtn = document.getElementById('cancelOrder');
    if (cancelOrderBtn) cancelOrderBtn.style.display = 'none';

    const separateAccountsBtn = document.getElementById('separateAccountsBtn');
    if (separateAccountsBtn) separateAccountsBtn.style.display = 'none';

    updatePaymentButtons();
}

function shouldApplyOrderRefresh(generationAtStart) {
    return generationAtStart === orderSummaryGeneration;
}

function isTerminalOrderStatus(status) {
    if (!status) return false;
    const s = String(status).toLowerCase();
    return s === 'cancelled' || s === 'completed';
}

// Variables para el CRUD
let isEditMode = false;
let originalItems = [];
let modifiedItems = [];
let newItems = [];
let deletedItems = [];

// ✅ NUEVO: Función para mostrar resumen de la orden
function showOrderSummary(order) {
    if (!order || !order.items || order.items.length === 0) {
        return '<p>No hay items en esta orden.</p>';
    }
    
    // ✅ NUEVO: Filtrar items cancelados del resumen
    const activeItems = order.items.filter(item => item.status !== 'cancelled' && item.status !== 'Cancelled');
    
    if (activeItems.length === 0) {
        return '<p>No hay items activos en esta orden (todos fueron cancelados).</p>';
    }
    
    let html = '<div class="order-summary">';
    html += '<h5>Resumen de la Orden</h5>';
    html += '<div class="table-responsive">';
    html += '<table class="table table-sm table-bordered">';
    html += '<thead class="table-dark">';
    html += '<tr><th>Producto</th><th>Cantidad</th><th>Precio</th><th>Estado</th></tr>';
    html += '</thead><tbody>';
    
    // ✅ NUEVO: Usar solo items activos en el resumen
    activeItems.forEach(item => {
        const statusClass = getStatusClass(item.status);
        html += `<tr>
            <td>${item.productName}</td>
            <td>${item.quantity}</td>
            <td>$${item.price.toFixed(2)}</td>
            <td><span class="badge ${statusClass}">${item.status}</span></td>
        </tr>`;
    });
    
    html += '</tbody></table>';
    
    // ✅ NUEVO: Calcular total solo con items activos
    const activeTotal = activeItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    html += `<div class="mt-3"><strong>Total: $${activeTotal.toFixed(2)}</strong></div>`;
    html += '</div></div>';
    
    return html;
}

// ✅ NUEVO: Función auxiliar para obtener clase CSS del estado
function getStatusClass(status) {
    switch(status.toLowerCase()) {
        case 'pending': return 'bg-warning';
        case 'preparing': return 'bg-info';
        case 'ready': return 'bg-success';
        case 'served': return 'bg-primary';
        case 'cancelled': return 'bg-danger';
        default: return 'bg-secondary';
    }
}

// Modificar la función handleTableClick para incluir SignalR
async function handleTableClick(tableId, tableNumber, status) {
    console.log('🔍 [OrderManagement] handleTableClick() - Función ejecutada correctamente');
    console.log('📋 [OrderManagement] handleTableClick() - Parámetros:', { tableId, tableNumber, status });
    
    // Asignar el tableId recibido a currentOrder antes de cualquier proceso
    if (!currentOrder) currentOrder = {};
    currentOrder.tableId = tableId;
    
    // Siempre intentar cargar la orden existente primero
    const existingOrder = await loadExistingOrder(tableId);
    
    // Unirse a grupos de SignalR
    await joinSignalRGroups(tableId, currentOrder.orderId);
    
    // Si hay una orden existente, refrescar usando forceRefreshOrder para asegurar agrupación correcta
    if (existingOrder && existingOrder.hasActiveOrder) {
        // ✅ NUEVO: Actualizar UI local para mesa ocupada
        console.log('🔄 [OrderManagement] handleTableClick() - Mesa ya ocupada, actualizando UI local...');
        if (typeof updateTableStatus === 'function') {
            updateTableStatus(tableId, 'Ocupada');
            console.log('✅ [OrderManagement] handleTableClick() - UI local actualizada para mesa ocupada');
        }
        
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
        console.log('🔍 [OrderManagement] loadExistingOrder() - Cargando orden existente para mesa:', tableId);
        
        const response = await fetch(`/Order/GetActiveOrder?tableId=${tableId}`);
        
        if (response.ok) {
            const result = await response.json();
            console.log('📡 [OrderManagement] loadExistingOrder() - Respuesta recibida:', result);
            
            if (result.hasActiveOrder) {
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
                        taxRate: item.taxRate || 0,
                        isFromBackend: true // ✅ PARÁMETRO PARA IDENTIFICAR ITEMS DEL BACKEND
                    })),
                    total: result.totalAmount || 0,
                    status: result.status || null
                };
                
                updateOrderUI();
                highlightSelectedTable(tableId);
                enableConfirmButton();
                
                // Cambiar el texto del botón según el estado
                const sendButton = document.getElementById('sendToKitchen');
                
                if (result.status === 'SentToKitchen' || result.status === 'Preparing') {
                    sendButton.textContent = 'Agregar a Cocina';
                } else {
                    sendButton.textContent = 'Enviar a Cocina';
                }
                
                // Unirse al grupo de SignalR para esta orden
                if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
                    await signalRConnection.invoke("JoinOrderGroup", currentOrder.orderId);
                }
                
                // Actualizar información de pagos
                if (typeof updatePaymentInfo === 'function') {
                    await updatePaymentInfo();
                }
                
                // ✅ NUEVO: Cargar descuento si existe en la orden
                if (result.discount && result.discount.amount > 0) {
                    if (typeof currentDiscount !== 'undefined') {
                        currentDiscount = {
                            type: result.discount.type || 'amount',
                            value: result.discount.value || result.discount.amount,
                            amount: result.discount.amount,
                            reason: result.discount.reason || '',
                            applied: true
                        };
                    }
                }
                
                return result;
            } else {
                return null;
            }
        } else {
            const errorText = await response.text();
            console.error('❌ [OrderManagement] loadExistingOrder() - Error HTTP:', response.status, errorText);
            
            // Intentar parsear el error como JSON
            try {
                const errorResult = JSON.parse(errorText);
                console.error('❌ [OrderManagement] loadExistingOrder() - Error del servidor:', errorResult);
                
                if (errorResult.error && errorResult.error.includes('Orden no encontrada')) {
                    console.log('ℹ️ [OrderManagement] loadExistingOrder() - No hay orden activa para esta mesa, iniciando nueva orden');
                    return null; // No mostrar error, simplemente iniciar nueva orden
                }
                
                throw new Error(`Error al obtener orden activa: ${errorResult.error}`);
            } catch (parseError) {
                throw new Error(`Error al obtener orden activa (HTTP ${response.status}): ${errorText}`);
            }
        }
    } catch (error) {
        console.error('❌ [OrderManagement] loadExistingOrder() - Error:', error);
        
        // Si el error es "Orden no encontrada", no mostrar error al usuario
        if (error.message && error.message.includes('Orden no encontrada')) {
            console.log('ℹ️ [OrderManagement] loadExistingOrder() - No hay orden activa, continuando con nueva orden');
            return null;
        }
        
        Swal.fire('Error', `No se pudo cargar la orden existente: ${error.message}`, 'error');
        return null;
    }
}

async function startNewOrder(tableId, tableNumber) {
    try {
        console.log('🔍 [OrderManagement] startNewOrder() - Iniciando nueva orden...');
        console.log('📋 [OrderManagement] startNewOrder() - TableId:', tableId, 'TableNumber:', tableNumber);
        
        // ✅ NUEVO: Marcar mesa como ocupada inmediatamente
        console.log('🔄 [OrderManagement] startNewOrder() - Marcando mesa como ocupada...');
        const response = await fetch('/Order/SetTableOccupied', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ TableId: tableId })
        });
        
        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                console.log('✅ [OrderManagement] startNewOrder() - Mesa marcada como ocupada exitosamente');
                console.log('📤 [OrderManagement] startNewOrder() - Notificación SignalR enviada automáticamente');
                
                // ✅ NUEVO: Actualizar UI local inmediatamente
                console.log('🔄 [OrderManagement] startNewOrder() - Actualizando UI local inmediatamente...');
                if (typeof updateTableStatus === 'function') {
                    updateTableStatus(tableId, 'Ocupada');
                    console.log('✅ [OrderManagement] startNewOrder() - UI local actualizada');
                } else {
                    console.warn('⚠️ [OrderManagement] startNewOrder() - Función updateTableStatus no disponible');
                }
            } else {
                console.warn('⚠️ [OrderManagement] startNewOrder() - No se pudo marcar la mesa como ocupada:', result.message);
            }
        } else {
            console.error('❌ [OrderManagement] startNewOrder() - Error HTTP al marcar mesa como ocupada:', response.status);
        }
    } catch (error) {
        console.error('❌ [OrderManagement] startNewOrder() - Error al marcar mesa como ocupada:', error);
    }
    
    currentOrder = { 
        items: [], 
        total: 0, 
        tableId: tableId, 
        orderId: null, 
        status: null 
    };
    
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
    
    console.log('✅ [OrderManagement] startNewOrder() - Nueva orden iniciada exitosamente');
}

// Función para forzar la actualización de la orden
async function forceRefreshOrder() {
    if (currentOrder && currentOrder.orderId) {

        await refreshOrderStatus(currentOrder.orderId);
    } else {

        Swal.fire('Info', 'No hay una orden activa para actualizar', 'info');
    }
}

// Refrescar estado de la orden desde el servidor
async function refreshOrderStatus(orderId) {
    const generationAtStart = orderSummaryGeneration;
    try {
        const response = await fetch(`/Order/GetOrderStatus/${orderId}`);

        if (!shouldApplyOrderRefresh(generationAtStart)) {
            return;
        }

        if (response.ok) {
            const result = await response.json();

            if (!shouldApplyOrderRefresh(generationAtStart)) {
                return;
            }

            if (result.success) {
                if (isTerminalOrderStatus(result.status)) {
                    resetActiveOrderState({ keepTableId: true });
                    return;
                }

                currentOrder.orderId = result.orderId;
                currentOrder.status = result.status;
                currentOrder.total = result.totalAmount;
                currentOrder.items = (result.items || []).map(item => ({
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
                    isFromBackend: true
                }));

                updateOrderUI();

                if (typeof initializeSeparateAccounts === 'function') {
                    initializeSeparateAccounts(result.orderId);
                }

                if (typeof updatePaymentInfo === 'function') {
                    await updatePaymentInfo();
                }
            } else {
                Swal.fire('Error', 'Error al obtener datos del servidor: ' + result.error, 'error');
            }
        } else if (response.status === 404) {
            resetActiveOrderState({ keepTableId: true });
            Swal.fire({
                title: 'Orden eliminada',
                text: 'La orden ha sido eliminada o cancelada',
                icon: 'info',
                timer: 2500,
                showConfirmButton: false
            });
        } else {
            Swal.fire('Error', 'Error HTTP: ' + response.status, 'error');
        }
    } catch (error) {
        Swal.fire('Error', 'Error al refrescar orden: ' + error.message, 'error');
    }
}

// Actualizar estado de la orden
function updateOrderStatus(newStatus) {
    if (currentOrder) {
        // 🎯 LOG ESTRATÉGICO: ESTADO DE ORDEN ACTUALIZADO
        console.log('🚀 [OrderManagement] updateOrderStatus() - ESTADO DE ORDEN ACTUALIZADO - Nuevo estado:', newStatus);
        
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
    resetActiveOrderState({ keepTableId: true });
    Swal.fire({
        title: 'Orden Cancelada',
        text: 'La orden ha sido cancelada',
        icon: 'warning',
        timer: 2000,
        showConfirmButton: false
    });
}

// Función para limpiar solo los items nuevos sin afectar la orden existente
function clearNewItemsOnly() {
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
    

}

// Vaciar orden
async function clearOrder() {
    const result = await Swal.fire({
        title: '¿Vaciar orden?',
        text: '¿Estás seguro de que deseas vaciar toda la orden?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, vaciar',
        cancelButtonText: 'Cancelar'
    });
    if (result.isConfirmed) {
        try {
            // 🎯 NUEVO: Verificar si la orden quedará vacía antes de limpiar
            const willBeEmpty = !currentOrder.items || currentOrder.items.length === 0;
            
            // Simplemente limpiar la interfaz sin cancelar la orden en el backend
            limpiarUIYEstadoLocal();
            
            // 🎯 NUEVO: Si la orden quedó vacía, verificar estado de mesa
            if (willBeEmpty && currentOrder.orderId) {
                console.log('🔍 [OrderManagement] clearOrder() - Orden vaciada, verificando estado de mesa');
                await checkAndUpdateTableIfOrderEmpty();
            }
            
            await Swal.fire({
                title: 'Orden Vaciada',
                text: 'La orden ha sido vaciada de la interfaz',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            });
        } catch (error) {
            Swal.fire('Error', 'No se pudo vaciar la orden', 'error');
        }
    }
}

function limpiarUIYEstadoLocal() {
    // Preservar orderId y status si existe una orden activa
    const orderId = currentOrder.orderId;
    const status = currentOrder.status;
    const tableId = currentOrder.tableId;
    
    // Limpiar solo los items y total
    currentOrder = { 
        items: [], 
        total: 0, 
        tableId: tableId,
        orderId: orderId,
        status: status
    };
    
    try {
        // Limpiar selección de mesas
        document.querySelectorAll('.mesa-btn').forEach(btn => btn.classList.remove('active'));
        
        // Limpiar tarjetas de productos
        document.querySelectorAll('.product-card').forEach(card => {
            card.classList.remove('selected-product');
            card.style.backgroundColor = '';
            card.style.border = '';
            card.style.transform = '';
            card.style.boxShadow = '';
        });
        
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
        
        // Actualizar UI de la orden
        updateOrderUI();
        
        // ✅ NUEVO: Limpiar descuento
        if (typeof initializeDiscount === 'function') {
            initializeDiscount();
        }
        
        disableConfirmButton();
        
        // Resetear texto del botón
        const sendButton = document.getElementById('sendToKitchen');
        sendButton.textContent = 'Confirmar Pedido';
    } catch (error) {
        throw error;
    }
}

window.currentOrder = currentOrder;
window.resetActiveOrderState = resetActiveOrderState;
window.shouldApplyOrderRefresh = shouldApplyOrderRefresh;
window.isTerminalOrderStatus = isTerminalOrderStatus;

// ✅ LOG: Confirmar que el archivo se carga correctamente
console.log('✅ [OrderManagement] order-management.js cargado correctamente');
console.log('✅ [OrderManagement] handleTableClick disponible:', typeof handleTableClick === 'function'); 