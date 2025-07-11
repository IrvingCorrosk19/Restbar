// ✅ Función para asegurar que todos los items tengan parámetros correctos
function ensureItemParameters() {
    if (!currentOrder?.items) return;
    
    console.log('[Frontend] ensureItemParameters iniciado');
    
    currentOrder.items.forEach((item, index) => {
        // ✅ Si el item no tiene isNew, establecerlo basado en si tiene ID real
        if (!item.hasOwnProperty('isNew')) {
            const hasRealId = item.id && item.id !== 'new' && item.id !== null && item.id.length === 36;
            item.isNew = !hasRealId;
            console.log(`[Frontend] Item ${index + 1} (${item.productName}): isNew establecido a ${item.isNew} (ID: ${item.id})`);
        }
        
        // ✅ Si el item no tiene isFromBackend, establecerlo basado en si tiene ID real
        if (!item.hasOwnProperty('isFromBackend')) {
            const hasRealId = item.id && item.id !== 'new' && item.id !== null && item.id.length === 36;
            item.isFromBackend = hasRealId;
            console.log(`[Frontend] Item ${index + 1} (${item.productName}): isFromBackend establecido a ${item.isFromBackend} (ID: ${item.id})`);
        }
    });
    
    console.log('[Frontend] ensureItemParameters completado');
}

// Order UI Management

// Actualizar UI de la orden con CRUD inline
function updateOrderUI() {
    console.log('[Frontend] updateOrderUI iniciado');
    console.log('[Frontend] currentOrder:', currentOrder);
    console.log('[Frontend] currentOrder.items:', currentOrder?.items);
    
    // ✅ Asegurar que currentOrder.status tenga un valor por defecto
    if (!currentOrder.status) {
        currentOrder.status = 'Pending';
        console.log('[Frontend] Estado de orden establecido por defecto a:', currentOrder.status);
    }
    
    // ✅ Asegurar que todos los items tengan parámetros correctos
    ensureItemParameters();
    
    // 🔍 LOG DETALLADO DE CADA ITEM
    if (currentOrder?.items) {
        console.log('[Frontend] === DETALLE DE ITEMS RECIBIDOS ===');
        currentOrder.items.forEach((item, index) => {
            console.log(`[Frontend] Item ${index + 1}:`);
            console.log(`[Frontend]   - ID: ${item.id || 'NO ID'}`);
            console.log(`[Frontend]   - ProductId: ${item.productId}`);
            console.log(`[Frontend]   - ProductName: ${item.productName}`);
            console.log(`[Frontend]   - Quantity: ${item.quantity}`);
            console.log(`[Frontend]   - Status: ${item.status}`);
            console.log(`[Frontend]   - KitchenStatus: ${item.kitchenStatus}`);
            console.log(`[Frontend]   - PreparedAt: ${item.preparedAt || 'NO'}`);
            console.log(`[Frontend]   - isNew: ${item.isNew}`);
            console.log(`[Frontend]   - isFromBackend: ${item.isFromBackend}`);
        });
        console.log('[Frontend] === FIN DETALLE ===');
        
        // ✅ VERIFICACIÓN DE PARÁMETROS CRÍTICOS
        console.log('[Frontend] === VERIFICACIÓN DE PARÁMETROS ===');
        currentOrder.items.forEach((item, index) => {
            const hasIsNew = item.hasOwnProperty('isNew');
            const hasIsFromBackend = item.hasOwnProperty('isFromBackend');
            const isNewValue = item.isNew;
            const isFromBackendValue = item.isFromBackend;
            
            console.log(`[Frontend] Item ${index + 1} (${item.productName}):`);
            console.log(`[Frontend]   - Tiene isNew: ${hasIsNew} (valor: ${isNewValue})`);
            console.log(`[Frontend]   - Tiene isFromBackend: ${hasIsFromBackend} (valor: ${isFromBackendValue})`);
            
            if (!hasIsNew || !hasIsFromBackend) {
                console.warn(`[Frontend] ⚠️ Item ${index + 1} no tiene parámetros completos!`);
            }
        });
        console.log('[Frontend] === FIN VERIFICACIÓN ===');
    }
    
    const orderItemsTbody = document.getElementById('orderItems');
    orderItemsTbody.innerHTML = '';
    if (!currentOrder.items || currentOrder.items.length === 0) {
        document.getElementById('orderTotal').textContent = '$0.00';
        document.getElementById('itemCount').textContent = '0 items';
        
        // Limpiar resumen de pagos cuando no hay items
        if (typeof clearPaymentSummary === 'function') {
            clearPaymentSummary();
            console.log('[Frontend] Resumen de pagos limpiado al no tener items');
        }
        
        console.log('[Frontend] No hay items para mostrar');
        return;
    }

    // ✅ Mantener orden original pero con agrupamiento visual
    const itemsToRender = [...currentOrder.items];
    console.log('[Frontend] Items en orden original para renderizar:', itemsToRender.length);
    
    // ✅ Orden de grupos visuales (Pendientes primero, luego Enviados, etc.)
    const groupOrder = [
        { status: 'Pending', label: '⏳ Pendientes de cocina', class: 'table-warning' },
        { status: 'Sent', label: '🔄 Enviados a cocina', class: 'table-info' },
        { status: 'Ready', label: '✅ Listos', class: 'table-success' },
        { status: 'Cancelled', label: '❌ Cancelados', class: 'table-secondary' }
    ];
    
    let total = 0;
    let itemCount = 0;
    let currentGroup = null;
    
    // ✅ Renderizar items en orden original pero con encabezados de grupo
    itemsToRender.forEach((item, index) => {
        console.log(`[Frontend] Renderizando item ${index + 1} en orden original:`, item.productName);
        
        // ✅ Determinar el grupo visual del item actual
        const itemStatus = item.kitchenStatus || item.status;
        const itemGroup = groupOrder.find(g => g.status === itemStatus);
        
        // ✅ Agregar encabezado de grupo solo si es diferente al anterior
        if (itemGroup && itemGroup !== currentGroup) {
            const groupRow = document.createElement('tr');
            groupRow.className = itemGroup.class;
            groupRow.innerHTML = `<td colspan="7"><strong>${itemGroup.label}</strong></td>`;
            orderItemsTbody.appendChild(groupRow);
            currentGroup = itemGroup;
        }
        
        // ✅ Renderizar el item con su grupo visual
        renderItemRow(item, itemGroup, orderItemsTbody);
        total += item.price * item.quantity;
        itemCount += item.quantity;
    });
    
    document.getElementById('orderTotal').textContent = `$${total.toFixed(2)}`;
    document.getElementById('itemCount').textContent = `${itemCount} items`;
    console.log('[Frontend] updateOrderUI completado - Total:', total, 'Items:', itemCount);
}

