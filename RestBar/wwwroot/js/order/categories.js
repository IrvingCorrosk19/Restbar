// Categories and Products Management

let selectedCategoryId = null;

// Cargar categorías
async function loadCategories() {
    try {
        const response = await fetch('/Order/GetActiveCategories');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const result = await response.json();
        
        if (result.success && result.data) {
            const categoriesHtml = result.data.map(cat => `
                <button class="btn btn-outline-primary categoria-btn" 
                        data-id="${cat.id}" 
                        onclick="selectCategory('${cat.id}', '${cat.name}')">
                    ${cat.name}
                </button>
            `).join('');
            document.getElementById('categories').innerHTML = categoriesHtml;
        } else {
            throw new Error('Formato de respuesta inválido');
        }
    } catch (error) {
        console.error('Error al cargar categorías:', error);
        Swal.fire('Error', 'No se pudieron cargar las categorías', 'error');
    }
}

function selectCategory(categoryId, categoryName) {
    selectedCategoryId = categoryId;
    document.querySelectorAll('.categoria-btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.id === categoryId) {
            btn.classList.add('active');
        }
    });
    loadProducts(categoryId);
}

// Cargar productos
async function loadProducts(categoryId) {
    try {
        const response = await fetch(`/Order/GetProducts?categoryId=${categoryId}`);
        const products = await response.json();
        // Calcular cantidades actuales en la orden para cada producto (solo status 'Pending')
        const productQuantities = {};
        if (currentOrder && currentOrder.items) {
            currentOrder.items.forEach(item => {
                if (item.status === 'Pending') {
                    if (!productQuantities[item.productId]) productQuantities[item.productId] = 0;
                    productQuantities[item.productId] += item.quantity;
                }
            });
        }
        const productsHtml = products.map(product => {
            const quantity = productQuantities[product.id] || 0;
            const showControls = quantity > 0;
            return `
            <div class="col-md-3 col-sm-6 mb-4">
                <div class="card h-100 product-card${showControls ? ' selected-product' : ''}" data-product-id="${product.id}">
                    <img src="${product.imageUrl || '/images/no-image.png'}" 
                         class="card-img-top" 
                         alt="${product.name}"
                         style="height: 120px; object-fit: cover;">
                    <div class="card-body p-2">
                        <h6 class="card-title mb-1">${product.name}</h6>
                        <p class="card-text text-primary mb-2">$${product.price.toFixed(2)}</p>
                        <div class="d-flex justify-content-between align-items-center">
                            <!-- <div class="quantity-controls">
                                <button class="btn btn-sm btn-outline-secondary" 
                                        onclick="decreaseProductCardQuantity('${product.id}')" 
                                        style="display: ${showControls ? 'inline-block' : 'none'};" 
                                        id="decrease-${product.id}">-</button>
                                <span class="quantity" id="quantity-${product.id}" style="display: ${showControls ? 'inline-block' : 'none'};">${quantity}</span>
                                <button class="btn btn-sm btn-outline-secondary" 
                                        onclick="increaseProductCardQuantity('${product.id}')" 
                                        style="display: ${showControls ? 'inline-block' : 'none'};" 
                                        id="increase-${product.id}">+</button>
                            </div> -->
                            <div class="btn-group" role="group">
                                <button class="btn btn-sm btn-outline-primary" 
                                        onclick="addToOrder('${product.id}', '${product.name}', ${product.price})">
                                    + Agregar
                                </button>
                                <button class="btn btn-sm btn-outline-info" 
                                        onclick="addToOrderWithNotes('${product.id}', '${product.name}', ${product.price})"
                                        title="Agregar con notas">
                                    📝
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            `;
        }).join('');
        document.getElementById('products').innerHTML = productsHtml;
    } catch (error) {
        console.error('Error al cargar productos:', error);
        Swal.fire('Error', 'No se pudieron cargar los productos', 'error');
    }
}

// Comentadas las funciones de cantidad para la card
/*
function increaseProductCardQuantity(productId) {}
function decreaseProductCardQuantity(productId) {}
function showQuantityControls(productId) {}
function hideQuantityControls(productId) {}
*/

// Agregar al pedido
function addToOrder(productId, productName, price) {
    if (!currentOrder.tableId) {
        Swal.fire('Error', 'Debes seleccionar una mesa primero', 'warning');
        return;
    }

    // Crear un nuevo item individual cada vez
    const newItem = {
        id: guid.newGuid(),
        productId,
        productName,
        price,
        quantity: 1,
        status: 'Pending',
        isNew: true // ✅ PARÁMETRO PARA IDENTIFICAR ITEMS NUEVOS
    };
    currentOrder.items.unshift(newItem); // ✅ CAMBIADO: unshift() en lugar de push() para agregar al principio

    // Recalcular la cantidad total de este producto en la orden (solo Pending)
    const totalQuantity = currentOrder.items.filter(item => item.productId === productId && item.status === 'Pending')
        .reduce((sum, item) => sum + item.quantity, 0);

    // Actualizar el contador en la card
    const quantityElement = document.getElementById(`quantity-${productId}`);
    if (quantityElement) {
        quantityElement.textContent = totalQuantity;
        // showQuantityControls(productId); // Comentado para evitar conflicto con la card
    }

    // Resaltar la card del producto
    const productCard = document.querySelector(`[data-product-id="${productId}"]`);
    if (productCard) {
        productCard.classList.add('selected-product');
    }

    updateOrderUI();
    enableConfirmButton();
}

