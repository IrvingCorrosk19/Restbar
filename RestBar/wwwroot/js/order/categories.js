// Categories and Products Management

let selectedCategoryId = null;

// Cargar categorías
async function loadCategories() {
    try {
        const response = await fetch('/Order/GetActiveCategories', {
            credentials: 'include',
            headers: { 'Accept': 'application/json' }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // Obtener el texto de la respuesta
        const responseText = await response.text();

        // Parsear JSON
        let result;
        try {
            result = JSON.parse(responseText);
        } catch (parseError) {
            Swal.fire('Error', 'Respuesta inválida del servidor (no es JSON válido).', 'error');
            return;
        }

        // Verificar estructura de respuesta
        if (!result || typeof result !== 'object') {
            Swal.fire('Error', 'Estructura de respuesta inválida del servidor.', 'error');
            return;
        }

        // Asegurar array de categorías
        const categories = Array.isArray(result?.data)
            ? result.data
            : (Array.isArray(result) ? result : []);
        
        if (!Array.isArray(categories)) {
            Swal.fire('Error', 'Estructura de datos inválida para categorías.', 'error');
            return;
        }

        if (categories.length === 0) {
            document.getElementById('categories').innerHTML = '<p class="text-muted">No hay categorías disponibles</p>';
            return;
        }
            
        const categoriesHtml = categories.map(cat => `
            <button class="btn btn-outline-primary categoria-btn" 
                    data-id="${cat.id}" 
                    onclick="selectCategory('${cat.id}', '${cat.name}')">
                ${cat.name}
            </button>
        `).join('');
        
        document.getElementById('categories').innerHTML = categoriesHtml;
        
    } catch (error) {
        Swal.fire('Error', 'No se pudieron cargar las categorías: ' + error.message, 'error');
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
        
        // ✅ NUEVO: Calcular cantidades actuales en la orden para cada producto (solo items activos)
        const productQuantities = {};
        if (currentOrder && currentOrder.items) {
            currentOrder.items.forEach(item => {
                // Solo contar items que no estén cancelados
                if (item.status !== 'cancelled' && item.status !== 'Cancelled') {
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
    try {
        console.log('🔍 [Categories] addToOrder() - Iniciando agregado de producto...');
        console.log('📋 [Categories] addToOrder() - Parámetros recibidos:', {
            productId,
            productName,
            price,
            taxRate,
            currentTableId: currentOrder?.tableId
        });

        if (!currentOrder.tableId) {
            console.log('❌ [Categories] addToOrder() - No hay mesa seleccionada');
            Swal.fire('Error', 'Debes seleccionar una mesa primero', 'warning');
            return;
        }

        // ✅ NUEVO: Calcular precio con impuesto
        const priceWithTax = price * (1 + taxRate / 100);
        const taxAmount = price * (taxRate / 100);
        
        console.log('💰 [Categories] addToOrder() - Cálculos de precio:', {
            priceOriginal: price,
            taxRate: taxRate,
            priceWithTax: priceWithTax,
            taxAmount: taxAmount
        });

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
        
        console.log('📦 [Categories] addToOrder() - Nuevo item creado:', newItem);
        
        currentOrder.items.unshift(newItem);
        console.log('📊 [Categories] addToOrder() - Total items en orden:', currentOrder.items.length);

        // Recalcular la cantidad total de este producto en la orden (solo Pending)
        const totalQuantity = currentOrder.items.filter(item => item.productId === productId && item.status === 'Pending')
            .reduce((sum, item) => sum + item.quantity, 0);
        
        console.log('🔢 [Categories] addToOrder() - Cantidad total del producto:', totalQuantity);

        // Actualizar el contador en la card
        const quantityElement = document.getElementById(`quantity-${productId}`);
        if (quantityElement) {
            quantityElement.textContent = totalQuantity;
            console.log('✅ [Categories] addToOrder() - Contador actualizado en UI');
        } else {
            console.log('⚠️ [Categories] addToOrder() - No se encontró elemento quantity-${productId}');
        }

        // Resaltar la card del producto
        const productCard = document.querySelector(`[data-product-id="${productId}"]`);
        if (productCard) {
            productCard.classList.add('selected-product');
            console.log('✅ [Categories] addToOrder() - Card del producto resaltada');
        } else {
            console.log('⚠️ [Categories] addToOrder() - No se encontró card del producto');
        }

        updateOrderUI();
        enableConfirmButton();
        
        console.log('✅ [Categories] addToOrder() - Producto agregado exitosamente:', {
            productName,
            quantity: 1,
            totalItems: currentOrder.items.length,
            totalOrder: currentOrder.total
        });
    } catch (error) {
        console.error('❌ [Categories] addToOrder() - Error:', error);
        Swal.fire('Error', 'No se pudo agregar el producto: ' + error.message, 'error');
    }
}

// Función para agregar con notas
function addToOrderWithNotes(productId, productName, price, taxRate = 0) {
    try {
        console.log('🔍 [Categories] addToOrderWithNotes() - Iniciando agregado con notas...');
        console.log('📋 [Categories] addToOrderWithNotes() - Parámetros recibidos:', {
            productId,
            productName,
            price,
            taxRate,
            currentTableId: currentOrder?.tableId
        });

        if (!currentOrder.tableId) {
            console.log('❌ [Categories] addToOrderWithNotes() - No hay mesa seleccionada');
            Swal.fire('Error', 'Debes seleccionar una mesa primero', 'warning');
            return;
        }

        console.log('✅ [Categories] addToOrderWithNotes() - Abriendo modal de edición...');
        // Abrir modal de edición
        openEditModal(productId, productName, price, taxRate);
        
        console.log('✅ [Categories] addToOrderWithNotes() - Modal abierto exitosamente');
    } catch (error) {
        console.error('❌ [Categories] addToOrderWithNotes() - Error:', error);
        Swal.fire('Error', 'No se pudo abrir el modal: ' + error.message, 'error');
    }
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
    try {
        console.log('🔍 [Categories] saveItemChanges() - Iniciando guardado de cambios...');
        
        const productId = document.getElementById('editProductId').value;
        const productName = document.getElementById('editProductName').value;
        const quantity = parseInt(document.getElementById('editQuantity').value) || 1;
        const notes = document.getElementById('editNotes').value.trim();
        const itemId = document.getElementById('editItemId').value;
        const unitPrice = parseFloat(document.getElementById('editUnitPrice').textContent.replace('$', '')) || 0;
        const taxRate = parseFloat(document.getElementById('editTaxRate').textContent.replace('%', '')) || 0;
        const priceWithTax = parseFloat(document.getElementById('editPriceWithTax').textContent.replace('$', '')) || unitPrice;
        
        console.log('📋 [Categories] saveItemChanges() - Datos del modal:', {
            productId,
            productName,
            quantity,
            notes,
            itemId,
            unitPrice,
            taxRate,
            priceWithTax,
            isEditing: !!itemId
        });
    
        if (itemId) {
            console.log('🔄 [Categories] saveItemChanges() - Editando item existente...');
            // Editar item existente
            const item = currentOrder.items.find(i => i.id === itemId);
            if (item) {
                console.log('📦 [Categories] saveItemChanges() - Item encontrado:', {
                    itemId: item.id,
                    isNew: item.isNew,
                    isFromBackend: item.isFromBackend,
                    currentQuantity: item.quantity,
                    newQuantity: quantity
                });
                
                try {
                    // ITEM NUEVO: Actualizar solo en frontend
                    if (item.isNew === true) {
                        console.log('✅ [Categories] saveItemChanges() - Actualizando item nuevo en frontend...');
                        item.quantity = quantity;
                        item.notes = notes;
                        item.taxRate = taxRate;
                        item.priceWithTax = priceWithTax;
                        
                        console.log('✅ [Categories] saveItemChanges() - Item nuevo actualizado exitosamente');
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
                        console.log('🔄 [Categories] saveItemChanges() - Actualizando item existente en backend...');
                        if (currentOrder.orderId) {
                            console.log('📡 [Categories] saveItemChanges() - Enviando petición al backend...');
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
                                console.log('✅ [Categories] saveItemChanges() - Respuesta del backend:', result);
                                
                                // Actualizar item localmente
                                item.quantity = quantity;
                                item.notes = notes;
                                
                                console.log('✅ [Categories] saveItemChanges() - Item existente actualizado exitosamente');
                                Swal.fire({
                                    title: 'Item Actualizado',
                                    text: `${productName} actualizado exitosamente (item existente)`,
                                    icon: 'success',
                                    timer: 1500,
                                    showConfirmButton: false
                                });
                            } else {
                                const errorData = await response.json();
                                console.error('❌ [Categories] saveItemChanges() - Error del backend:', errorData);
                                throw new Error(errorData.error || 'Error al actualizar el item en backend');
                            }
                        } else {
                            console.log('⚠️ [Categories] saveItemChanges() - No hay orderId, actualizando solo en frontend...');
                            item.quantity = quantity;
                            item.notes = notes;
                            
                            console.log('✅ [Categories] saveItemChanges() - Item actualizado sin backend');
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
                        console.error('❌ [Categories] saveItemChanges() - No se puede determinar el tipo de item');
                        Swal.fire('Error', 'No se pudo determinar el tipo de item. Contacte al administrador.', 'error');
                        return;
                    }
                } catch (error) {
                    console.error('❌ [Categories] saveItemChanges() - Error al actualizar item:', error);
                    Swal.fire('Error', error.message || 'No se pudo actualizar el item', 'error');
                    return;
                }
            } else {
                console.error('❌ [Categories] saveItemChanges() - Item no encontrado con ID:', itemId);
            }
        } else {
            console.log('🆕 [Categories] saveItemChanges() - Creando nuevo item...');
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
            
            console.log('📦 [Categories] saveItemChanges() - Nuevo item creado:', newItem);
            
            currentOrder.items.unshift(newItem);
            console.log('📊 [Categories] saveItemChanges() - Total items en orden:', currentOrder.items.length);
            
            // Actualizar contador en la card del producto
            const quantityElement = document.getElementById(`quantity-${productId}`);
            if (quantityElement) {
                const currentQuantity = parseInt(quantityElement.textContent) || 0;
                quantityElement.textContent = currentQuantity + quantity;
                console.log('✅ [Categories] saveItemChanges() - Contador actualizado:', currentQuantity + quantity);
            } else {
                console.log('⚠️ [Categories] saveItemChanges() - No se encontró elemento quantity-${productId}');
            }
            
            // Resaltar la card del producto
            const productCard = document.querySelector(`[data-product-id="${productId}"]`);
            if (productCard) {
                productCard.classList.add('selected-product');
                console.log('✅ [Categories] saveItemChanges() - Card del producto resaltada');
            } else {
                console.log('⚠️ [Categories] saveItemChanges() - No se encontró card del producto');
            }
            
            console.log('✅ [Categories] saveItemChanges() - Producto agregado exitosamente:', {
                productName,
                quantity,
                totalItems: currentOrder.items.length
            });
            
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
        
        console.log('✅ [Categories] saveItemChanges() - Proceso completado exitosamente');
    } catch (error) {
        console.error('❌ [Categories] saveItemChanges() - Error general:', error);
        Swal.fire('Error', 'No se pudo guardar los cambios: ' + error.message, 'error');
    }
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