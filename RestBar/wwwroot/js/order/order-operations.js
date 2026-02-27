// Order Operations - Funciones para operaciones CRUD de órdenes

// 🎯 FUNCIÓN MEJORADA: Usar método existente CheckAndUpdateTableStatus
async function checkAndUpdateTableStatusAjax(orderId) {
    console.log('🔍 ENTRADA: checkAndUpdateTableStatusAjax() - orderId:', orderId);
    try {
        console.log('🔄 [OrderOperations] checkAndUpdateTableStatusAjax() - Verificando estado de mesa para orden:', orderId);
        
        const response = await fetch('/Order/CheckAndUpdateTableStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                OrderId: orderId
            })
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                console.log('✅ [OrderOperations] checkAndUpdateTableStatusAjax() - Estado de mesa verificado y actualizado');
                return true;
            } else {
                console.error('❌ [OrderOperations] checkAndUpdateTableStatusAjax() - Error del servidor:', result.message);
                return false;
            }
        } else {
            console.error('❌ [OrderOperations] checkAndUpdateTableStatusAjax() - Error HTTP:', response.status);
            return false;
        }
    } catch (error) {
        console.error('❌ [OrderOperations] checkAndUpdateTableStatusAjax() - Error:', error);
        return false;
    }
}

// 🎯 FUNCIÓN MEJORADA: Verificar si la orden está vacía y actualizar mesa usando método existente
async function checkAndUpdateTableIfOrderEmpty() {
    console.log('🔍 ENTRADA: checkAndUpdateTableIfOrderEmpty()');
    try {
        console.log('🔍 [OrderOperations] checkAndUpdateTableIfOrderEmpty() - Verificando si orden está vacía...');
        
        if (!currentOrder || !currentOrder.orderId) {
            console.log('⚠️ [OrderOperations] checkAndUpdateTableIfOrderEmpty() - No hay orden activa');
            return;
        }
        
        // Verificar si la orden tiene items
        const hasItems = currentOrder.items && currentOrder.items.length > 0;
        console.log('📊 [OrderOperations] checkAndUpdateTableIfOrderEmpty() - Items en orden:', currentOrder.items ? currentOrder.items.length : 0);
        
        if (!hasItems) {
            console.log('🚀 [OrderOperations] checkAndUpdateTableIfOrderEmpty() - ORDEN VACÍA - Verificando estado de mesa');
            
            // Usar el método existente para verificar y actualizar el estado de la mesa
            const success = await checkAndUpdateTableStatusAjax(currentOrder.orderId);
            
            if (success) {
                // Mostrar notificación
                Swal.fire({
                    title: 'Mesa Liberada',
                    text: 'La mesa ha sido liberada automáticamente',
                    icon: 'info',
                    timer: 2000,
                    showConfirmButton: false,
                    toast: true,
                    position: 'top-end'
                });
                
                // Limpiar la orden actual
                currentOrder = { items: [], total: 0, tableId: null };
                updateOrderUI();
                clearOrderUI();
            }
        }
    } catch (error) {
        console.error('❌ [OrderOperations] checkAndUpdateTableIfOrderEmpty() - Error:', error);
    }
}

// Función para enviar orden a cocina
async function sendToKitchen() {
    try {
        console.log('🔍 [OrderOperations] sendToKitchen() - Iniciando envío a cocina...');
        
        // Validar que hay una mesa seleccionada
        if (!currentOrder || !currentOrder.tableId) {
            console.log('❌ [OrderOperations] sendToKitchen() - No hay mesa seleccionada');
            Swal.fire('Error', 'Debes seleccionar una mesa antes de enviar la orden', 'error');
            return;
        }
        
        // Validar que hay items en la orden
        if (!currentOrder.items || currentOrder.items.length === 0) {
            console.log('❌ [OrderOperations] sendToKitchen() - No hay items en la orden');
            Swal.fire('Error', 'Debes agregar al menos un producto a la orden', 'error');
            return;
        }
        
        // ✅ NUEVO: Verificar si el usuario es admin y mostrar modal para seleccionar estación
        const userRole = await getCurrentUserRole();
        console.log('🔍 [OrderOperations] sendToKitchen() - Rol del usuario:', userRole);
        
        if (userRole === 'admin' || userRole === 'superadmin') {
            // Mostrar modal para seleccionar estación
            await showStationSelectionModal();
            return;
        }
        
        // Si no es admin, continuar con el flujo normal
        await sendOrderToKitchen(null);
    } catch (error) {
        console.error('❌ [OrderOperations] sendToKitchen() - Error:', error);
        Swal.close();
        Swal.fire('Error', 'Error al enviar la orden a cocina', 'error');
    }
}