// ✅ Nueva función para agregar con notas
function addToOrderWithNotes(productId, productName, price) {
    if (!currentOrder.tableId) {
        Swal.fire('Error', 'Debes seleccionar una mesa primero', 'warning');
        return;
    }

    // Abrir modal de edición
    openEditModal(productId, productName, price);
}

// ✅ Función para abrir el modal de edición
function openEditModal(productId, productName, price, itemId = null) {
    console.log('[openEditModal] Abriendo modal para:', productName);
    
    // Llenar el modal con los datos del producto
    document.getElementById('editProductId').value = productId;
    document.getElementById('editProductName').value = productName;
    document.getElementById('editUnitPrice').textContent = `$${price.toFixed(2)}`;
    document.getElementById('editQuantity').value = '1';
    document.getElementById('editNotes').value = '';
    
    // Calcular total inicial
    updateModalTotal();
    
    // Si es edición de un item existente
    if (itemId) {
        document.getElementById('editItemId').value = itemId;
        const item = currentOrder.items.find(i => i.id === itemId);
        if (item) {
            document.getElementById('editQuantity').value = item.quantity;
            document.getElementById('editNotes').value = item.notes || '';
            updateModalTotal();
        }
    } else {
        document.getElementById('editItemId').value = '';
    }
    
    // ✅ Agregar event listener para actualizar total automáticamente
    const quantityInput = document.getElementById('editQuantity');
    quantityInput.addEventListener('input', updateModalTotal);
    
    // Mostrar el modal
    const modal = new bootstrap.Modal(document.getElementById('editItemModal'));
    modal.show();
}

// ✅ Función para actualizar el total en el modal
function updateModalTotal() {
    const quantity = parseInt(document.getElementById('editQuantity').value) || 1;
    const unitPrice = parseFloat(document.getElementById('editUnitPrice').textContent.replace('$', '')) || 0;
    const total = quantity * unitPrice;
    document.getElementById('editTotalPrice').textContent = `$${total.toFixed(2)}`;
}

// ✅ Funciones para cambiar cantidad en el modal
function increaseModalQuantity() {
    const input = document.getElementById('editQuantity');
    const currentValue = parseInt(input.value) || 1;
    input.value = Math.min(99, currentValue + 1);
    updateModalTotal();
}

function decreaseModalQuantity() {
    const input = document.getElementById('editQuantity');
    const currentValue = parseInt(input.value) || 1;
    input.value = Math.max(1, currentValue - 1);
    updateModalTotal();
}

