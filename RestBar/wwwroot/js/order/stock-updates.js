// Stock Updates Management via SignalR
// Maneja las actualizaciones de stock en tiempo real

let stockUpdateConnection = null;

// Inicializar conexión SignalR para actualizaciones de stock
function initializeStockUpdates() {
    try {
        console.log('[StockUpdates] Inicializando conexión SignalR para actualizaciones de stock...');
        
        stockUpdateConnection = new signalR.HubConnectionBuilder()
            .withUrl("/orderHub")
            .withAutomaticReconnect()
            .build();

        // Configurar handlers para eventos de stock
        setupStockUpdateHandlers();

        // Iniciar conexión
        stockUpdateConnection.start()
            .then(() => {
                console.log('[StockUpdates] ✅ Conexión SignalR establecida para actualizaciones de stock');
                joinStockUpdatesGroup();
            })
            .catch(err => {
                console.error('[StockUpdates] ❌ Error al conectar SignalR para stock:', err);
            });

    } catch (error) {
        console.error('[StockUpdates] ❌ Error al inicializar SignalR para stock:', error);
    }
}

// Configurar handlers para eventos de stock
function setupStockUpdateHandlers() {
    if (!stockUpdateConnection) return;

    // Handler para cuando se reduce el stock
    stockUpdateConnection.on("StockReduced", (data) => {
        console.log('[StockUpdates] 📦 Stock reducido recibido:', data);
        updateProductStockInUI(data);
    });

    // Handler para cuando se actualiza el stock
    stockUpdateConnection.on("StockUpdated", (data) => {
        console.log('[StockUpdates] 📦 Stock actualizado recibido:', data);
        updateProductStockInUI(data);
    });

    // Handler para reconexión
    stockUpdateConnection.onreconnecting((error) => {
        console.log('[StockUpdates] 🔄 Reconectando SignalR para stock...', error);
    });

    stockUpdateConnection.onreconnected((connectionId) => {
        console.log('[StockUpdates] ✅ Reconectado SignalR para stock. ConnectionId:', connectionId);
        joinStockUpdatesGroup();
    });

    stockUpdateConnection.onclose((error) => {
        console.log('[StockUpdates] ❌ Conexión SignalR cerrada para stock:', error);
    });
}

// Unirse al grupo de actualizaciones de stock
function joinStockUpdatesGroup() {
    if (stockUpdateConnection && stockUpdateConnection.state === signalR.HubConnectionState.Connected) {
        stockUpdateConnection.invoke("JoinStockUpdatesGroup")
            .then(() => {
                console.log('[StockUpdates] ✅ Unido al grupo de actualizaciones de stock');
            })
            .catch(err => {
                console.error('[StockUpdates] ❌ Error al unirse al grupo de stock:', err);
            });
    }
}

// Actualizar el stock de un producto en la UI
function updateProductStockInUI(data) {
    try {
        const productId = data.productId || data.ProductId;
        const productName = data.productName || data.ProductName;
        const newStock = data.newStock || data.NewStock;
        const oldStock = data.oldStock || data.OldStock;
        const quantityReduced = data.quantityReduced || data.QuantityReduced;

        console.log(`[StockUpdates] 🔄 Actualizando stock para ${productName}: ${oldStock} -> ${newStock} (reducido: ${quantityReduced})`);

        // Buscar la card del producto
        const productCard = document.querySelector(`[data-product-id="${productId}"]`);
        if (!productCard) {
            console.log(`[StockUpdates] ⚠️ Card del producto ${productName} no encontrada en la UI`);
            return;
        }

        // Actualizar el badge de stock
        const stockBadge = productCard.querySelector('.badge');
        if (stockBadge) {
            if (newStock > 0) {
                stockBadge.className = 'badge bg-success';
                stockBadge.textContent = `Stock: ${newStock}`;
            } else {
                stockBadge.className = 'badge bg-danger';
                stockBadge.textContent = 'Sin stock';
            }
        }

        // Actualizar los botones según el stock
        const addButtons = productCard.querySelectorAll('button');
        addButtons.forEach(button => {
            if (newStock <= 0) {
                button.disabled = true;
                if (button.textContent.includes('Agregar')) {
                    button.textContent = 'Sin stock';
                }
            } else {
                button.disabled = false;
                if (button.textContent === 'Sin stock') {
                    button.textContent = '+ Agregar';
                }
            }
        });

        // Mostrar notificación visual
        showStockUpdateNotification(productName, oldStock, newStock, quantityReduced);

        console.log(`[StockUpdates] ✅ Stock actualizado para ${productName} en la UI`);

    } catch (error) {
        console.error('[StockUpdates] ❌ Error al actualizar stock en UI:', error);
    }
}

// Mostrar notificación visual de cambio de stock
function showStockUpdateNotification(productName, oldStock, newStock, quantityReduced) {
    try {
        // Crear notificación toast
        const toast = document.createElement('div');
        toast.className = 'toast align-items-center text-white bg-info border-0';
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-box me-2"></i>
                    <strong>${productName}</strong><br>
                    Stock: ${oldStock} → ${newStock} 
                    <small class="text-light">(-${quantityReduced})</small>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        // Agregar al contenedor de toasts
        let toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toastContainer';
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        toastContainer.appendChild(toast);

        // Mostrar el toast
        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 3000
        });
        bsToast.show();

        // Remover el toast después de que se oculte
        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });

    } catch (error) {
        console.error('[StockUpdates] ❌ Error al mostrar notificación de stock:', error);
    }
}

// Función para desconectar SignalR de stock
function disconnectStockUpdates() {
    if (stockUpdateConnection) {
        stockUpdateConnection.invoke("LeaveStockUpdatesGroup")
            .then(() => {
                console.log('[StockUpdates] ✅ Desconectado del grupo de actualizaciones de stock');
            })
            .catch(err => {
                console.error('[StockUpdates] ❌ Error al desconectarse del grupo de stock:', err);
            });

        stockUpdateConnection.stop();
        stockUpdateConnection = null;
    }
}

// Función para recargar productos después de actualización de stock
function reloadProductsAfterStockUpdate() {
    if (selectedCategoryId) {
        console.log('[StockUpdates] 🔄 Recargando productos después de actualización de stock...');
        loadProducts(selectedCategoryId);
    }
}

// Exportar funciones para uso global
window.initializeStockUpdates = initializeStockUpdates;
window.disconnectStockUpdates = disconnectStockUpdates;
window.reloadProductsAfterStockUpdate = reloadProductsAfterStockUpdate; 