// Actualizar total de la orden
function updateOrderTotal() {
    currentOrder.total = currentOrder.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
}

// Habilitar/deshabilitar botón de confirmar
function enableConfirmButton() {
    const btn = document.getElementById('sendToKitchen');
    const cancelBtn = document.getElementById('cancelOrder');
    
    // ✅ Asegurar que currentOrder.status tenga un valor por defecto
    if (!currentOrder.status) {
        currentOrder.status = 'Pending';
        console.log('[enableConfirmButton] Estado de orden establecido por defecto a:', currentOrder.status);
    }
    
    btn.disabled = !currentOrder.tableId || currentOrder.items.length === 0;
    if (!btn.disabled) {
        btn.classList.remove('btn-secondary');
        btn.classList.add('btn-success');
    } else {
        btn.classList.remove('btn-success');
        btn.classList.add('btn-secondary');
    }
    
    // Mostrar botón de cancelar solo si hay una orden activa
    if (currentOrder.orderId && currentOrder.status) {
        cancelBtn.style.display = 'inline-block';
    } else {
        cancelBtn.style.display = 'none';
    }
}

function disableConfirmButton() {
    const btn = document.getElementById('sendToKitchen');
    const cancelBtn = document.getElementById('cancelOrder');
    
    btn.disabled = true;
    cancelBtn.style.display = 'none';
}

