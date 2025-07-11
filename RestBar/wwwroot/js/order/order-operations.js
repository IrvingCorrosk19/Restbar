// Order Operations (Send to Kitchen, Cancel, etc.)

// Enviar orden a cocina con CRUD automático
async function sendToKitchen() {
    console.log('[Frontend] sendToKitchen iniciado');
    console.log('[Frontend] currentOrder.tableId:', currentOrder.tableId);
    console.log('[Frontend] currentOrder.tableId type:', typeof currentOrder.tableId);
    
    if (!currentOrder.tableId || currentOrder.tableId === "00000000-0000-0000-0000-000000000000") {
        console.error('[Frontend] Error: tableId inválido:', currentOrder.tableId);
        Swal.fire('Atención', 'Selecciona una mesa primero', 'warning');
        return;
    }
    if (currentOrder.items.length === 0) {
        Swal.fire('Atención', 'La orden está vacía', 'warning');
        return;
    }

    try {
        console.log('[Frontend] Enviando orden a cocina:', {
            tableId: currentOrder.tableId,
            orderType: 'DineIn',
            items: currentOrder.items.map(item => ({
                id: item.id,  // ✅ NUEVO: Incluir el ID del order_item
                productId: item.productId,
                quantity: item.quantity,
                notes: item.notes || '',
                status: item.status
            }))
        });
        
        // ✅ LOGGING DETALLADO DE ITEMS
        console.log('[Frontend] === DETALLE DE ITEMS A ENVIAR ===');
        currentOrder.items.forEach((item, index) => {
            console.log(`[Frontend] Item ${index + 1}:`, {
                id: item.id,
                productId: item.productId,
                quantity: item.quantity,
                status: item.status,
                isGuid: item.id && item.id.length === 36
            });
        });
        console.log('[Frontend] === FIN DETALLE DE ITEMS ===');

        const response = await fetch('/Order/SendToKitchen', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                tableId: currentOrder.tableId,
                orderType: 'DineIn',
                items: currentOrder.items.map(item => ({
                    id: item.id,  // ✅ NUEVO: Incluir el ID del order_item
                    productId: item.productId,
                    quantity: item.quantity,
                    notes: item.notes || '',
                    status: item.status
                }))
            })
        });

        if (response.ok) {
            const result = await response.json();
            console.log('[Frontend] Respuesta exitosa del servidor:', result);
            
            // Actualizar la orden con el ID del servidor
            currentOrder.orderId = result.orderId;
            currentOrder.status = result.status || 'SentToKitchen';
            
            console.log('[Frontend] Orden actualizada - ID:', currentOrder.orderId, 'Status:', currentOrder.status);
            
            // Verificar que la orden esté en estado SentToKitchen
            if (currentOrder.status === 'SentToKitchen') {
                console.log('[Frontend] ✅ Orden correctamente en estado SentToKitchen');
            } else {
                console.log('[Frontend] ⚠️ Orden en estado inesperado:', currentOrder.status);
            }
            
            // Actualizar el estado de la mesa
            if (result.updatedTable) {
                updateTableUI(result.updatedTable);
            }

            // Mostrar mensaje de éxito
            await Swal.fire({
                title: '¡Éxito!',
                text: result.message || 'Pedido enviado a cocina exitosamente',
                icon: 'success',
                timer: 2000,
                showConfirmButton: false
            });
            
            // Recargar la orden completa desde el servidor para mostrar el estado actualizado
            console.log('[Frontend] Recargando orden actualizada desde servidor...');
            await loadExistingOrder(currentOrder.tableId);
            
            // Limpiar los arrays de cambios
            newItems = [];
            modifiedItems = [];
            deletedItems = [];
            
            // Unirse al grupo de SignalR para esta orden
            if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
                await signalRConnection.invoke("JoinOrderGroup", currentOrder.orderId);
                console.log('[Frontend] Unido al grupo SignalR para orden:', currentOrder.orderId);
            }
            
            console.log('[Frontend] sendToKitchen completado exitosamente');
        } else {
            const errorData = await response.json();
            console.error('[Frontend] Error del servidor:', errorData);
            throw new Error(errorData.error || 'Error al enviar la orden');
        }
        
    } catch (error) {
        console.error('Error al enviar orden:', error);
        Swal.fire('Error', error.message || 'No se pudo procesar la orden', 'error');
    }
}

