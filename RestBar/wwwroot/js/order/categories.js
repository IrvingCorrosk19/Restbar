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
            const stockStatus = product.stock !== null && product.stock !== undefined ? 
                (product.stock > 0 ? 
                    `<span class="badge bg-success">Stock: ${product.stock}</span>` : 
                    `<span class="badge bg-danger">Sin stock</span>`) : 
                `<span class="badge bg-warning">Stock no configurado</span>`;
            
            // ✅ NUEVO: Calcular precio con impuesto
            const taxRate = product.taxRate || 0;
            const priceWithTax = product.price * (1 + taxRate / 100);
            const taxAmount = product.price * (taxRate / 100);
            
            return `
            <div class="col-md-3 col-sm-6 mb-4">
                <div class="card h-100 product-card${showControls ? ' selected-product' : ''}" data-product-id="${product.id}">
                    <img src="${product.imageUrl || '/images/no-image.png'}" 
                         class="card-img-top" 
                         alt="${product.name}"
                         style="height: 120px; object-fit: cover;">
                    <div class="card-body p-2">
                        <h6 class="card-title mb-1">${product.name}</h6>
                        <p class="card-text text-primary mb-1">$${product.price.toFixed(2)}</p>
                        ${taxRate > 0 ? `<small class="text-muted">+ ${taxRate}% IVA = $${priceWithTax.toFixed(2)}</small>` : ''}
                        <div class="mb-2">
                            ${stockStatus}
                        </div>
                        <div class="d-flex justify-content-between align-items-center">
                            <div class="btn-group" role="group">
                                <button class="btn btn-sm btn-outline-primary" 
                                        onclick="addToOrder('${product.id}', '${product.name}', ${product.price}, ${taxRate})"
                                        ${product.stock !== null && product.stock <= 0 ? 'disabled' : ''}>
                                    ${product.stock !== null && product.stock <= 0 ? 'Sin stock' : '+ Agregar'}
                                </button>
                                <button class="btn btn-sm btn-outline-info" 
                                        onclick="addToOrderWithNotes('${product.id}', '${product.name}', ${product.price}, ${taxRate})"
                                        title="Agregar con notas"
                                        ${product.stock !== null && product.stock <= 0 ? 'disabled' : ''}>
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
        Swal.fire('Error', 'No se pudieron cargar los productos', 'error');
    }
}

// Agregar al pedido
function addToOrder(productId, productName, price, taxRate = 0) {
    if (!currentOrder.tableId) {
        Swal.fire('Error', 'Debes seleccionar una mesa primero', 'warning');
        return;
    }

    // ✅ NUEVO: Calcular precio con impuesto
    const priceWithTax = price * (1 + taxRate / 100);
    const taxAmount = price * (taxRate / 100);

    // Crear un nuevo item individual cada vez
    const newItem = {
        id: guid.newGuid(),
        productId,
        productName,
        price,
        priceWithTax,
        taxRate,
        taxAmount,
        quantity: 1,
        status: 'Pending',
        isNew: true
    };
    currentOrder.items.unshift(newItem);

    // Recalcular la cantidad total de este producto en la orden (solo Pending)
    const totalQuantity = currentOrder.items.filter(item => item.productId === productId && item.status === 'Pending')
        .reduce((sum, item) => sum + item.quantity, 0);

    // Actualizar el contador en la card
    const quantityElement = document.getElementById(`quantity-${productId}`);
    if (quantityElement) {
        quantityElement.textContent = totalQuantity;
    }

    // Resaltar la card del producto
    const productCard = document.querySelector(`[data-product-id="${productId}"]`);
    if (productCard) {
        productCard.classList.add('selected-product');
    }

    updateOrderUI();
    enableConfirmButton();
}

// Función para agregar con notas
function addToOrderWithNotes(productId, productName, price, taxRate = 0) {
    if (!currentOrder.tableId) {
        Swal.fire('Error', 'Debes seleccionar una mesa primero', 'warning');
        return;
    }

    // Abrir modal de edición
    openEditModal(productId, productName, price, taxRate);
}

// Función para abrir el modal de edición
function openEditModal(productId, productName, price, taxRate = 0, itemId = null) {
    // ✅ NUEVO: Calcular precio con impuesto
    const priceWithTax = price * (1 + taxRate / 100);
    const taxAmount = price * (taxRate / 100);
    
    // Llenar el modal con los datos del producto
    document.getElementById('editProductId').value = productId;
    document.getElementById('editProductName').value = productName;
    document.getElementById('editUnitPrice').textContent = `$${price.toFixed(2)}`;
    document.getElementById('editTaxRate').textContent = `${taxRate}%`;
    document.getElementById('editPriceWithTax').textContent = `$${priceWithTax.toFixed(2)}`;
    document.getElementById('editQuantity').value = '1';
    document.getElementById('editNotes').value = '';
    
    // Calcular total inicial
    updateModalTotal(taxRate);
    
    // Si es edición de un item existente
    if (itemId) {
        document.getElementById('editItemId').value = itemId;
        const item = currentOrder.items.find(i => i.id === itemId);
        if (item) {
            document.getElementById('editQuantity').value = item.quantity;
            document.getElementById('editNotes').value = item.notes || '';
            updateModalTotal(item.taxRate || 0);
        }
    } else {
        document.getElementById('editItemId').value = '';
    }
    
    // Agregar event listener para actualizar total automáticamente
    const quantityInput = document.getElementById('editQuantity');
    quantityInput.addEventListener('input', () => updateModalTotal(taxRate));
    
    // Mostrar el modal
    const modal = new bootstrap.Modal(document.getElementById('editItemModal'));
    modal.show();
}

// Función para actualizar el total en el modal
function updateModalTotal(taxRate = 0) {
    const quantity = parseInt(document.getElementById('editQuantity').value) || 1;
    const unitPrice = parseFloat(document.getElementById('editUnitPrice').textContent.replace('$', '')) || 0;
    const subtotal = quantity * unitPrice;
    const taxAmount = subtotal * (taxRate / 100);
    const total = subtotal + taxAmount;
    
    document.getElementById('editSubtotal').textContent = `$${subtotal.toFixed(2)}`;
    document.getElementById('editTaxAmount').textContent = `$${taxAmount.toFixed(2)}`;
    document.getElementById('editTotalPrice').textContent = `$${total.toFixed(2)}`;
}

// Funciones para cambiar cantidad en el modal
function increaseModalQuantity() {
    const input = document.getElementById('editQuantity');
    const currentValue = parseInt(input.value) || 1;
    input.value = Math.min(99, currentValue + 1);
    const taxRate = parseFloat(document.getElementById('editTaxRate').textContent.replace('%', '')) || 0;
    updateModalTotal(taxRate);
}

function decreaseModalQuantity() {
    const input = document.getElementById('editQuantity');
    const currentValue = parseInt(input.value) || 1;
    input.value = Math.max(1, currentValue - 1);
    const taxRate = parseFloat(document.getElementById('editTaxRate').textContent.replace('%', '')) || 0;
    updateModalTotal(taxRate);
}

// Función para guardar cambios del modal
async function saveItemChanges() {
    const productId = document.getElementById('editProductId').value;
    const productName = document.getElementById('editProductName').value;
    const quantity = parseInt(document.getElementById('editQuantity').value) || 1;
    const notes = document.getElementById('editNotes').value.trim();
    const itemId = document.getElementById('editItemId').value;
    const unitPrice = parseFloat(document.getElementById('editUnitPrice').textContent.replace('$', '')) || 0;
    const taxRate = parseFloat(document.getElementById('editTaxRate').textContent.replace('%', '')) || 0;
    const priceWithTax = parseFloat(document.getElementById('editPriceWithTax').textContent.replace('$', '')) || unitPrice;
    
    if (itemId) {
        // Editar item existente
        const item = currentOrder.items.find(i => i.id === itemId);
        if (item) {
            try {
                // ITEM NUEVO: Actualizar solo en frontend
                if (item.isNew === true) {
                    item.quantity = quantity;
                    item.notes = notes;
                    item.taxRate = taxRate;
                    item.priceWithTax = priceWithTax;
                    
                    Swal.fire({
                        title: 'Item Actualizado',
                        text: `${productName} actualizado exitosamente (item nuevo)`,
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false
                    });
                }
                // ITEM EXISTENTE: Actualizar en backend y frontend
                else if (item.isFromBackend === true) {
                    if (currentOrder.orderId) {
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
                            throw new Error(errorData.error || 'Error al actualizar el item en backend');
                        }
                    } else {
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
                // CASO AMBIGUO: No se puede determinar claramente
                else {
                    Swal.fire('Error', 'No se pudo determinar el tipo de item. Contacte al administrador.', 'error');
                    return;
                }
            } catch (error) {
                Swal.fire('Error', error.message || 'No se pudo actualizar el item', 'error');
                return;
            }
        }
    } else {
        // Crear nuevo item
        const newItem = {
            id: guid.newGuid(),
            productId,
            productName,
            price: unitPrice,
            priceWithTax: priceWithTax,
            taxRate: taxRate,
            quantity,
            notes,
            status: 'Pending',
            isNew: true
        };
        
        currentOrder.items.unshift(newItem);
        
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
    // Buscar el último item agregado para este producto (el más reciente)
    const itemsForProduct = currentOrder.items.filter(i => i.productId === productId && i.status === 'Pending');
    if (itemsForProduct.length > 0) {
        // Tomar el último item agregado
        const lastItem = itemsForProduct[itemsForProduct.length - 1];
        lastItem.quantity = newQuantity;
        updateOrderUI();
    }
}

window.selectedCategoryId = selectedCategoryId;

// ✅ NUEVA: Función para recargar productos después de pago
function reloadProductsAfterPayment() {
    if (selectedCategoryId) {
        loadProducts(selectedCategoryId);
    }
}

// Exponer nuevas funciones al ámbito global
window.addToOrderWithNotes = addToOrderWithNotes;
window.openEditModal = openEditModal;
window.updateModalTotal = updateModalTotal;
window.increaseModalQuantity = increaseModalQuantity;
window.decreaseModalQuantity = decreaseModalQuantity;
window.saveItemChanges = saveItemChanges;
window.reloadProductsAfterPayment = reloadProductsAfterPayment; 