// Función para mostrar resumen de la orden
function showOrderSummary(order) {
    let itemsHtml = '';
    let readyItems = 0;
    let pendingItems = 0;
    let preparingItems = 0;
    
    order.items.forEach(item => {
        const statusColor = getStatusColor(item.status);
        const statusText = getStatusDisplayText(item.status);
        const stationInfo = item.preparedByStation ? ` - ${item.preparedByStation}` : '';
        const timeInfo = item.preparedAt ? ` (${new Date(item.preparedAt).toLocaleTimeString()})` : '';
        
        // Contar items por estado
        if (item.status === 'Ready') readyItems++;
        else if (item.status === 'Pending') pendingItems++;
        else if (item.status === 'Preparing') preparingItems++;
        
        itemsHtml += `
            <div class="order-item-summary">
                <div class="d-flex justify-content-between align-items-center">
                    <span>${item.quantity}x ${item.productName}</span>
                    <span class="badge ${statusColor}">${statusText}${stationInfo}${timeInfo}</span>
                </div>
                <div class="text-end text-muted">$${(item.price * item.quantity).toFixed(2)}</div>
            </div>
        `;
    });
    
    // Crear resumen del estado
    let statusSummary = '';
    if (readyItems > 0) statusSummary += `<span class="badge bg-success me-1">${readyItems} listos</span>`;
    if (preparingItems > 0) statusSummary += `<span class="badge bg-warning me-1">${preparingItems} preparando</span>`;
    if (pendingItems > 0) statusSummary += `<span class="badge bg-secondary me-1">${pendingItems} pendientes</span>`;
    
    return `
        <div class="order-summary">
            <div class="mb-3">
                <strong>Estado de la orden:</strong> 
                <span class="badge ${getStatusColor(order.status)}">${getStatusDisplayText(order.status)}</span>
            </div>
            <div class="mb-3">
                <strong>Resumen de items:</strong><br>
                ${statusSummary}
            </div>
            <div class="mb-3">
                <strong>Items en la orden:</strong>
                <div class="order-items-list mt-2" style="max-height: 200px; overflow-y: auto;">
                    ${itemsHtml}
                </div>
            </div>
            <div class="text-end">
                <strong>Total: $${order.total.toFixed(2)}</strong>
            </div>
            <div class="mt-3 p-2 bg-light rounded">
                <small class="text-muted">
                    <i class="fas fa-info-circle"></i> 
                    Puedes agregar más productos a esta orden. Los items listos se mantendrán y los nuevos se enviarán a cocina.
                </small>
            </div>
        </div>
    `;
}

// Funciones CRUD inline
function updateQuantityInline(rowId, delta) {
    const row = document.getElementById(rowId);
    const input = row.querySelector('.quantity-input');
    const currentValue = parseInt(input.value) || 1;
    const newValue = Math.max(1, Math.min(99, currentValue + delta));
    input.value = newValue;
    updateQuantityFromInput(rowId, newValue);
}

function updateQuantityFromInput(rowId, newValue) {
    const row = document.getElementById(rowId);
    const itemId = row.dataset.itemId;
    const quantity = parseInt(newValue) || 1;
    
    // ✅ Buscar el item específico por ID (no por productId y status)
    const item = currentOrder.items.find(i => i.id === itemId);
    if (item) {
        item.quantity = quantity;
        
        // Actualizar el total del item
        const totalCell = row.querySelector('.item-total');
        if (totalCell) {
            totalCell.textContent = `$${(item.price * quantity).toFixed(2)}`;
        }
        
        // Actualizar el total general
        updateOrderTotal();
        const orderTotal = document.getElementById('orderTotal');
        orderTotal.textContent = `$${currentOrder.total.toFixed(2)}`;
        
        // Marcar como modificado
        markItemAsModified(item);
        
        console.log('[Frontend] Item individual actualizado:', item);
    }
}

function validateQuantity(rowId) {
    const row = document.getElementById(rowId);
    const input = row.querySelector('.quantity-input');
    const value = parseInt(input.value);
    
    if (isNaN(value) || value < 1) {
        input.value = 1;
        updateQuantityFromInput(rowId, 1);
    } else if (value > 99) {
        input.value = 99;
        updateQuantityFromInput(rowId, 99);
    }
}