// Cancelar orden
async function cancelOrder() {
    console.log('[Frontend] cancelOrder iniciado');
    console.log('[Frontend] currentOrder:', currentOrder);
    
    if (!currentOrder.orderId) {
        console.error('[Frontend] Error: No hay orderId en currentOrder');
        Swal.fire('Error', 'No hay una orden activa para cancelar', 'warning');
        return;
    }

    console.log('[Frontend] Mostrando confirmación de cancelación...');
    // Confirmación simple para cancelar orden
    const result = await Swal.fire({
        title: '¿Cancelar Orden?',
        text: 'Esta acción no se puede deshacer. ¿Estás seguro?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, Cancelar',
        cancelButtonText: 'No, Mantener',
        confirmButtonColor: '#d33'
    });

    console.log('[Frontend] Resultado de confirmación:', result);
    if (result.isConfirmed) {
        console.log('[Frontend] Usuario confirmó cancelación, llamando performCancellation...');
        await performCancellation('Cancelación por usuario');
    } else {
        console.log('[Frontend] Usuario canceló la operación');
    }
}

// ✅ Función para editar un item (cantidad y notas) - MEJORADA
async function editItem(itemId) {
    console.log('[Frontend] editItem iniciado para itemId:', itemId);
    
    const item = currentOrder.items.find(i => i.id === itemId);
    if (!item) {
        console.error('[Frontend] Item no encontrado:', itemId);
        Swal.fire('Error', 'Item no encontrado', 'error');
        return;
    }
    
    // ✅ ANÁLISIS DEL ITEM PARA IDENTIFICACIÓN
    console.log('[Frontend] === ANÁLISIS DE ITEM PARA EDICIÓN ===');
    console.log('[Frontend] Item ID:', item.id);
    console.log('[Frontend] Item isNew:', item.isNew);
    console.log('[Frontend] Item isFromBackend:', item.isFromBackend);
    console.log('[Frontend] Item status:', item.status);
    console.log('[Frontend] Orden status:', currentOrder.status);
    console.log('[Frontend] Orden ID:', currentOrder.orderId);
    console.log('[Frontend] === FIN ANÁLISIS ===');
    
    // Verificar que el item esté en estado Pending y la orden en estado editable
    const editableOrderStates = ['Pending', 'SentToKitchen'];
    const editableItemStates = ['Pending'];
    
    if (!editableItemStates.includes(item.status) || !editableOrderStates.includes(currentOrder.status)) {
        console.warn('[Frontend] No se puede editar item - Status:', item.status, 'Order Status:', currentOrder.status);
        Swal.fire('Atención', 'Solo se pueden editar items pendientes en órdenes pendientes o enviadas a cocina', 'warning');
        return;
    }
    
    // ✅ Usar el nuevo modal de edición con información del tipo de item
    console.log('[Frontend] Abriendo modal de edición para item:', item);
    console.log('[Frontend] Tipo de item:', item.isNew ? 'NUEVO' : item.isFromBackend ? 'EXISTENTE' : 'NO DETERMINADO');
    openEditModal(item.productId, item.productName, item.price, itemId);
}