// ✅ NUEVO: Función para obtener el rol del usuario actual
async function getCurrentUserRole() {
    try {
        // Intentar obtener desde window.currentUser si está disponible
        if (typeof window.currentUser !== 'undefined' && window.currentUser && window.currentUser.role) {
            return window.currentUser.role.toLowerCase();
        }
        
        // Si no está disponible, obtener desde la API
        if (typeof getCurrentUser === 'function') {
            const user = await getCurrentUser();
            if (user && user.role) {
                return user.role.toLowerCase();
            }
        }
        
        // Fallback: obtener desde la API directamente
        const response = await fetch('/Auth/CurrentUser', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        if (response.ok) {
            const result = await response.json();
            if (result.success && result.user && result.user.role) {
                return result.user.role.toLowerCase();
            }
        }
        
        return null;
    } catch (error) {
        console.error('❌ [OrderOperations] getCurrentUserRole() - Error:', error);
        return null;
    }
}

// ✅ NUEVO: Función para mostrar el modal de selección de estación
async function showStationSelectionModal() {
    try {
        console.log('🔍 [OrderOperations] showStationSelectionModal() - Cargando estaciones...');
        
        // Cargar estaciones disponibles
        const response = await fetch('/Station/GetStations');
        const result = await response.json();
        
        if (!result.success) {
            console.error('❌ [OrderOperations] showStationSelectionModal() - Error al cargar estaciones:', result.message);
            Swal.fire('Error', 'No se pudieron cargar las estaciones', 'error');
            return;
        }
        
        const stations = result.data.$values || result.data || [];
        console.log('✅ [OrderOperations] showStationSelectionModal() - Estaciones cargadas:', stations.length);
        
        // Llenar el select de estaciones
        const stationSelect = document.getElementById('stationSelect');
        if (!stationSelect) {
            console.error('❌ [OrderOperations] showStationSelectionModal() - Select de estaciones no encontrado');
            return;
        }
        
        stationSelect.innerHTML = '<option value="">Seleccione una estación...</option>';
        stations.forEach(station => {
            const option = document.createElement('option');
            option.value = station.id;
            option.textContent = `${station.name} (${station.type || 'Sin tipo'})`;
            option.dataset.stationType = station.type || '';
            stationSelect.appendChild(option);
        });
        
        // Event listener para mostrar información de la estación seleccionada
        stationSelect.addEventListener('change', function() {
            const selectedOption = this.options[this.selectedIndex];
            const stationInfo = document.getElementById('stationInfo');
            const stationDetails = document.getElementById('stationDetails');
            
            if (this.value && selectedOption) {
                stationInfo.style.display = 'block';
                stationDetails.innerHTML = `
                    <p class="mb-1"><strong>Nombre:</strong> ${selectedOption.textContent}</p>
                    <p class="mb-0"><strong>Tipo:</strong> ${selectedOption.dataset.stationType || 'No especificado'}</p>
                `;
            } else {
                stationInfo.style.display = 'none';
            }
        });
        
        // Mostrar el modal
        const modal = new bootstrap.Modal(document.getElementById('selectStationModal'));
        modal.show();
        
        // Guardar referencia al modal para usarla después
        window.stationSelectionModal = modal;
    } catch (error) {
        console.error('❌ [OrderOperations] showStationSelectionModal() - Error:', error);
        Swal.fire('Error', 'Error al cargar el modal de selección de estación', 'error');
    }
}

// ✅ NUEVO: Función para confirmar la selección de estación y enviar la orden
async function confirmStationSelection() {
    const stationSelect = document.getElementById('stationSelect');
    
    if (!stationSelect || !stationSelect.value) {
        Swal.fire('Error', 'Debes seleccionar una estación', 'error');
        return;
    }
    
    const selectedStationId = stationSelect.value;
    console.log('✅ [OrderOperations] confirmStationSelection() - Estación seleccionada:', selectedStationId);
    
    // Cerrar el modal
    if (window.stationSelectionModal) {
        window.stationSelectionModal.hide();
    }
    
    // Enviar la orden con la estación seleccionada
    await sendOrderToKitchen(selectedStationId);
}

// ✅ NUEVO: Función para enviar la orden a cocina (refactorizada)
async function sendOrderToKitchen(selectedStationId) {
    try {
        console.log('🔍 [OrderOperations] sendOrderToKitchen() - Enviando orden...', { selectedStationId });
        
        // Preparar datos para enviar (según SendOrderDto)
        const orderData = {
            TableId: currentOrder.tableId,
            OrderType: 'DineIn', // Tipo de orden por defecto
            Items: currentOrder.items.map(item => ({
                Id: item.id || '00000000-0000-0000-0000-000000000000', // Guid.empty para items nuevos
                ProductId: item.productId,
                Quantity: item.quantity,
                Notes: item.notes || '',
                Discount: item.discount || 0,
                Status: item.status || 'Pending'
            })),
            SelectedStationId: selectedStationId || null // ✅ NUEVO: Estación seleccionada manualmente
        };
        
        console.log('🔍 [OrderOperations] sendOrderToKitchen() - Datos a enviar:', orderData);
        
        // Mostrar loading
        Swal.fire({
            title: 'Enviando a cocina...',
            allowOutsideClick: false,
            didOpen: () => { Swal.showLoading(); }
        });
        
        // Enviar petición al backend
        const response = await fetch('/Order/SendToKitchen', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(orderData)
        });
        
        console.log('📡 [OrderOperations] sendOrderToKitchen() - Respuesta recibida:', response.status, response.statusText);
        
        if (response.ok) {
            const result = await response.json();
            console.log('✅ [OrderOperations] sendOrderToKitchen() - Resultado:', result);
            
            // Actualizar la orden actual con la respuesta
            if (result.orderId) {
                currentOrder.orderId = result.orderId;
                currentOrder.status = result.status;
                
                // Unirse a grupos de SignalR
                if (typeof joinSignalRGroups === 'function') {
                    await joinSignalRGroups(currentOrder.tableId, currentOrder.orderId);
                }
                
                // Actualizar UI
                updateOrderUI();
                
                // Cambiar texto del botón
                const sendButton = document.getElementById('sendToKitchen');
                if (sendButton) {
                    sendButton.textContent = 'Agregar a Cocina';
                }
            }
            
            Swal.close();
            Swal.fire({
                title: '¡Orden Enviada!',
                text: result.message || 'La orden ha sido enviada a cocina exitosamente',
                icon: 'success',
                timer: 2000,
                showConfirmButton: false
            });
            
            console.log('✅ [OrderOperations] sendOrderToKitchen() - Orden enviada exitosamente');
        } else {
            const errorResult = await response.json();
            console.log('❌ [OrderOperations] sendOrderToKitchen() - Error:', errorResult);
            
            Swal.close();
            Swal.fire('Error', errorResult.error || 'Error al enviar la orden a cocina', 'error');
        }
    } catch (error) {
        console.error('❌ [OrderOperations] sendOrderToKitchen() - Error:', error);
        Swal.close();
        Swal.fire('Error', 'Error al enviar la orden a cocina', 'error');
    }
}