function updateNotes(rowId, notes) {
    const row = document.getElementById(rowId);
    const productId = row.dataset.productId;
    const status = row.dataset.status;
    
    // Actualizar el item en currentOrder
    const item = currentOrder.items.find(i => i.productId === productId && i.status === status);
    if (item) {
        item.notes = notes;
        markItemAsModified(item);
    }
}

// ✅ Función mejorada para remover item inline - Consistente con removeItem
function removeItemInline(rowId) {
    console.log('[Frontend] removeItemInline iniciado - rowId:', rowId);
    
    const row = document.getElementById(rowId);
    if (!row) {
        console.error('[Frontend] No se encontró la fila:', rowId);
        return;
    }
    
    const productId = row.dataset.productId;
    const status = row.dataset.status;
    const itemId = row.dataset.itemId;
    
    console.log('[Frontend] Datos de la fila:', {
        productId: productId,
        status: status,
        itemId: itemId
    });
    
    // Buscar el item específico
    const item = currentOrder.items.find(i => i.id === itemId);
    if (!item) {
        console.error('[Frontend] No se encontró el item a eliminar');
        Swal.fire('Error', 'No se encontró el item a eliminar', 'error');
        return;
    }
    
    // Usar la función principal removeItem para consistencia
    removeItem(itemId);
}

function duplicateItem(rowId) {
    const row = document.getElementById(rowId);
    const productId = row.dataset.productId;
    const status = row.dataset.status;
    
    const originalItem = currentOrder.items.find(i => i.productId === productId && i.status === status);
    if (originalItem) {
        const newItem = {
            ...originalItem,
            id: null, // Nuevo item
            quantity: 1, // Cantidad por defecto
            notes: '', // Notas vacías
            status: 'Pending' // Estado pendiente
        };
        
        currentOrder.items.unshift(newItem); // ✅ CAMBIADO: unshift() en lugar de push() para agregar al principio
        newItems.push(newItem);
        updateOrderUI();
        enableConfirmButton();
        
        Swal.fire({
            title: 'Item Duplicado',
            text: 'El item ha sido duplicado y agregado al pedido',
            icon: 'success',
            timer: 1500,
            showConfirmButton: false
        });
    }
}

function markItemAsModified(item) {
    // Marcar el item como modificado para tracking local
    if (item.id && !modifiedItems.find(m => m.id === item.id)) {
        modifiedItems.push({...item});
    }
}