// Función para realizar la cancelación
async function performCancellation(reason) {
    try {
        console.log('[Frontend] performCancellation iniciado');
        console.log('[Frontend] currentOrder:', currentOrder);
        console.log('[Frontend] currentOrder.orderId:', currentOrder?.orderId);
        console.log('[Frontend] reason:', reason);
        
        // Validar que currentOrder y orderId existan
        if (!currentOrder || !currentOrder.orderId) {
            console.error('[Frontend] Error: currentOrder o orderId es null/undefined');
            console.error('[Frontend] currentOrder:', currentOrder);
            Swal.fire('Error', 'No hay una orden válida para cancelar', 'error');
            return;
        }

        const cancelData = {
            orderId: currentOrder.orderId,
            userId: 'current-user-id', // TODO: Obtener del contexto de autenticación
            reason: reason
        };

        console.log('[Frontend] Datos a enviar para cancelación:', cancelData);
        console.log('[Frontend] JSON a enviar:', JSON.stringify(cancelData));

        console.log('[Frontend] Iniciando fetch a /Order/Cancel...');
        const response = await fetch('/Order/Cancel', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(cancelData)
        });

        console.log('[Frontend] Respuesta del servidor - Status:', response.status);
        console.log('[Frontend] Respuesta del servidor - StatusText:', response.statusText);
        console.log('[Frontend] Respuesta del servidor - Headers:', response.headers);

        if (response.ok) {
            console.log('[Frontend] Respuesta exitosa, procesando JSON...');
            const result = await response.json();
            console.log('[Frontend] Resultado exitoso:', result);
            
            console.log('[Frontend] Mostrando mensaje de éxito...');
            await Swal.fire({
                title: 'Orden Cancelada',
                text: 'La orden ha sido cancelada exitosamente',
                icon: 'success',
                timer: 2000,
                showConfirmButton: false
            });

            console.log('[Frontend] Limpiando orden actual...');
            // Limpiar la orden actual
            await clearOrder();
            
            console.log('[Frontend] Recargando mesas...');
            // Recargar las mesas para actualizar estados
            await loadTables();
            
            console.log('[Frontend] Cancelación completada exitosamente');
        } else {
            console.error('[Frontend] Error en respuesta del servidor');
            console.error('[Frontend] Status:', response.status);
            console.error('[Frontend] StatusText:', response.statusText);
            
            let errorData;
            try {
                console.log('[Frontend] Intentando parsear JSON del error...');
                errorData = await response.json();
                console.error('[Frontend] Error data:', errorData);
            } catch (parseError) {
                console.error('[Frontend] Error parseando JSON del error:', parseError);
                const errorText = await response.text();
                console.error('[Frontend] Error text:', errorText);
                errorData = { error: 'Error del servidor: ' + response.status + ' - ' + errorText.substring(0, 200) };
            }
            
            throw new Error(errorData.error || 'Error al cancelar la orden');
        }
    } catch (error) {
        console.error('[Frontend] Error en performCancellation:', error);
        console.error('[Frontend] Error name:', error.name);
        console.error('[Frontend] Error message:', error.message);
        console.error('[Frontend] Error stack:', error.stack);
        
        Swal.fire('Error', error.message || 'No se pudo cancelar la orden', 'error');
    }
}

