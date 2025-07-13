// SignalR Connection Management
let signalRConnection = null;

// ✅ NUEVA FUNCIÓN: Remover item de la UI local cuando se cancela
function removeItemFromUILocal(itemId) {
    console.log('🔍 ENTRADA: removeItemFromUILocal() - itemId:', itemId);
    
    try {
        // Buscar el elemento del item en la tabla
        const itemRow = document.querySelector(`tr[data-item-id="${itemId}"]`);
        
        if (itemRow) {
            console.log('🗑️ [SignalR] removeItemFromUILocal() - Item encontrado, removiendo de UI...');
            
            // Remover la fila de la tabla
            itemRow.remove();
            
            // Actualizar el objeto currentOrder local
            if (currentOrder && currentOrder.items) {
                const itemIndex = currentOrder.items.findIndex(item => item.id === itemId);
                if (itemIndex !== -1) {
                    console.log('🗑️ [SignalR] removeItemFromUILocal() - Removiendo item de currentOrder...');
                    currentOrder.items.splice(itemIndex, 1);
                    
                    // Recalcular totales
                    currentOrder.total = currentOrder.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
                    
                    // Actualizar UI
                    updateOrderUI();
                    
                    console.log('✅ [SignalR] removeItemFromUILocal() - Item removido exitosamente de UI local');
                }
            }
        } else {
            console.log('⚠️ [SignalR] removeItemFromUILocal() - Item no encontrado en UI');
        }
    } catch (error) {
        console.error('❌ [SignalR] removeItemFromUILocal() - Error:', error);
    }
}