// Función para renderizar una fila de item individual
function renderItemRow(item, group, tbody) {
    console.log('[renderItemRow] Renderizando item:', item.productName, '| ID:', item.id, '| Status:', item.status);
    
    const row = document.createElement('tr');
    row.className = 'order-item-row';
    row.dataset.itemId = item.id;
    row.dataset.productId = item.productId;
    
    // ✅ MEJORAR LÓGICA DE ESTADOS: Asegurar que currentOrder.status tenga un valor por defecto
    const orderStatus = currentOrder.status || 'Pending';
    console.log('[renderItemRow] Order status:', orderStatus);
    
    // Estados que permiten edición
    const editableOrderStates = ['Pending', 'SentToKitchen'];
    const editableItemStates = ['Pending'];
    
    const canEdit = editableOrderStates.includes(orderStatus) && editableItemStates.includes(item.status);
    console.log('[renderItemRow] Can edit:', canEdit, '| Order status:', orderStatus, '| Item status:', item.status);
    
    // Estado visual
    const statusDisplay = getStatusDisplay(item.status);
    
    // ✅ CONTROLES DE CANTIDAD: REACTIVADOS
    let quantityControls = '';
    if (canEdit) {
        console.log('[renderItemRow] Controles de cantidad HABILITADOS para item:', item.productName);
        quantityControls = `
            <div class="quantity-controls d-flex align-items-center justify-content-center">
                <button class="btn btn-sm btn-outline-secondary" onclick="decreaseQuantity('${item.id}')" title="Disminuir cantidad">
                    <i class="fas fa-minus"></i>
                </button>
                <span class="mx-2 fw-bold quantity-display">${item.quantity}</span>
                <button class="btn btn-sm btn-outline-secondary" onclick="increaseQuantity('${item.id}')" title="Aumentar cantidad">
                    <i class="fas fa-plus"></i>
                </button>
            </div>
        `;
    } else {
        console.log('[renderItemRow] NO se muestran botones de cantidad para item:', item.productName, '| ID:', item.id, '| Order Status:', orderStatus, '| Item Status:', item.status);
        quantityControls = `<span class="fw-bold quantity-display">${item.quantity}</span>`;
    }
    
    // ✅ Aplicar clase de estado visual según el grupo (mantener apariencia anterior)
    if (group) {
        row.className = `order-item-row ${group.class}`;
    } else {
        // ✅ Fallback: aplicar clase según el status del item
        let rowClass = 'order-item-row';
        if (item.status === 'Ready') {
            rowClass += ' table-success';
        } else if (item.status === 'Preparing' || item.kitchenStatus === 'Sent') {
            rowClass += ' table-info';
        } else if (item.status === 'Pending') {
            rowClass += ' table-warning';
        } else if (item.status === 'Cancelled') {
            rowClass += ' table-secondary';
        }
        row.className = rowClass;
    }
    
    row.innerHTML = `
        <td>${item.productName}</td>
        <td class="quantity-cell">${quantityControls}</td>
        <td>$${item.price?.toFixed(2) ?? '0.00'}</td>
        <td class="item-total">$${(item.price * item.quantity).toFixed(2)}</td>
        <td class="estado-cell">${statusDisplay}</td>
        <td>
            ${item.notes ? `<span class="text-info" title="${item.notes}">📝 ${item.notes}</span>` : '<span class="text-muted">Sin notas</span>'}
        </td>
        <td class="acciones-cell">
            ${canEdit ? `<button class='btn btn-sm btn-primary me-1' onclick='editItem("${item.id}")'>Editar</button>` : ''}
            ${canEdit ? `<button class='btn btn-sm btn-danger' onclick='removeItem("${item.id}")'>Eliminar</button>` : ''}
            ${!canEdit ? `<i class="text-muted">✔</i>` : ''}
        </td>
    `;
    tbody.appendChild(row);
}

// ✅ NO AGRUPAR: Función eliminada - cada item se maneja individualmente
// Los items ya no se agrupan, cada uno mantiene su propia cantidad y estado