// Actualizar cantidad en el pedido
async function updateQuantity(productId, delta, status) {
    console.log('[Frontend] updateQuantity iniciado - productId:', productId, 'delta:', delta, 'status:', status);
    
    // Buscar el item específico por productId y status
    const item = currentOrder.items.find(i => i.productId === productId && i.status === status);
    console.log('[Frontend] Item encontrado:', item);
    
    if (item) {
        const newQuantity = item.quantity + delta;
        console.log('[Frontend] Nueva cantidad calculada:', newQuantity);
        
        if (newQuantity <= 0) {
            console.log('[Frontend] Cantidad <= 0, preguntando si eliminar...');
            const result = await Swal.fire({
                title: '¿Eliminar producto?',
                text: '¿Deseas eliminar este producto de la orden?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar'
            });

            if (result.isConfirmed) {
                console.log('[Frontend] Usuario confirmó eliminar, llamando removeItem...');
                await removeItem(item.id);
            } else {
                console.log('[Frontend] Usuario canceló eliminar');
            }
        } else {
            console.log('[Frontend] Cantidad > 0, actualizando...');
            // Si es una orden existente, actualizar en el servidor
            if (currentOrder.orderId && currentOrder.status) {
                console.log('[Frontend] Es orden existente, actualizando en servidor...');
                try {
                    const requestData = {
                        orderId: currentOrder.orderId,
                        productId: productId,
                        quantity: newQuantity
                    };
                    
                    console.log('[Frontend] Datos para actualizar cantidad:', requestData);
                    
                    const response = await fetch('/Order/UpdateItemQuantityInOrder', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(requestData)
                    });

                    console.log('[Frontend] Respuesta actualizar cantidad - Status:', response.status);

                    if (response.ok) {
                        console.log('[Frontend] Cantidad actualizada exitosamente en servidor');
                        const result = await response.json();
                        console.log('[Frontend] Resultado actualizar cantidad:', result);
                        
                        if (result.orderDeleted) {
                            console.log('[Frontend] Orden eliminada completamente, limpiando interfaz...');
                            
                            // Obtener el tableId antes de limpiar currentOrder
                            const tableIdToUpdate = currentOrder.tableId;
                            
                            // Limpiar completamente la orden actual
                            currentOrder = { 
                                items: [], 
                                total: 0, 
                                tableId: null, 
                                orderId: null, 
                                status: null 
                            };
                            document.querySelectorAll('.mesa-btn').forEach(btn => btn.classList.remove('active'));
                            updateOrderUI();
                            
                            // Actualizar el estado de la mesa
                            if (tableIdToUpdate) {
                                await loadTables();
                            }
                            
                            Swal.fire('Orden Eliminada', 'La orden ha sido eliminada completamente', 'info');
                            return;
                        }
                        
                        // Actualizar el item local con la nueva cantidad
                        item.quantity = newQuantity;
                        updateOrderUI();
                        
                        console.log('[Frontend] Cantidad actualizada localmente');
                    } else {
                        console.error('[Frontend] Error al actualizar cantidad en servidor');
                        const errorText = await response.text();
                        console.error('[Frontend] Error response:', errorText);
                        Swal.fire('Error', 'No se pudo actualizar la cantidad', 'error');
                    }
                } catch (error) {
                    console.error('[Frontend] Error en actualizar cantidad:', error);
                    Swal.fire('Error', 'Error al actualizar la cantidad', 'error');
                }
            } else {
                // Si es una orden nueva, solo actualizar localmente
                console.log('[Frontend] Es orden nueva, actualizando localmente...');
                item.quantity = newQuantity;
                updateOrderUI();
            }
        }
    } else {
        console.error('[Frontend] Item no encontrado para actualizar');
    }
}