// ✅ Función para guardar cambios del modal - MEJORADA
async function saveItemChanges() {
    const productId = document.getElementById('editProductId').value;
    const productName = document.getElementById('editProductName').value;
    const quantity = parseInt(document.getElementById('editQuantity').value) || 1;
    const notes = document.getElementById('editNotes').value.trim();
    const itemId = document.getElementById('editItemId').value;
    const unitPrice = parseFloat(document.getElementById('editUnitPrice').textContent.replace('$', '')) || 0;
    
    console.log('[saveItemChanges] Guardando cambios:', {
        productId, productName, quantity, notes, itemId, unitPrice
    });
    
    if (itemId) {
        // ✅ Editar item existente
        const item = currentOrder.items.find(i => i.id === itemId);
        if (item) {
            // ✅ ANÁLISIS DEL ITEM PARA IDENTIFICACIÓN
            console.log('[saveItemChanges] === ANÁLISIS DE ITEM EXISTENTE ===');
            console.log('[saveItemChanges] Item ID:', item.id);
            console.log('[saveItemChanges] Item isNew:', item.isNew);
            console.log('[saveItemChanges] Item isFromBackend:', item.isFromBackend);
            console.log('[saveItemChanges] Item status:', item.status);
            console.log('[saveItemChanges] Orden status:', currentOrder.status);
            console.log('[saveItemChanges] Orden ID:', currentOrder.orderId);
            console.log('[saveItemChanges] === FIN ANÁLISIS ===');
            
            try {
                // ✅ ITEM NUEVO: Actualizar solo en frontend
                if (item.isNew === true) {
                    console.log('[saveItemChanges] ✅ ITEM NUEVO detectado (isNew = true), actualizando solo en frontend');
                    item.quantity = quantity;
                    item.notes = notes;
                    console.log('[saveItemChanges] Item nuevo actualizado en frontend:', item);
                    
                    Swal.fire({
                        title: 'Item Actualizado',
                        text: `${productName} actualizado exitosamente (item nuevo)`,
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false
                    });
                }
                // ✅ ITEM EXISTENTE: Actualizar en backend y frontend
                else if (item.isFromBackend === true) {
                    console.log('[saveItemChanges] ✅ ITEM EXISTENTE detectado (isFromBackend = true), llamando al backend');
                    
                    if (currentOrder.orderId) {
                        console.log('[saveItemChanges] Orden existente, llamando al backend...');
                        const response = await fetch('/Order/UpdateItemInOrder', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({
                                orderId: currentOrder.orderId,
                                productId: item.productId,
                                quantity: quantity,
                                notes: notes
                            })
                        });
                        
                        if (response.ok) {
                            const result = await response.json();
                            console.log('[saveItemChanges] Item actualizado en backend:', result);
                            
                            // Actualizar item localmente
                            item.quantity = quantity;
                            item.notes = notes;
                            
                            Swal.fire({
                                title: 'Item Actualizado',
                                text: `${productName} actualizado exitosamente (item existente)`,
                                icon: 'success',
                                timer: 1500,
                                showConfirmButton: false
                            });
                        } else {
                            const errorData = await response.json();
                            console.error('[saveItemChanges] Error del servidor:', errorData);
                            throw new Error(errorData.error || 'Error al actualizar el item en backend');
                        }
                    } else {
                        console.warn('[saveItemChanges] Item existente pero sin orderId, actualizando solo en frontend');
                        item.quantity = quantity;
                        item.notes = notes;
                        
                        Swal.fire({
                            title: 'Item Actualizado',
                            text: `${productName} actualizado exitosamente (sin backend)`,
                            icon: 'success',
                            timer: 1500,
                            showConfirmButton: false
                        });
                    }
                }
                // ✅ CASO AMBIGUO: No se puede determinar claramente
                else {
                    console.warn('[saveItemChanges] ⚠️ CASO AMBIGUO: No se puede determinar si el item es nuevo o existente');
                    console.warn('[saveItemChanges] Item:', item);
                    Swal.fire('Error', 'No se pudo determinar el tipo de item. Contacte al administrador.', 'error');
                    return;
                }
            } catch (error) {
                console.error('[saveItemChanges] Error al actualizar item existente:', error);
                Swal.fire('Error', error.message || 'No se pudo actualizar el item', 'error');
                return;
            }
        }
    } else {
        // ✅ Crear nuevo item
        console.log('[saveItemChanges] ✅ Creando NUEVO item');
        const newItem = {
            id: guid.newGuid(),
            productId,
            productName,
            price: unitPrice,
            quantity,
            notes,
            status: 'Pending',
            isNew: true // ✅ PARÁMETRO PARA IDENTIFICAR ITEMS NUEVOS
        };
        
        currentOrder.items.unshift(newItem); // ✅ CAMBIADO: unshift() en lugar de push() para agregar al principio
        console.log('[saveItemChanges] Nuevo item creado:', newItem);
        
        // Actualizar contador en la card del producto
        const quantityElement = document.getElementById(`quantity-${productId}`);
        if (quantityElement) {
            const currentQuantity = parseInt(quantityElement.textContent) || 0;
            quantityElement.textContent = currentQuantity + quantity;
        }
        
        // Resaltar la card del producto
        const productCard = document.querySelector(`[data-product-id="${productId}"]`);
        if (productCard) {
            productCard.classList.add('selected-product');
        }
        
        // Mostrar confirmación
        Swal.fire({
            title: 'Producto Agregado',
            text: `${productName} agregado al pedido`,
            icon: 'success',
            timer: 1500,
            showConfirmButton: false
        });
    }
    
    // Cerrar modal y actualizar UI
    const modal = bootstrap.Modal.getInstance(document.getElementById('editItemModal'));
    modal.hide();
    
    updateOrderUI();
    enableConfirmButton();
}

// Función para actualizar cantidad de item en el pedido
function updateOrderItemQuantity(productId, newQuantity) {
    // ✅ Buscar el último item agregado para este producto (el más reciente)
    const itemsForProduct = currentOrder.items.filter(i => i.productId === productId && i.status === 'Pending');
    if (itemsForProduct.length > 0) {
        // Tomar el último item agregado
        const lastItem = itemsForProduct[itemsForProduct.length - 1];
        lastItem.quantity = newQuantity;
        console.log('[Frontend] Cantidad actualizada para item individual:', lastItem);
        updateOrderUI();
    }
}

window.selectedCategoryId = selectedCategoryId;

// ✅ Exponer nuevas funciones al ámbito global
window.addToOrderWithNotes = addToOrderWithNotes;
window.openEditModal = openEditModal;
window.updateModalTotal = updateModalTotal;
window.increaseModalQuantity = increaseModalQuantity;
window.decreaseModalQuantity = decreaseModalQuantity;
window.saveItemChanges = saveItemChanges; 