// ✅ Función para aumentar cantidad de un item específico - MEJORADA CON LÓGICA CLARA
async function increaseQuantity(itemId) {
    console.log('[increaseQuantity] Iniciado para itemId:', itemId);
    console.log('[increaseQuantity] === VERIFICACIÓN INICIAL ===');
    console.log('[increaseQuantity] currentOrder existe:', !!currentOrder);
    console.log('[increaseQuantity] currentOrder.items existe:', !!currentOrder?.items);
    console.log('[increaseQuantity] Número total de items:', currentOrder?.items?.length || 0);
    
    // Buscar el item por id único
    let item = currentOrder.items.find(i => i.id === itemId);
    
    if (!item) {
        console.error('[increaseQuantity] ❌ Item no encontrado por id:', itemId);
        console.error('[increaseQuantity] Items disponibles:', currentOrder.items.map(i => ({ id: i.id, name: i.productName })));
        Swal.fire('Error', 'No se encontró el item a modificar', 'error');
        return;
    }
    
    console.log('[increaseQuantity] ✅ Item encontrado:', {
        id: item.id,
        productName: item.productName,
        quantity: item.quantity,
        status: item.status,
        isNew: item.isNew,
        isFromBackend: item.isFromBackend
    });
    console.log('[increaseQuantity] === FIN VERIFICACIÓN INICIAL ===');

    // Validación de estado
    const pendingStates = ['Pending', 'Pendiente', 'Pendientes de cocina'];
    const sentToKitchenStates = ['SentToKitchen', 'Enviado a cocina'];
    const orderStatus = (currentOrder.status || 'Pending').toString();
    const isOrderPending = pendingStates.includes(orderStatus);
    const isOrderSentToKitchen = sentToKitchenStates.includes(orderStatus);
    const itemStatus = (item.status || '').toString();
    const isItemPending = pendingStates.includes(itemStatus);

    console.log('[increaseQuantity] itemStatus:', itemStatus, '| isItemPending:', isItemPending);
    console.log('[increaseQuantity] orderStatus:', orderStatus, '| isOrderPending:', isOrderPending, '| isOrderSentToKitchen:', isOrderSentToKitchen);

    if (!isItemPending || (!isOrderPending && !isOrderSentToKitchen)) {
        console.warn('[increaseQuantity] No se puede modificar item - Status:', item.status, 'Order Status:', currentOrder.status);
        Swal.fire('Atención', 'Solo se pueden modificar items pendientes en órdenes pendientes o enviadas a cocina', 'warning');
        return;
    }

    try {
        // ✅ ANÁLISIS DEL ITEM PARA IDENTIFICACIÓN (igual que saveItemChanges)
        console.log('[increaseQuantity] === ANÁLISIS DE ITEM ===');
        console.log('[increaseQuantity] Item ID:', item.id);
        console.log('[increaseQuantity] Item isNew:', item.isNew);
        console.log('[increaseQuantity] Item isFromBackend:', item.isFromBackend);
        console.log('[increaseQuantity] Item status:', item.status);
        console.log('[increaseQuantity] Orden status:', currentOrder.status);
        console.log('[increaseQuantity] Orden ID:', currentOrder.orderId);
        console.log('[increaseQuantity] === FIN ANÁLISIS ===');

        // ✅ ITEM NUEVO: Actualizar solo en frontend
        if (item.isNew === true) {
            console.log('[increaseQuantity] ✅ ITEM NUEVO detectado (isNew = true), actualizando solo en frontend');
            
            const newQuantity = item.quantity + 1;
            console.log('[increaseQuantity] Aumentando cantidad de', item.quantity, 'a', newQuantity, 'en frontend');
            
            item.quantity = newQuantity;
            updateOrderUI();
            showQuantityUpdateFeedback(item.id, newQuantity);
            
            Swal.fire({
                title: 'Cantidad Aumentada',
                text: `Se aumentó la cantidad de ${item.productName} a ${newQuantity} (item nuevo)`,
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            });
        }
        // ✅ ITEM EXISTENTE: Actualizar en backend y frontend
        else if (item.isFromBackend === true) {
            console.log('[increaseQuantity] ✅ ITEM EXISTENTE detectado (isFromBackend = true), llamando al backend');
            
            const newQuantity = item.quantity + 1;
            console.log('[increaseQuantity] Actualizando cantidad de', item.quantity, 'a', newQuantity, 'en backend');
            
            if (currentOrder.orderId) {
                console.log('[increaseQuantity] Orden existente, llamando al backend...');
                const response = await fetch('/Order/UpdateItemQuantityInOrder', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        orderId: currentOrder.orderId,
                        productId: item.productId,
                        quantity: newQuantity,
                        itemId: item.id // ✅ NUEVO: Enviar ItemId específico
                    })
                });
                
                if (response.ok) {
                    const result = await response.json();
                    console.log('[increaseQuantity] Cantidad actualizada exitosamente en backend:', result);
                    item.quantity = newQuantity;
                    updateOrderUI();
                    showQuantityUpdateFeedback(item.id, newQuantity);
                    
                    Swal.fire({
                        title: 'Cantidad Actualizada',
                        text: `Se aumentó la cantidad de ${item.productName} a ${newQuantity} (item existente)`,
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false
                    });
                } else {
                    const errorData = await response.json();
                    console.error('[increaseQuantity] Error del servidor:', errorData);
                    throw new Error(errorData.error || 'Error al actualizar cantidad en backend');
                }
            } else {
                console.warn('[increaseQuantity] Item existente pero sin orderId, actualizando solo en frontend');
                item.quantity = newQuantity;
                updateOrderUI();
                showQuantityUpdateFeedback(item.id, newQuantity);
                
                Swal.fire({
                    title: 'Cantidad Actualizada',
                    text: `Se aumentó la cantidad de ${item.productName} a ${newQuantity} (sin backend)`,
                    icon: 'success',
                    timer: 1500,
                    showConfirmButton: false
                });
            }
        }
        // ✅ CASO AMBIGUO: No se puede determinar claramente
        else {
            console.warn('[increaseQuantity] ⚠️ CASO AMBIGUO: No se puede determinar si el item es nuevo o existente');
            console.warn('[increaseQuantity] Item:', item);
            Swal.fire('Error', 'No se pudo determinar el tipo de item. Contacte al administrador.', 'error');
        }
    } catch (error) {
        console.error('[increaseQuantity] Error al aumentar cantidad:', error);
        Swal.fire('Error', error.message || 'No se pudo actualizar la cantidad', 'error');
    }
}