// ✅ Función mejorada para remover item - Maneja items nuevos y existentes
async function removeItem(itemId) {
    console.log('[Frontend] removeItem iniciado - itemId:', itemId);
    
    // Buscar el item específico por ID
    const item = currentOrder.items.find(i => i.id === itemId);
    if (!item) {
        console.error('[Frontend] No se encontró el item a eliminar:', itemId);
        Swal.fire('Error', 'No se encontró el item a eliminar', 'error');
        return;
    }
    
    console.log('[Frontend] Item encontrado:', {
        id: item.id,
        productName: item.productName,
        status: item.status,
        orderId: currentOrder.orderId
    });
    
    // Log detallado de todos los items del mismo producto para debugging
    const itemsWithSameProduct = currentOrder.items.filter(i => i.productId === item.productId);
    console.log('[Frontend] Items con el mismo ProductId:', itemsWithSameProduct.length);
    itemsWithSameProduct.forEach((sameItem, index) => {
        console.log(`[Frontend]   Item ${index + 1}: ID=${sameItem.id}, Status=${sameItem.status}, Quantity=${sameItem.quantity}`);
    });
    
    // Log completo de todos los items en la orden
    console.log('[Frontend] === TODOS LOS ITEMS EN LA ORDEN ===');
    currentOrder.items.forEach((orderItem, index) => {
        console.log(`[Frontend] Item ${index + 1}: ID=${orderItem.id}, Product=${orderItem.productName}, Status=${orderItem.status}, Quantity=${orderItem.quantity}`);
    });
    console.log('[Frontend] === FIN TODOS LOS ITEMS ===');
    
    // Confirmación antes de eliminar
    const result = await Swal.fire({
        title: '¿Eliminar Item?',
        text: `¿Estás seguro de que quieres eliminar ${item.productName} del pedido?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, Eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#dc3545'
    });
    
    if (!result.isConfirmed) {
        console.log('[Frontend] Eliminación cancelada por el usuario');
        return;
    }
    
    // ✅ LÓGICA MEJORADA CON IDENTIFICACIÓN CLARA
    
    console.log('[Frontend] === ANÁLISIS DE ITEM PARA ELIMINACIÓN ===');
    console.log('[Frontend] Item ID:', item.id);
    console.log('[Frontend] Item isNew:', item.isNew);
    console.log('[Frontend] Item isFromBackend:', item.isFromBackend);
    console.log('[Frontend] Item status:', item.status);
    console.log('[Frontend] Orden status:', currentOrder.status);
    console.log('[Frontend] Orden ID:', currentOrder.orderId);
    console.log('[Frontend] === FIN ANÁLISIS ===');
    
    // ESCENARIO 1: ITEM NUEVO (isNew = true)
    if (item.isNew === true) {
        console.log('[Frontend] ✅ ESCENARIO 1: Item NUEVO detectado (isNew = true), eliminando solo del frontend');
        
        // Eliminar solo del frontend
        currentOrder.items = currentOrder.items.filter(i => i.id !== itemId);
        updateOrderUI();
        enableConfirmButton();
        
        Swal.fire({
            title: 'Item Eliminado',
            text: `${item.productName} eliminado del pedido (item nuevo)`,
            icon: 'success',
            timer: 1500,
            showConfirmButton: false
        });
        return;
    }
    
    // ESCENARIO 2: ITEM EXISTENTE (isFromBackend = true)
    if (item.isFromBackend === true) {
        console.log('[Frontend] ✅ ESCENARIO 2: Item EXISTENTE detectado (isFromBackend = true), llamando al backend para eliminar');
        console.log('[Frontend] Item ID:', item.id);
        console.log('[Frontend] Item status:', item.status);
        console.log('[Frontend] Orden status:', currentOrder.status);
        
        // Continuar con la lógica del backend (no hacer return aquí)
    }
    
    // ESCENARIO 3: ORDEN NUEVA (sin orderId en backend)
    if (!currentOrder.orderId) {
        console.log('[Frontend] ✅ ESCENARIO 3: Orden NUEVA detectada, eliminando solo del frontend');
        
        // Eliminar solo del frontend
        currentOrder.items = currentOrder.items.filter(i => i.id !== itemId);
        updateOrderUI();
        enableConfirmButton();
        
        Swal.fire({
            title: 'Item Eliminado',
            text: `${item.productName} eliminado del pedido`,
            icon: 'success',
            timer: 1500,
            showConfirmButton: false
        });
        return;
    }
    
    // ESCENARIO 4: CASO AMBIGUO - No se puede determinar claramente
    if (!item.isNew && !item.isFromBackend) {
        console.warn('[Frontend] ⚠️ ESCENARIO 4: CASO AMBIGUO - No se puede determinar si el item es nuevo o existente');
        console.warn('[Frontend] Item:', item);
        Swal.fire('Error', 'No se pudo determinar el tipo de item. Contacte al administrador.', 'error');
        return;
    }
    
    // ESCENARIO 5: ITEM EXISTENTE EN ORDEN ENVIADA A COCINA
    console.log('[Frontend] ✅ ESCENARIO 5: Item EXISTENTE en orden enviada a cocina');
    console.log('[Frontend] Llamando al backend para eliminar item...');
    
    // RESUMEN FINAL DE LA DECISIÓN
    console.log('[Frontend] === RESUMEN DE DECISIÓN ===');
    console.log('[Frontend] Item ID:', item.id);
    console.log('[Frontend] Item Status:', item.status);
    console.log('[Frontend] Orden Status:', currentOrder.status);
    console.log('[Frontend] Orden ID:', currentOrder.orderId);
    console.log('[Frontend] Decisión: ELIMINAR DEL BACKEND');
    console.log('[Frontend] === FIN RESUMEN ===');
    
    // Validar que tenemos el ItemId específico
    if (!item.id || item.id === 'new' || item.id === 'temp') {
        console.error('[Frontend] ERROR: Item no tiene ID válido para eliminación en backend');
        Swal.fire('Error', 'No se puede eliminar este item porque no tiene un ID válido', 'error');
        return;
    }
    
    // Validación adicional para items existentes
    if (!item.id || item.id.length < 10) {
        console.error('[Frontend] ERROR: Item no tiene ID válido para eliminación en backend');
        console.error('[Frontend] Item ID:', item.id);
        console.error('[Frontend] Item ID length:', item.id ? item.id.length : 'undefined');
        Swal.fire('Error', 'No se puede eliminar este item porque no tiene un ID válido', 'error');
        return;
    }
    
    try {
        const requestData = {
            orderId: currentOrder.orderId,
            productId: item.productId,
            status: item.status, // Incluir el status para identificar el item específico
            itemId: item.id // Incluir el ID específico del item para eliminación precisa
        };
        
        console.log('[Frontend] Datos para remover item:', requestData);
        console.log('[Frontend] ItemId específico a eliminar:', item.id);
        console.log('[Frontend] ProductId:', item.productId);
        console.log('[Frontend] Status:', item.status);
        
        const response = await fetch('/Order/RemoveItemFromOrder', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(requestData)
        });
        
        console.log('[Frontend] Respuesta remover item - Status:', response.status);
        
        if (response.ok) {
            const result = await response.json();
            console.log('[Frontend] Resultado remover item:', result);
            
            // Si la orden fue eliminada completamente (quedó vacía)
            if (result.orderDeleted) {
                console.log('[Frontend] Orden eliminada completamente');
                
                // Limpiar completamente la orden
                currentOrder = { 
                    items: [], 
                    total: 0, 
                    tableId: null, 
                    orderId: null, 
                    status: null 
                };
                
                // Limpiar selección de mesa
                document.querySelectorAll('.mesa-btn').forEach(btn => btn.classList.remove('active'));
                updateOrderUI();
                enableConfirmButton();
                
                // Recargar mesas para actualizar estados
                await loadTables();
                
                Swal.fire({
                    title: 'Orden Eliminada',
                    text: 'La orden quedó vacía y fue eliminada completamente',
                    icon: 'info',
                    timer: 2000,
                    showConfirmButton: false
                });
                return;
            }
            
            // Item eliminado exitosamente, actualizar frontend
            console.log('[Frontend] Item eliminado exitosamente del backend');
            
            // Remover el item específico del frontend
            currentOrder.items = currentOrder.items.filter(i => i.id !== itemId);
            updateOrderUI();
            enableConfirmButton();
            
            Swal.fire({
                title: 'Item Eliminado',
                text: `${item.productName} eliminado del pedido`,
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            });
            
        } else {
            // Error del servidor
            const errorData = await response.json();
            console.error('[Frontend] Error del servidor al remover item:', errorData);
            throw new Error(errorData.error || 'Error al eliminar el item');
        }
        
    } catch (error) {
        console.error('[Frontend] Error en remover item:', error);
        Swal.fire('Error', error.message || 'No se pudo eliminar el item', 'error');
    }
} 