// Inicializar SignalR
async function initializeSignalR() {
    try {
        const statusIndicator = document.getElementById('signalrStatus');
        statusIndicator.className = 'signalr-status connecting';
        statusIndicator.title = 'Conectando...';

        signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl("/orderHub")
            .withAutomaticReconnect()
            .build();

        // Configurar eventos de SignalR
        signalRConnection.on("OrderStatusChanged", (orderId, newStatus) => {
            // 🎯 LOG ESTRATÉGICO: NOTIFICACIÓN DE CAMBIO DE ESTADO RECIBIDA
            console.log('🚀 [SignalR] OrderStatusChanged() - NOTIFICACIÓN RECIBIDA - OrderId:', orderId, 'Nuevo estado:', newStatus);
            
            if (currentOrder.orderId === orderId) {
                console.log('🔄 [SignalR] OrderStatusChanged() - Actualizando orden actual...');
                // Refrescar el estado completo desde el servidor
                refreshOrderStatus(orderId);
            } else {
                console.log('ℹ️ [SignalR] OrderStatusChanged() - Notificación para otra orden, ignorando...');
            }
        });

        signalRConnection.on("OrderItemStatusChanged", (data) => {
            console.log('🔍 ENTRADA: OrderItemStatusChanged SignalR - data:', data);
            
            // 🎯 LOG ESTRATÉGICO: NOTIFICACIÓN DE ITEM CANCELADO RECIBIDA
            if (data.Status === 'Cancelled') {
                console.log('🚀 [SignalR] OrderItemStatusChanged() - ITEM CANCELADO - OrderId:', data.OrderId, 'ItemId:', data.ItemId);
            }
            
            console.log('📡 [SignalR] OrderItemStatusChanged recibido:', data);
            
            // ✅ NUEVO: Mostrar notificación si es un item cancelado
            if (data.Status === 'Cancelled' && data.Message) {
                showOrderItemDeletedNotification(data.Message, data.Type || 'warning');
                
                // ✅ NUEVO: Mostrar notificación específica para items cancelados
                Swal.fire({
                    title: 'Item Cancelado',
                    text: `${data.ProductName} fue cancelado y removido de la orden`,
                    icon: 'info',
                    timer: 3000,
                    showConfirmButton: false,
                    toast: true,
                    position: 'top-end'
                });
            }
            
            if (currentOrder.orderId === data.OrderId) {
                console.log('🔄 [SignalR] OrderItemStatusChanged() - Actualizando orden actual...');
                
                // ✅ NUEVO: Si el item fue cancelado, removerlo de la UI local inmediatamente
                if (data.Status === 'Cancelled') {
                    console.log('🗑️ [SignalR] OrderItemStatusChanged() - Removiendo item cancelado de UI local...');
                    removeItemFromUILocal(data.ItemId);
                }
                
                // Refrescar el estado completo desde el servidor
                refreshOrderStatus(data.OrderId);
            }
        });

        signalRConnection.on("OrderItemUpdated", function (data) {
            // Actualizar el DOM del ítem afectado usando ProductId
            const row = document.querySelector(`[data-item-id="${data.ProductId}"]`);
            if (row) {
                // Actualizar la celda de estado
                const estadoCell = row.querySelector(".estado-cell");
                if (estadoCell) {
                    estadoCell.innerHTML = `✅ Listo<br><small>${data.Timestamp}</small>`;
                }
                
                // Cambiar clases de la fila
                row.classList.remove("table-warning", "table-info");
                row.classList.add("table-success");
                
                // Actualizar celda de acciones
                const accionesCell = row.querySelector(".acciones-cell");
                if (accionesCell) {
                    accionesCell.innerHTML = `<i class="text-muted">✔</i>`;
                }
                
                // Mostrar notificación
                Swal.fire({
                    title: 'Item Listo',
                    text: `${data.ProductName} está listo para servir`,
                    icon: 'success',
                    timer: 3000,
                    showConfirmButton: false,
                    toast: true,
                    position: 'top-end'
                });
            }
        });

        signalRConnection.on("OrderCancelled", (orderId) => {
            if (currentOrder.orderId === orderId) {
                handleOrderCancelled();
            }
        });

        signalRConnection.on("OrderCompleted", (data) => {
            console.log('📡 [SignalR] OrderCompleted recibido:', data);
            
            // ✅ NUEVO: Mostrar notificación de orden completada
            if (data.Message) {
                showOrderCompletedNotification(data.Message, data.Type || 'success');
            }
            
            // ✅ NUEVO: Actualizar estado de mesa si es relevante
            if (data.TableNumber && currentOrder.tableNumber === data.TableNumber) {
                console.log('🔄 [SignalR] OrderCompleted - Actualizando estado de mesa a ParaPago');
                updateTableStatus(data.TableNumber, 'ParaPago');
                
                // ✅ NUEVO: Refrescar la orden para mostrar el estado actualizado
                if (currentOrder.orderId) {
                    console.log('🔄 [SignalR] OrderCompleted - Refrescando estado de la orden');
                    refreshOrderStatus(currentOrder.orderId);
                }
            }
        });

        signalRConnection.on("TableStatusChanged", (data) => {
            console.log('🔍 [SignalR] TableStatusChanged() - INICIANDO - data recibida:', data);
            
            // ✅ CORREGIDO: Usar las propiedades correctas (minúsculas)
            const tableId = data.tableId || data.TableId;
            const newStatus = data.newStatus || data.NewStatus;
            
            console.log('📋 [SignalR] TableStatusChanged() - Extraídos parámetros:');
            console.log('📋 [SignalR] TableStatusChanged() - tableId:', tableId);
            console.log('📋 [SignalR] TableStatusChanged() - newStatus:', newStatus);
            
            // 🎯 LOG ESTRATÉGICO: NOTIFICACIÓN DE MESA RECIBIDA
            console.log('🚀 [SignalR] TableStatusChanged() - NOTIFICACIÓN DE MESA RECIBIDA - TableId:', tableId, 'Nuevo estado:', newStatus);
            console.log('📡 [SignalR] TableStatusChanged() - Datos completos recibidos:', data);
            
            // ✅ NUEVO: Mostrar notificación de cambio de estado
            if (data.message || data.Message) {
                console.log('📢 [SignalR] TableStatusChanged() - Mostrando notificación al usuario...');
                showTableStatusNotification(data.message || data.Message, data.type || data.Type || 'info');
                console.log('✅ [SignalR] TableStatusChanged() - Notificación mostrada');
            }
            
            // SIEMPRE actualizar la mesa, sin importar si es la actual o no
            console.log('🔄 [SignalR] TableStatusChanged() - Llamando updateTableStatus con parámetros:');
            console.log('🔄 [SignalR] TableStatusChanged() - updateTableStatus(' + tableId + ', ' + newStatus + ')');
            
            updateTableStatus(tableId, newStatus);
            
            console.log('✅ [SignalR] TableStatusChanged() - COMPLETADO - Handler ejecutado exitosamente');
        });

        // ✅ NUEVO: Escuchar nuevas órdenes (para notificación en Order/Index)
        signalRConnection.on("NewOrder", (data) => {
            console.log('📡 [SignalR] NewOrder recibido:', data);
            
            // Mostrar notificación de nueva orden
            showNewOrderNotification(data.OrderId, data.TableNumber);
        });

        signalRConnection.on("KitchenUpdate", () => {
            showKitchenUpdateNotification();
        });

        signalRConnection.on("PaymentProcessed", (orderId, amount, method, isFullyPaid) => {
    
            if (currentOrder.orderId === orderId) {
                handlePaymentProcessed(amount, method, isFullyPaid);
            }
        });

        // Manejar reconexión
        signalRConnection.onreconnecting(() => {
    
            statusIndicator.className = 'signalr-status connecting';
            statusIndicator.title = 'Reconectando...';
        });

        signalRConnection.onreconnected(() => {
            console.log('🔄 [SignalR] Reconectado, reuniéndose a grupos...');
            statusIndicator.className = 'signalr-status connected';
            statusIndicator.title = 'Conectado';
            
            // Reunirse a los grupos necesarios
            if (currentOrder.orderId) {
                signalRConnection.invoke("JoinOrderGroup", currentOrder.orderId);
                console.log('✅ [SignalR] Reunido al grupo de orden:', currentOrder.orderId);
            }
            if (currentOrder.tableId) {
                signalRConnection.invoke("JoinTableGroup", currentOrder.tableId);
                console.log('✅ [SignalR] Reunido al grupo de mesa:', currentOrder.tableId);
            }
            
            // ✅ NUEVO: Reunirse al grupo de órdenes
            signalRConnection.invoke("JoinOrdersGroup");
            console.log('✅ [SignalR] Reunido al grupo "orders"');
            
            // Reunirse al grupo de cocina
            signalRConnection.invoke("JoinKitchenGroup");
            console.log('✅ [SignalR] Reunido al grupo "kitchen"');
            
            // Reunirse al grupo general de mesas
            signalRConnection.invoke("JoinAllTablesGroup");
            console.log('✅ [SignalR] Reunido al grupo "table_all"');
        });

        signalRConnection.onclose(() => {
            statusIndicator.className = 'signalr-status';
            statusIndicator.title = 'Desconectado';
        });

        await signalRConnection.start();
        
        statusIndicator.className = 'signalr-status connected';
        statusIndicator.title = 'Conectado';

        // ✅ NUEVO: Unirse al grupo de órdenes para recibir notificaciones
        await signalRConnection.invoke("JoinOrdersGroup");
        console.log('✅ [SignalR] Unido al grupo "orders" exitosamente');

        // Unirse al grupo de cocina
        await signalRConnection.invoke("JoinKitchenGroup");
        
        // Unirse al grupo general de mesas para recibir todas las notificaciones
        await signalRConnection.invoke("JoinAllTablesGroup");
    } catch (error) {
        
        const statusIndicator = document.getElementById('signalrStatus');
        statusIndicator.className = 'signalr-status';
        statusIndicator.title = 'Error de conexión';
    }
}