// Evitar doble envío de cancelación
let cancelProcessing = false;

// Función para cancelar orden
async function cancelOrder() {
    if (cancelProcessing) {
        Swal.fire('Espere', 'La cancelación ya está en proceso', 'info');
        return;
    }
    if (!currentOrder || !currentOrder.orderId) {
        Swal.fire('Error', 'No hay una orden activa para cancelar', 'error');
        return;
    }

    const result = await Swal.fire({
        title: '¿Cancelar orden?',
        text: '¿Estás seguro de que deseas cancelar esta orden? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, cancelar',
        cancelButtonText: 'No, mantener'
    });

    if (result.isConfirmed) {
        cancelProcessing = true;
        try {
            const response = await fetch('/Order/Cancel', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    OrderId: currentOrder.orderId,
                    Reason: 'Cancelación solicitada por el usuario',
                    UserId: null,
                    SupervisorId: null
                })
            });

            if (response.ok) {
                const result = await response.json();
                
                if (result.success) {
                    Swal.fire({
                        title: 'Orden Cancelada',
                        text: 'La orden ha sido cancelada exitosamente',
                        icon: 'success',
                        timer: 2000,
                        showConfirmButton: false
                    });

                    // 🎯 LOG ESTRATÉGICO: ORDEN CANCELADA
                    console.log('🚀 [OrderOperations] cancelOrder() - ORDEN CANCELADA - Limpiando orden actual');
                    
                    // Guardar información de la mesa antes de limpiar
                    const tableId = currentOrder.tableId;
                    
                    // Limpiar la orden actual
                    currentOrder = { items: [], total: 0, tableId: null };
                    updateOrderUI();
                    clearOrderUI();
                    
                    // 🔄 ACTUALIZAR ESTADO DE LA MESA A DISPONIBLE usando método existente
                    if (tableId && typeof updateTableStatus === 'function') {
                        console.log('🔄 [OrderOperations] cancelOrder() - Actualizando mesa a Disponible:', tableId);
                        updateTableStatus(tableId, 'Disponible');
                    }
                    
                    // 📡 NOTIFICAR VIA SIGNALR (se maneja automáticamente en el backend)
                    console.log('📡 [OrderOperations] cancelOrder() - Notificación SignalR enviada desde backend');
                } else {
                    Swal.fire('Error', result.message || 'Error al cancelar la orden', 'error');
                }
            } else {
                let msg = 'Error al cancelar la orden';
                try {
                    const err = await response.json();
                    if (err && err.message) msg = err.message;
                } catch (_) {}
                Swal.fire('Error', msg, 'error');
            }
        } catch (error) {
            Swal.fire('Error', error.message || 'Error de conexión al cancelar la orden', 'error');
        } finally {
            cancelProcessing = false;
        }
    }
}

