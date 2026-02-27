// ✅ NUEVA: Función para calcular desglose de impuestos del pedido
console.log('✅ [OrderUI] order-ui.js cargado correctamente');

function calculateOrderTaxBreakdown() {
    if (!currentOrder || !currentOrder.items) {
        return { subtotal: 0, totalTax: 0, totalWithTax: 0 };
    }

    let subtotal = 0;
    let totalTax = 0;

    currentOrder.items.forEach(item => {
        const itemSubtotal = item.price * item.quantity;
        const taxRate = item.taxRate || 0;
        const itemTax = itemSubtotal * (taxRate / 100);
        
        subtotal += itemSubtotal;
        totalTax += itemTax;
    });

    const totalWithTax = subtotal + totalTax;

    return {
        subtotal: subtotal,
        totalTax: totalTax,
        totalWithTax: totalWithTax
    };
}

// ✅ Función para asegurar que todos los items tengan parámetros correctos
function ensureItemParameters() {
    if (!currentOrder?.items) return;
    
    currentOrder.items.forEach((item, index) => {
        // ✅ Si el item no tiene isNew, establecerlo basado en si tiene ID real
        if (!item.hasOwnProperty('isNew')) {
            const hasRealId = item.id && item.id !== 'new' && item.id !== null && item.id.length === 36;
            item.isNew = !hasRealId;
        }
        
        // ✅ Si el item no tiene isFromBackend, establecerlo basado en si tiene ID real
        if (!item.hasOwnProperty('isFromBackend')) {
            const hasRealId = item.id && item.id !== 'new' && item.id !== null && item.id.length === 36;
            item.isFromBackend = hasRealId;
        }
    });
}

// Order UI Management - Funciones para manejar la interfaz de usuario de órdenes

// Función para actualizar la interfaz de usuario de la orden
function updateOrderUI() {
    const orderItemsContainer = document.getElementById('orderItems');
    const orderTotalElement = document.getElementById('orderTotal');
    const itemCountElement = document.getElementById('itemCount');
    const orderSubtotalElement = document.getElementById('orderSubtotal');
    const orderTaxElement = document.getElementById('orderTax');
    const orderDiscountElement = document.getElementById('orderDiscount');
    
    if (!orderItemsContainer || !orderTotalElement || !itemCountElement) {
        return;
    }

    // Limpiar contenedor
    orderItemsContainer.innerHTML = '';

    // Verificar si hay items para mostrar
    if (!currentOrder || !currentOrder.items || currentOrder.items.length === 0) {
        // Limpiar resumen de pagos si no hay items
        const paymentSummary = document.getElementById('paymentSummary');
        if (paymentSummary) {
            paymentSummary.innerHTML = '';
        }
        
        // Limpiar campos de totales
        if (orderSubtotalElement) orderSubtotalElement.textContent = '$0.00';
        if (orderTaxElement) orderTaxElement.textContent = '$0.00';
        if (orderDiscountElement) orderDiscountElement.textContent = '$0.00';
        if (orderTotalElement) orderTotalElement.textContent = '$0.00';
        if (itemCountElement) itemCountElement.textContent = '0';
        
        return;
    }

    // ✅ NUEVO: Filtrar items cancelados - no mostrar ni incluir en totales
    const activeItems = currentOrder.items.filter(item => item.status !== 'cancelled' && item.status !== 'Cancelled');
    
    console.log(`🔍 [OrderUI] Total items: ${currentOrder.items.length}, Items activos: ${activeItems.length}, Items cancelados: ${currentOrder.items.length - activeItems.length}`);

    // Ordenar items activos por orden de agregado (mantener orden original)
    const itemsToRender = [...activeItems];

    // Renderizar cada item activo
    itemsToRender.forEach((item, index) => {
        const itemRow = renderItemRow(item, index);
        orderItemsContainer.appendChild(itemRow);
    });

    // ✅ NUEVO: Calcular totales solo con items activos (excluir cancelados)
    const subtotal = activeItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const totalTax = activeItems.reduce((sum, item) => {
        const itemSubtotal = item.price * item.quantity;
        const taxRate = item.taxRate || 0;
        return sum + (itemSubtotal * (taxRate / 100));
    }, 0);
    
    // Obtener descuento actual (si existe)
    const currentDiscount = currentOrder.discount || 0;
    const discountAmount = currentDiscount > 0 ? (subtotal * (currentDiscount / 100)) : 0;
    
    // Calcular total final
    const total = subtotal + totalTax - discountAmount;
    
    // Actualizar elementos de la interfaz
    if (orderSubtotalElement) orderSubtotalElement.textContent = `$${subtotal.toFixed(2)}`;
    if (orderTaxElement) orderTaxElement.textContent = `$${totalTax.toFixed(2)}`;
    if (orderDiscountElement) orderDiscountElement.textContent = `$${discountAmount.toFixed(2)}`;
    if (orderTotalElement) orderTotalElement.textContent = `$${total.toFixed(2)}`;
    
    // ✅ NUEVO: Contar solo items activos (excluir cancelados)
    const itemCount = activeItems.reduce((sum, item) => sum + item.quantity, 0);
    if (itemCountElement) itemCountElement.textContent = `${itemCount} items`;

    // ✅ NUEVO: Habilitar/deshabilitar botón de confirmar solo si hay items activos
    if (activeItems.length > 0) {
        enableConfirmButton();
    } else {
        disableConfirmButton();
    }
    
    // 🎯 LOG ESTRATÉGICO: ACTUALIZAR BOTONES DE PAGO
    console.log('🚀 [OrderUI] updateOrderUI() - ACTUALIZANDO BOTONES DE PAGO - Estado orden:', currentOrder.status);
    updatePaymentButtons();
}

