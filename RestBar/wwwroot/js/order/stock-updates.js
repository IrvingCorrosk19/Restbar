// Stock Updates - Sistema de actualizaciones de stock en tiempo real

// Inicializar conexión SignalR para actualizaciones de stock
function initializeStockUpdates() {
    if (typeof signalR === 'undefined') {
        return;
    }

    // Crear conexión SignalR para stock
    const stockConnection = new signalR.HubConnectionBuilder()
        .withUrl('/orderHub')
        .withAutomaticReconnect()
        .build();

    // Configurar eventos de conexión
    stockConnection.onreconnecting((error) => {
        // Reconectando SignalR para stock
    });

    stockConnection.onreconnected((connectionId) => {
        // Reconectado SignalR para stock
        joinStockGroup(stockConnection);
    });

    stockConnection.onclose((error) => {
        // Conexión SignalR cerrada para stock
    });

    // Configurar eventos de stock
    stockConnection.on('StockReduced', (data) => {
        handleStockReduced(data);
    });

    stockConnection.on('StockUpdated', (data) => {
        handleStockUpdated(data);
    });

    // Unirse al grupo de actualizaciones de stock
    function joinStockGroup(connection) {
        if (connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke('JoinStockGroup')
                .then(() => {
                    // Unido al grupo de actualizaciones de stock
                })
                .catch((error) => {
                    // Error al unirse al grupo de stock
                });
        }
    }

    // Manejar reducción de stock
    function handleStockReduced(data) {
        const { productId, productName, oldStock, newStock, quantityReduced } = data;
        
        // Actualizar stock en la UI
        updateStockInUI(productId, productName, oldStock, newStock, quantityReduced);
    }

    // Manejar actualización de stock
    function handleStockUpdated(data) {
        const { productId, productName, newStock } = data;
        
        // Actualizar stock en la UI
        updateStockInUI(productId, productName, null, newStock, null);
    }

    // Actualizar stock en la UI
    function updateStockInUI(productId, productName, oldStock, newStock, quantityReduced) {
        // Buscar la card del producto
        const productCard = document.querySelector(`[data-product-id="${productId}"]`);
        
        if (productCard) {
            // Buscar elementos de stock en la card
            const stockElement = productCard.querySelector('.stock-display');
            const stockBadge = productCard.querySelector('.stock-badge');
            const stockText = productCard.querySelector('.stock-text');
            
            if (stockElement) {
                stockElement.textContent = newStock;
            }
            
            if (stockBadge) {
                // Actualizar clase del badge según el stock
                stockBadge.className = 'stock-badge badge ' + getStockBadgeClass(newStock);
            }
            
            if (stockText) {
                stockText.textContent = `Stock: ${newStock}`;
            }
            
            // Agregar efecto visual temporal
            productCard.style.transition = 'all 0.3s ease';
            productCard.style.backgroundColor = '#fff3cd';
            
            setTimeout(() => {
                productCard.style.backgroundColor = '';
            }, 2000);
            
            // Mostrar notificación si es necesario
            if (quantityReduced && quantityReduced > 0) {
                showStockNotification(productName, oldStock, newStock, quantityReduced);
            }
        }
    }

    // Obtener clase del badge según el stock
    function getStockBadgeClass(stock) {
        if (stock <= 0) {
            return 'badge-danger';
        } else if (stock <= 5) {
            return 'badge-warning';
        } else if (stock <= 10) {
            return 'badge-info';
        } else {
            return 'badge-success';
        }
    }

    // Mostrar notificación de stock
    function showStockNotification(productName, oldStock, newStock, quantityReduced) {
        const message = `Stock de ${productName} reducido: ${oldStock} → ${newStock} (${quantityReduced} unidades)`;
        
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'Stock Actualizado',
                text: message,
                icon: newStock <= 5 ? 'warning' : 'info',
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true
            });
        }
    }

    // Función para desconectarse del grupo de stock
    function leaveStockGroup() {
        if (stockConnection.state === signalR.HubConnectionState.Connected) {
            stockConnection.invoke('LeaveStockGroup')
                .then(() => {
                    // Desconectado del grupo de actualizaciones de stock
                })
                .catch((error) => {
                    // Error al desconectarse del grupo de stock
                });
        }
    }

    // Función para recargar productos después de actualización de stock
    function reloadProductsAfterStockUpdate() {
        // Recargar productos después de actualización de stock
        if (typeof loadCategories === 'function') {
            loadCategories();
        }
    }

    // Iniciar conexión
    stockConnection.start()
        .then(() => {
            // Conexión SignalR establecida para actualizaciones de stock
            joinStockGroup(stockConnection);
        })
        .catch((error) => {
            // Error al conectar SignalR para stock
        });

    // Exponer funciones globalmente
    window.stockUpdates = {
        leaveGroup: leaveStockGroup,
        reloadProducts: reloadProductsAfterStockUpdate
    };
}

// Inicializar cuando el DOM esté listo
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeStockUpdates);
} else {
    initializeStockUpdates();
} 