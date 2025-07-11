// SignalR Connection Management
let signalRConnection = null;

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
            console.log(`[SignalR] OrderStatusChanged: ${orderId} -> ${newStatus}`);
            if (currentOrder.orderId === orderId) {
                // Refrescar el estado completo desde el servidor
                refreshOrderStatus(orderId);
            }
        });

        signalRConnection.on("OrderItemStatusChanged", (orderId, itemId, newStatus) => {
            console.log(`[SignalR] OrderItemStatusChanged: ${orderId} -> ${itemId} -> ${newStatus}`);
            if (currentOrder.orderId === orderId) {
                // Refrescar el estado completo desde el servidor
                refreshOrderStatus(orderId);
            }
        });

        signalRConnection.on("OrderItemUpdated", function (data) {
            console.log("[SignalR] OrderItemUpdated:", data);
            
            // Actualizar el DOM del ítem afectado usando ProductId
            const row = document.querySelector(`[data-item-id="${data.ProductId}"]`);
            if (row) {
                console.log("[Frontend] Actualizando fila del item:", data.ProductId);
                
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
                
                console.log("[Frontend] Item actualizado exitosamente en la UI");
            } else {
                console.warn("[Frontend] No se encontró la fila del item:", data.ProductId);
            }
        });

        signalRConnection.on("OrderCancelled", (orderId) => {
            console.log(`[SignalR] OrderCancelled: ${orderId}`);
            if (currentOrder.orderId === orderId) {
                handleOrderCancelled();
            }
        });

        signalRConnection.on("TableStatusChanged", (tableId, newStatus) => {
            console.log(`[SignalR] TableStatusChanged recibido:`);
            console.log(`[SignalR]   - tableId: ${tableId}`);
            console.log(`[SignalR]   - newStatus: ${newStatus}`);
            console.log(`[SignalR]   - currentOrder.tableId: ${currentOrder?.tableId}`);
            
            // SIEMPRE actualizar la mesa, sin importar si es la actual o no
            console.log(`[SignalR] Actualizando estado de mesa: ${tableId} -> ${newStatus}`);
            updateTableStatus(tableId, newStatus);
        });

        signalRConnection.on("NewOrder", (orderId, tableNumber) => {
            console.log(`[SignalR] NewOrder: ${orderId} -> ${tableNumber}`);
            showNewOrderNotification(orderId, tableNumber);
        });

        signalRConnection.on("KitchenUpdate", () => {
            console.log(`[SignalR] KitchenUpdate`);
            showKitchenUpdateNotification();
        });

        signalRConnection.on("PaymentProcessed", (orderId, amount, method, isFullyPaid) => {
            console.log(`[SignalR] PaymentProcessed: ${orderId} -> $${amount} (${method}) - Completo: ${isFullyPaid}`);
            if (currentOrder.orderId === orderId) {
                handlePaymentProcessed(amount, method, isFullyPaid);
            }
        });

        // Manejar reconexión
        signalRConnection.onreconnecting(() => {
            console.log("[SignalR] Reconectando...");
            statusIndicator.className = 'signalr-status connecting';
            statusIndicator.title = 'Reconectando...';
        });

        signalRConnection.onreconnected(() => {
            console.log("[SignalR] Reconectado");
            statusIndicator.className = 'signalr-status connected';
            statusIndicator.title = 'Conectado';
            // Reunirse a los grupos necesarios
            if (currentOrder.orderId) {
                signalRConnection.invoke("JoinOrderGroup", currentOrder.orderId);
            }
            if (currentOrder.tableId) {
                signalRConnection.invoke("JoinTableGroup", currentOrder.tableId);
            }
        });

        signalRConnection.onclose(() => {
            console.log("[SignalR] Conexión cerrada");
            statusIndicator.className = 'signalr-status';
            statusIndicator.title = 'Desconectado';
        });

        await signalRConnection.start();
        console.log("[SignalR] Conectado al hub");
        
        statusIndicator.className = 'signalr-status connected';
        statusIndicator.title = 'Conectado';

        // Unirse al grupo de cocina
        await signalRConnection.invoke("JoinKitchenGroup");
        
        // Unirse al grupo general de mesas para recibir todas las notificaciones
        await signalRConnection.invoke("JoinAllTablesGroup");
        console.log("[SignalR] Unido al grupo general de mesas");
    } catch (error) {
        console.error("[SignalR] Error al conectar:", error);
        const statusIndicator = document.getElementById('signalrStatus');
        statusIndicator.className = 'signalr-status';
        statusIndicator.title = 'Error de conexión';
    }
}

// Unirse a grupos de SignalR cuando se selecciona una mesa
async function joinSignalRGroups(tableId, orderId) {
    console.log('[SignalR] joinSignalRGroups iniciado');
    console.log('[SignalR] tableId:', tableId);
    console.log('[SignalR] orderId:', orderId);
    console.log('[SignalR] signalRConnection state:', signalRConnection?.state);
    
    if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
        try {
            if (tableId) {
                console.log('[SignalR] Uniéndose al grupo de mesa:', tableId);
                await signalRConnection.invoke("JoinTableGroup", tableId);
                console.log('[SignalR] Unido exitosamente al grupo de mesa:', tableId);
            }
            if (orderId) {
                console.log('[SignalR] Uniéndose al grupo de orden:', orderId);
                await signalRConnection.invoke("JoinOrderGroup", orderId);
                console.log('[SignalR] Unido exitosamente al grupo de orden:', orderId);
            }
        } catch (error) {
            console.error("[SignalR] Error al unirse a grupos:", error);
        }
    } else {
        console.warn('[SignalR] No se puede unir a grupos - SignalR no está conectado');
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
    console.log(`[SignalR] handlePaymentProcessed: $${amount} (${method}) - Completo: ${isFullyPaid}`);
    
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
        console.log(`[SignalR] Pago completo - Limpiando UI para nueva orden`);
        
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
            console.log(`[SignalR] Información de pagos limpiada para nueva orden`);
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
            console.log(`[SignalR] Información de pagos actualizada para pago parcial`);
        }
        
        // Refrescar el estado completo de la orden
        if (currentOrder.orderId) {
            await refreshOrderStatus(currentOrder.orderId);
            console.log(`[SignalR] Estado de orden refrescado`);
        }
    }
}

// Función para limpiar el resumen de pagos
function clearPaymentSummary() {
    console.log('[SignalR] clearPaymentSummary iniciado - Limpiando resumen de pagos');
    
    // Limpiar elementos de pago
    const totalPaidElement = document.getElementById('totalPaid');
    const remainingAmountElement = document.getElementById('remainingAmount');
    
    if (totalPaidElement) {
        totalPaidElement.textContent = '$0.00';
        console.log('[SignalR] totalPaid limpiado');
    }
    
    if (remainingAmountElement) {
        remainingAmountElement.textContent = '$0.00';
        console.log('[SignalR] remainingAmount limpiado');
    }
    
    // Ocultar botones de pago
    const paymentBtn = document.getElementById('partialPaymentBtn');
    const historyBtn = document.getElementById('paymentHistoryBtn');
    
    if (paymentBtn) {
        paymentBtn.style.display = 'none';
        console.log('[SignalR] Botón de pago parcial ocultado');
    }
    
    if (historyBtn) {
        historyBtn.style.display = 'none';
        console.log('[SignalR] Botón de historial ocultado');
    }
    
    console.log('[SignalR] clearPaymentSummary completado');
}

// Exportar función para uso global
window.clearPaymentSummary = clearPaymentSummary;
window.signalRConnection = signalRConnection; 