// Función para renderizar una fila de item
function renderItemRow(item, index) {
    const row = document.createElement('tr');
    row.className = 'order-item-row';
    row.setAttribute('data-item-id', item.id);
    row.setAttribute('data-product-id', item.productId);

    const statusBadge = getStatusDisplay(item.status);
    const totalItemPrice = (item.price * item.quantity).toFixed(2);

    row.innerHTML = `
        <td>
            <div class="d-flex align-items-center">
                <span class="badge bg-secondary me-2">${index + 1}</span>
                <div>
                    <div class="fw-bold">${item.productName}</div>
                    <small class="text-muted">ID: ${item.productId}</small>
                </div>
            </div>
        </td>
        <td class="text-center">
            <div class="btn-group btn-group-sm">
                <button type="button" class="btn btn-outline-secondary" onclick="decreaseQuantity('${item.productId}')">
                    <i class="fas fa-minus"></i>
                </button>
                <span class="btn btn-outline-secondary disabled">${item.quantity}</span>
                <button type="button" class="btn btn-outline-secondary" onclick="increaseQuantity('${item.productId}')">
                    <i class="fas fa-plus"></i>
                </button>
            </div>
        </td>
        <td class="text-end">$${item.price.toFixed(2)}</td>
        <td class="text-end fw-bold">$${totalItemPrice}</td>
        <td class="text-center">${statusBadge}</td>
        <td>
            ${item.notes ? `<div class="text-info small"><i class="fas fa-sticky-note"></i> ${item.notes}</div>` : '<span class="text-muted">Sin notas</span>'}
        </td>
        <td class="text-center">
            <button type="button" class="btn btn-outline-danger btn-sm" onclick="removeItem('${item.id}')" title="Eliminar item">
                <i class="fas fa-trash"></i>
            </button>
        </td>
    `;

    return row;
}