// ✅ Función para disminuir cantidad de un item específico - MEJORADA CON LÓGICA CLARA
async function decreaseQuantity(itemId) {
    console.log('[decreaseQuantity] Iniciado para itemId:', itemId);
    console.log('[decreaseQuantity] === VERIFICACIÓN INICIAL ===');
    console.log('[decreaseQuantity] currentOrder existe:', !!currentOrder);
    console.log('[decreaseQuantity] currentOrder.items existe:', !!currentOrder?.items);
    console.log('[decreaseQuantity] Número total de items:', currentOrder?.items?.length || 0);
    
    // Buscar el item por id único
    let item = currentOrder.items.find(i => i.id === itemId);
    
    if (!item) {
        console.error('[decreaseQuantity] ❌ Item no encontrado por id:', itemId);
        console.error('[decreaseQuantity] Items disponibles:', currentOrder.items.map(i => ({ id: i.id, name: i.productName })));
        Swal.fire('Error', 'No se encontró el item a modificar', 'error');
        return;
    }
    
    console.log('[decreaseQuantity] ✅ Item encontrado:', {
        id: item.id,
        productName: item.productName,
        quantity: item.quantity,
        status: item.status,
        isNew: item.isNew,
        isFromBackend: item.isFromBackend
    });
    console.log('[decreaseQuantity] === FIN VERIFICACIÓN INICIAL ===');

    // Validación de estado
    const pendingStates = ['Pending', 'Pendiente', 'Pendientes de cocina'];
    const sentToKitchenStates = ['SentToKitchen', 'Enviado a cocina'];
    const orderStatus = (currentOrder.status || 'Pending').toString();
    const isOrderPending = pendingStates.includes(orderStatus);
    const isOrderSentToKitchen = sentToKitchenStates.includes(orderStatus);
    const itemStatus = (item.status || '').toString();
    const isItemPending = pendingStates.includes(itemStatus);

    console.log('[decreaseQuantity] itemStatus:', itemStatus, '| isItemPending:', isItemPending);
    console.log('[decreaseQuantity] orderStatus:', orderStatus, '| isOrderPending:', isOrderPending, '| isOrderSentToKitchen:', isOrderSentToKitchen);

    if (!isItemPending || (!isOrderPending && !isOrderSentToKitchen)) {
        console.warn('[decreaseQuantity] No se puede modificar item - Status:', item.status, 'Order Status:', currentOrder.status);
        Swal.fire('Atención', 'Solo se pueden modificar items pendientes en órdenes pendientes o enviadas a cocina', 'warning');
        return;
    }

    // Validar que la cantidad no baje de 1
    if (item.quantity <= 1) {
        console.log('[decreaseQuantity] Cantidad mínima alcanzada, no se puede disminuir más');
        Swal.fire('Atención', 'La cantidad mínima es 1. Si deseas eliminar el item, usa el botón "Eliminar"', 'warning');
        return;
    }

    try {
        // ✅ ANÁLISIS DEL ITEM PARA IDENTIFICACIÓN (igual que saveItemChanges)
        console.log('[decreaseQuantity] === ANÁLISIS DE ITEM ===');
        console.log('[decreaseQuantity] Item ID:', item.id);
        console.log('[decreaseQuantity] Item isNew:', item.isNew);
        console.log('[decreaseQuantity] Item isFromBackend:', item.isFromBackend);
        console.log('[decreaseQuantity] Item status:', item.status);
        console.log('[decreaseQuantity] Orden status:', currentOrder.status);
        console.log('[decreaseQuantity] Orden ID:', currentOrder.orderId);
        console.log('[decreaseQuantity] === FIN ANÁLISIS ===');

        // ✅ ITEM NUEVO: Actualizar solo en frontend
        if (item.isNew === true) {
            console.log('[decreaseQuantity] ✅ ITEM NUEVO detectado (isNew = true), actualizando solo en frontend');
            
            const newQuantity = item.quantity - 1;
            console.log('[decreaseQuantity] Disminuyendo cantidad de', item.quantity, 'a', newQuantity, 'en frontend');
            
            item.quantity = newQuantity;
            updateOrderUI();
            showQuantityUpdateFeedback(item.id, newQuantity);
            
            Swal.fire({
                title: 'Cantidad Disminuida',
                text: `Se disminuyó la cantidad de ${item.productName} a ${newQuantity} (item nuevo)`,
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            });
        }
        // ✅ ITEM EXISTENTE: Actualizar en backend y frontend
        else if (item.isFromBackend === true) {
            console.log('[decreaseQuantity] ✅ ITEM EXISTENTE detectado (isFromBackend = true), llamando al backend');
            
            const newQuantity = item.quantity - 1;
            console.log('[decreaseQuantity] Actualizando cantidad de', item.quantity, 'a', newQuantity, 'en backend');
            
            if (currentOrder.orderId) {
                console.log('[decreaseQuantity] Orden existente, llamando al backend...');
                const response = await fetch('/Order/UpdateItemQuantityInOrder', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        orderId: currentOrder.orderId,
                        productId: item.productId,
                        quantity: newQuantity,
                        itemId: item.id // ✅ NUEVO: Enviar ItemId específico
                    })
                });
                
                if (response.ok) {
                    const result = await response.json();
                    console.log('[decreaseQuantity] Cantidad actualizada exitosamente en backend:', result);
                    item.quantity = newQuantity;
                    updateOrderUI();
                    showQuantityUpdateFeedback(item.id, newQuantity);
                    
                    Swal.fire({
                        title: 'Cantidad Actualizada',
                        text: `Se disminuyó la cantidad de ${item.productName} a ${newQuantity} (item existente)`,
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false
                    });
                } else {
                    const errorData = await response.json();
                    console.error('[decreaseQuantity] Error del servidor:', errorData);
                    throw new Error(errorData.error || 'Error al actualizar cantidad en backend');
                }
            } else {
                console.warn('[decreaseQuantity] Item existente pero sin orderId, actualizando solo en frontend');
                item.quantity = newQuantity;
                updateOrderUI();
                showQuantityUpdateFeedback(item.id, newQuantity);
                
                Swal.fire({
                    title: 'Cantidad Actualizada',
                    text: `Se disminuyó la cantidad de ${item.productName} a ${newQuantity} (sin backend)`,
                    icon: 'success',
                    timer: 1500,
                    showConfirmButton: false
                });
            }
        }
        // ✅ CASO AMBIGUO: No se puede determinar claramente
        else {
            console.warn('[decreaseQuantity] ⚠️ CASO AMBIGUO: No se puede determinar si el item es nuevo o existente');
            console.warn('[decreaseQuantity] Item:', item);
            Swal.fire('Error', 'No se pudo determinar el tipo de item. Contacte al administrador.', 'error');
        }
    } catch (error) {
        console.error('[decreaseQuantity] Error al disminuir cantidad:', error);
        Swal.fire('Error', error.message || 'No se pudo actualizar la cantidad', 'error');
    }
}

// ✅ Nueva función para mostrar feedback visual de cambios de cantidad
function showQuantityUpdateFeedback(itemId, newQuantity) {
    const quantityElement = document.querySelector(`[data-item-id="${itemId}"] .quantity-display`);
    if (quantityElement) {
        quantityElement.classList.add('updated');
        setTimeout(() => {
            quantityElement.classList.remove('updated');
        }, 300);
    }
}

// Exponer funciones al ámbito global para los botones onclick
window.increaseQuantity = increaseQuantity;
window.decreaseQuantity = decreaseQuantity; 