// Función para actualizar cantidad de un item
async function updateQuantity(productId, delta, status = null) {
    if (!currentOrder || !currentOrder.items) {
        return;
    }

    const item = currentOrder.items.find(i => i.productId === productId);
    if (!item) {
        return;
    }

    const newQuantity = item.quantity + delta;

    if (newQuantity <= 0) {
        const result = await Swal.fire({
            title: '¿Eliminar item?',
            text: `¿Deseas eliminar ${item.productName} de la orden?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'No, mantener'
        });

        if (result.isConfirmed) {
            await removeItem(item.id);
        }
    } else {
        if (currentOrder.orderId && item.isFromBackend) {
            // Es una orden existente, actualizar en el servidor
            try {
                const requestData = {
                    orderId: currentOrder.orderId,
                    productId: productId,
                    quantity: newQuantity,  // ✅ Corregido: el DTO espera "quantity" no "newQuantity"
                    itemId: item.id || null  // ✅ Agregado: ItemId para actualización precisa
                };

                const response = await fetch('/Order/UpdateItemQuantity', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(requestData)
                });

                if (response.ok) {
                    const result = await response.json();
                    
                    if (result.success) {
                        item.quantity = newQuantity;
                        updateOrderUI();
                        
                        if (result.orderDeleted) {
                            // La orden fue eliminada completamente
                            currentOrder = { items: [], total: 0, tableId: null };
                            updateOrderUI();
                            clearOrderUI();
                        }
                    } else {
                        Swal.fire('Error', result.message || 'Error al actualizar cantidad', 'error');
                    }
                } else {
                    Swal.fire('Error', 'Error al actualizar cantidad en el servidor', 'error');
                }
            } catch (error) {
                Swal.fire('Error', 'Error de conexión al actualizar cantidad', 'error');
            }
        } else {
            // Es una orden nueva, actualizar localmente
            item.quantity = newQuantity;
            updateOrderUI();
        }
    }
}

// Función para eliminar un item de la orden
async function removeItem(itemId) {
    console.log('🔍 ENTRADA: removeItem() - itemId:', itemId);
    if (!currentOrder || !currentOrder.items) {
        return;
    }

    const item = currentOrder.items.find(i => i.id === itemId);
    if (!item) {
        return;
    }

    // Verificar si hay otros items con el mismo ProductId
    const itemsWithSameProduct = currentOrder.items.filter(i => i.productId === item.productId);
    
    // Mostrar todos los items en la orden para debug
    currentOrder.items.forEach((orderItem, index) => {
        // Debug info
    });

    const result = await Swal.fire({
        title: '¿Eliminar item?',
        text: `¿Estás seguro de que deseas eliminar ${item.productName} de la orden?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'No, mantener'
    });

    if (!result.isConfirmed) {
        return;
    }

    // Análisis del item para determinar la estrategia de eliminación
    const itemAnalysis = {
        id: item.id,
        isNew: item.isNew,
        isFromBackend: item.isFromBackend,
        status: item.status,
        orderStatus: currentOrder.status,
        orderId: currentOrder.orderId
    };

    if (item.isNew) {
        // ESCENARIO 1: Item NUEVO detectado (isNew = true), eliminando solo del frontend
        currentOrder.items = currentOrder.items.filter(i => i.id !== itemId);
        updateOrderUI();
        
        if (currentOrder.items.length === 0) {
            disableConfirmButton();
        }
    } else if (item.isFromBackend && currentOrder.orderId) {
        // ESCENARIO 2: Item EXISTENTE detectado (isFromBackend = true), llamando al backend para eliminar
        try {
            const requestData = {
                orderId: currentOrder.orderId,
                itemId: item.id,
                productId: item.productId,
                status: item.status
            };

            const response = await fetch('/Order/RemoveItemFromOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });

            if (response.ok) {
                const result = await response.json();
                
                if (result.success) {
                    if (result.orderDeleted) {
                        // La orden fue eliminada completamente
                        console.log('🚀 [OrderOperations] removeItem() - ORDEN ELIMINADA COMPLETAMENTE - Mesa liberada');
                        
                        // Limpiar la orden actual
                        currentOrder = { items: [], total: 0, tableId: null };
                        updateOrderUI();
                        clearOrderUI();
                        
                        // 📡 NOTIFICAR VIA SIGNALR (se maneja automáticamente en el backend)
                        console.log('📡 [OrderOperations] removeItem() - Notificación SignalR enviada desde backend');
                        
                        Swal.fire({
                            title: 'Orden Eliminada',
                            text: 'La orden fue eliminada completamente y la mesa está disponible',
                            icon: 'info',
                            timer: 3000,
                            showConfirmButton: false
                        });
                    } else {
                        // Solo se eliminó el item - verificar si la orden quedó vacía
                        currentOrder.items = currentOrder.items.filter(i => i.id !== itemId);
                        updateOrderUI();
                        
                        // 🎯 NUEVO: Verificar si la orden quedó vacía y actualizar mesa
                        await checkAndUpdateTableIfOrderEmpty();
                        
                        Swal.fire({
                            title: 'Item Eliminado',
                            text: `${item.productName} ha sido eliminado de la orden`,
                            icon: 'success',
                            timer: 2000,
                            showConfirmButton: false
                        });
                    }
                } else {
                    Swal.fire('Error', result.message || 'Error al eliminar el item', 'error');
                }
            } else {
                Swal.fire('Error', 'Error al eliminar el item del servidor', 'error');
            }
        } catch (error) {
            Swal.fire('Error', 'Error de conexión al eliminar el item', 'error');
        }
    } else if (!currentOrder.orderId) {
        // ESCENARIO 3: Orden NUEVA detectada, eliminando solo del frontend
        currentOrder.items = currentOrder.items.filter(i => i.id !== itemId);
        updateOrderUI();
        
        if (currentOrder.items.length === 0) {
            console.log('🚀 [OrderOperations] removeItem() - ORDEN NUEVA VACÍA - Mesa liberada');
            
            // Guardar información de la mesa antes de limpiar
            const tableId = currentOrder.tableId;
            
            // Limpiar la orden actual
            currentOrder = { items: [], total: 0, tableId: null };
            updateOrderUI();
            clearOrderUI();
            
            // 🔄 ACTUALIZAR ESTADO DE LA MESA A DISPONIBLE
            if (tableId && typeof updateTableStatus === 'function') {
                console.log('🔄 [OrderOperations] removeItem() - Actualizando mesa a Disponible:', tableId);
                updateTableStatus(tableId, 'Disponible');
            }
            
            disableConfirmButton();
        }
    } else if (item.isFromBackend && currentOrder.status === 'SentToKitchen') {
        // ESCENARIO 5: Item EXISTENTE en orden enviada a cocina
        try {
            const requestData = {
                orderId: currentOrder.orderId,
                itemId: item.id,
                productId: item.productId,
                status: item.status
            };

            const response = await fetch('/Order/RemoveItemFromOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });

            if (response.ok) {
                const result = await response.json();
                
                if (result.success) {
                    if (result.orderDeleted) {
                        currentOrder = { items: [], total: 0, tableId: null };
                        updateOrderUI();
                        clearOrderUI();
                    } else {
                        currentOrder.items = currentOrder.items.filter(i => i.id !== itemId);
                        updateOrderUI();
                    }
                    
                    Swal.fire({
                        title: 'Item Eliminado',
                        text: `${item.productName} ha sido eliminado de la orden`,
                        icon: 'success',
                        timer: 2000,
                        showConfirmButton: false
                    });
                } else {
                    Swal.fire('Error', result.message || 'Error al eliminar el item', 'error');
                }
            } else {
                Swal.fire('Error', 'Error al eliminar el item del servidor', 'error');
            }
        } catch (error) {
            Swal.fire('Error', 'Error de conexión al eliminar el item', 'error');
        }
    }
}

// Exportar funciones para uso global
window.cancelOrder = cancelOrder;
window.updateQuantity = updateQuantity;
window.removeItem = removeItem; 