// Unirse a grupos de SignalR cuando se selecciona una mesa
async function joinSignalRGroups(tableId, orderId) {
    if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
        try {
            console.log('🔍 [SignalR] joinSignalRGroups() - Uniéndose a grupos SignalR...');
            console.log('📋 [SignalR] joinSignalRGroups() - TableId:', tableId, 'OrderId:', orderId);
            
            if (tableId) {
                await signalRConnection.invoke("JoinTableGroup", tableId);
                console.log('✅ [SignalR] joinSignalRGroups() - Unido al grupo de mesa:', tableId);
            }
            if (orderId) {
                await signalRConnection.invoke("JoinOrderGroup", orderId);
                console.log('✅ [SignalR] joinSignalRGroups() - Unido al grupo de orden:', orderId);
            }
            
            // ✅ NUEVO: Unirse al grupo 'orders' para recibir notificaciones de cambio de estado
            await signalRConnection.invoke("JoinOrdersGroup");
            console.log('✅ [SignalR] joinSignalRGroups() - Unido al grupo de órdenes (orders)');
            
        } catch (error) {
            console.error('❌ [SignalR] joinSignalRGroups() - Error al unirse a grupos:', error);
        }
    } else {
        console.warn('⚠️ [SignalR] joinSignalRGroups() - Conexión SignalR no está conectada');
    }
}

// Mostrar notificación de nueva orden
function showNewOrderNotification(orderId, tableNumber) {
    Swal.fire({
        title: 'Nueva Orden',
        text: `Nueva orden recibida para Mesa ${tableNumber}`,
        icon: 'info',
        timer: 5000,
        showConfirmButton: false,
        toast: true,
        position: 'top-end'
    });
}

// ✅ NUEVO: Mostrar notificación de cambio de estado de mesa
function showTableStatusNotification(message, type = 'info') {
    try {
        console.log('🔔 [SignalR] showTableStatusNotification() - Mostrando notificación:', { message, type });
        
        const iconMap = {
            'success': 'success',
            'error': 'error', 
            'warning': 'warning',
            'info': 'info',
            'table_status_changed': 'info'
        };
        
        const icon = iconMap[type] || 'info';
        
        Swal.fire({
            title: 'Estado de Mesa',
            text: message,
            icon: icon,
            timer: 4000,
            showConfirmButton: false,
            toast: true,
            position: 'top-end'
        });
    } catch (error) {
        console.error('❌ [SignalR] showTableStatusNotification() - Error:', error);
        alert(message); // Fallback
    }
}