// Función para aumentar cantidad de un item
async function increaseQuantity(productId) {
    if (!currentOrder || !currentOrder.items) {
        return;
    }

    const item = currentOrder.items.find(i => i.productId === productId);
    if (!item) {
        return;
    }

    const newQuantity = item.quantity + 1;

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

// Función para disminuir cantidad de un item
async function decreaseQuantity(productId) {
    if (!currentOrder || !currentOrder.items) {
        return;
    }

    const item = currentOrder.items.find(i => i.productId === productId);
    if (!item) {
        return;
    }

    const newQuantity = item.quantity - 1;

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

// Función para habilitar botón de confirmar
function enableConfirmButton() {
    const confirmButton = document.getElementById('sendToKitchen');
    if (confirmButton) {
        confirmButton.disabled = false;
        confirmButton.classList.remove('btn-secondary');
        confirmButton.classList.add('btn-primary');
    }
}

// Función para deshabilitar botón de confirmar
function disableConfirmButton() {
    const confirmButton = document.getElementById('sendToKitchen');
    if (confirmButton) {
        confirmButton.disabled = true;
        confirmButton.classList.remove('btn-primary');
        confirmButton.classList.add('btn-secondary');
    }
}

// Función para limpiar la interfaz de orden
function clearOrderUI() {
    const orderItemsContainer = document.getElementById('orderItems');
    const orderTotalElement = document.getElementById('orderTotal');
    const itemCountElement = document.getElementById('itemCount');
    
    if (orderItemsContainer) {
        orderItemsContainer.innerHTML = '';
    }
    
    if (orderTotalElement) {
        orderTotalElement.textContent = '$0.00';
    }
    
    if (itemCountElement) {
        itemCountElement.textContent = '0';
    }
    
    disableConfirmButton();
}

// 🎯 FUNCIÓN ESTRATÉGICA: ACTUALIZAR BOTONES DE PAGO SEGÚN ESTADO
// ✅ MEJORADO: Permite pagar en cualquier momento que haya una orden activa
function updatePaymentButtons() {
    try {
        console.log('🔍 [OrderUI] updatePaymentButtons() - Iniciando actualización de botones de pago...');
        
        const separateAccountsBtn = document.getElementById('separateAccountsBtn');
        const partialPaymentBtn = document.getElementById('partialPaymentBtn');
        const paymentHistoryBtn = document.getElementById('paymentHistoryBtn');
        const cancelOrderBtn = document.getElementById('cancelOrder');
        
        if (!partialPaymentBtn || !paymentHistoryBtn) {
            console.warn('⚠️ [OrderUI] updatePaymentButtons() - Botones de pago no encontrados en el DOM');
            return;
        }
        
        // ✅ MEJORADO: Verificar si hay una orden activa con items
        const hasActiveOrder = currentOrder && currentOrder.orderId && currentOrder.items && currentOrder.items.length > 0;
        
        // Verificar estado de la orden
        const orderStatus = currentOrder?.status;
        const isCancelled = orderStatus === 'Cancelled' || orderStatus === 'cancelled' || orderStatus === 'Completed';
        const isCompleted = orderStatus === 'Completed' || orderStatus === 'completed';
        
        // ✅ MEJORADO: Permitir pago en cualquier momento mientras haya una orden activa
        // Solo ocultar si la orden está cancelada o completada
        const canPay = hasActiveOrder && !isCancelled && !isCompleted;
        
        console.log('📋 [OrderUI] updatePaymentButtons() - Estado verificado:', {
            hasActiveOrder,
            orderStatus,
            isCancelled,
            isCompleted,
            canPay,
            itemsCount: currentOrder?.items?.length || 0
        });
        
        // ✅ MEJORADO: Mostrar botones de pago siempre que haya una orden activa y no esté cancelada/completada
        if (canPay) {
            // 🎯 LOG ESTRATÉGICO: BOTONES DE PAGO HABILITADOS - Pago disponible en cualquier momento
            console.log('🚀 [OrderUI] updatePaymentButtons() - BOTONES DE PAGO HABILITADOS - Pago disponible');
            
            // Mostrar botón de cuentas separadas siempre que haya una orden activa
            if (separateAccountsBtn) {
                separateAccountsBtn.style.display = '';
            }
            
            partialPaymentBtn.style.display = '';
            paymentHistoryBtn.style.display = '';
            
            // Mostrar botón de cancelar solo si la orden no está completada
            if (cancelOrderBtn && !isCompleted) {
                cancelOrderBtn.style.display = '';
            } else if (cancelOrderBtn) {
                cancelOrderBtn.style.display = 'none';
            }
        } else {
            // Ocultar botones si no se cumplen las condiciones
            if (separateAccountsBtn) {
                separateAccountsBtn.style.display = 'none';
            }
            
            partialPaymentBtn.style.display = 'none';
            paymentHistoryBtn.style.display = 'none';
            
            if (cancelOrderBtn) {
                cancelOrderBtn.style.display = 'none';
            }
            
            console.log('🔒 [OrderUI] updatePaymentButtons() - Botones de pago ocultos - Orden cancelada, completada o sin items');
        }
        
    } catch (error) {
        console.error('❌ [OrderUI] updatePaymentButtons() - Error:', error);
    }
}

// Exportar funciones para uso global
window.updateOrderUI = updateOrderUI;
window.increaseQuantity = increaseQuantity;
window.decreaseQuantity = decreaseQuantity;
window.enableConfirmButton = enableConfirmButton;
window.disableConfirmButton = disableConfirmButton;
window.clearOrderUI = clearOrderUI;
window.updatePaymentButtons = updatePaymentButtons; 