// ✅ NUEVO: Mostrar notificación de orden completada
function showOrderCompletedNotification(message, type = 'success') {
    try {
        console.log('🔔 [SignalR] showOrderCompletedNotification() - Mostrando notificación:', { message, type });
        
        Swal.fire({
            title: 'Orden Completada',
            text: message,
            icon: 'success',
            timer: 5000,
            showConfirmButton: false,
            toast: true,
            position: 'top-end'
        });
    } catch (error) {
        console.error('❌ [SignalR] showOrderCompletedNotification() - Error:', error);
        alert(message); // Fallback
    }
}

// ✅ NUEVO: Mostrar notificación de item eliminado
function showOrderItemDeletedNotification(message, type = 'warning') {
    try {
        console.log('🔔 [SignalR] showOrderItemDeletedNotification() - Mostrando notificación:', { message, type });
        
        Swal.fire({
            title: 'Item Eliminado',
            text: message,
            icon: 'warning',
            timer: 4000,
            showConfirmButton: false,
            toast: true,
            position: 'top-end'
        });
    } catch (error) {
        console.error('❌ [SignalR] showOrderItemDeletedNotification() - Error:', error);
        alert(message); // Fallback
    }
}

// Mostrar notificación de actualización de cocina
function showKitchenUpdateNotification() {
    Swal.fire({
        title: 'Actualización de Cocina',
        text: 'Se han actualizado los estados de los pedidos',
        icon: 'info',
        timer: 3000,
        showConfirmButton: false,
        toast: true,
        position: 'top-end'
    });
}

// Manejar pago procesado
async function handlePaymentProcessed(amount, method, isFullyPaid) {
    
    // Mostrar notificación de pago
    const title = isFullyPaid ? 'Pago Completado' : 'Pago Parcial Procesado';
    const text = isFullyPaid ? 
        `Pago completo de $${amount} (${method}). La orden está completada.` : 
        `Pago parcial de $${amount} (${method}) procesado correctamente.`;
    
    Swal.fire({
        title: title,
        text: text,
        icon: 'success',
        timer: 4000,
        showConfirmButton: false,
        toast: true,
        position: 'top-end'
    });
    
    if (isFullyPaid) {
        // Pago completo: la orden está completada, limpiar UI para nuevo pedido
        
        // Limpiar la orden actual pero mantener la mesa seleccionada
        const currentTableId = currentOrder.tableId;
        
        // Resetear orden para nueva
        currentOrder = {
            items: [],
            total: 0,
            tableId: currentTableId,
            orderId: null,
            status: null
        };
        
        // Actualizar UI
        updateOrderUI();
        
        // Resetear botón de envío
        const sendButton = document.getElementById('sendToKitchen');
        if (sendButton) {
            sendButton.textContent = 'Confirmar Pedido';
        }
        
        // Limpiar información de pagos y resumen completo
        clearPaymentSummary();
        if (typeof updatePaymentInfo === 'function') {
            await updatePaymentInfo();
        }
        
        // Mostrar mensaje adicional
        setTimeout(() => {
            Swal.fire({
                title: 'Mesa Lista',
                text: 'La mesa está disponible para un nuevo pedido',
                icon: 'info',
                timer: 3000,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });
        }, 2000);
        
    } else {
        // Pago parcial: actualizar información manteniendo la orden
        if (typeof updatePaymentInfo === 'function') {
            await updatePaymentInfo();
        }
        
        // Refrescar el estado completo de la orden
        if (currentOrder.orderId) {
            await refreshOrderStatus(currentOrder.orderId);
        }
    }
}

// Función para limpiar el resumen de pagos
function clearPaymentSummary() {
    // Limpiar elementos de pago
    const totalPaidElement = document.getElementById('totalPaid');
    const remainingAmountElement = document.getElementById('remainingAmount');
    
    if (totalPaidElement) {
        totalPaidElement.textContent = '$0.00';
    }
    
    if (remainingAmountElement) {
        remainingAmountElement.textContent = '$0.00';
    }
    
    // Ocultar botones de pago
    const paymentBtn = document.getElementById('partialPaymentBtn');
    const historyBtn = document.getElementById('paymentHistoryBtn');
    
    if (paymentBtn) {
        paymentBtn.style.display = 'none';
    }
    
    if (historyBtn) {
        historyBtn.style.display = 'none';
    }
}

// Exportar función para uso global
window.clearPaymentSummary = clearPaymentSummary;
window.signalRConnection = signalRConnection; 