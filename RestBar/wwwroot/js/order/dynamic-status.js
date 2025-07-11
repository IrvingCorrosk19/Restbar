// ✅ SISTEMA DE ESTADOS DINÁMICOS
// Maneja las actualizaciones en tiempo real de estados con colores y animaciones

class DynamicStatusManager {
    constructor() {
        this.initializeEventListeners();
        this.updateInterval = null;
        this.startPeriodicUpdates();
    }

    // Inicializar event listeners
    initializeEventListeners() {
        // Escuchar cambios de estado desde SignalR
        if (typeof orderHub !== 'undefined') {
            orderHub.on('OrderStatusChanged', (orderId, newStatus) => {
                this.updateOrderStatus(orderId, newStatus);
            });

            orderHub.on('OrderItemStatusChanged', (orderId, itemId, newStatus) => {
                this.updateOrderItemStatus(orderId, itemId, newStatus);
            });
        }

        // Escuchar cambios en el DOM
        this.observeDOMChanges();
    }

    // Observar cambios en el DOM para aplicar estilos dinámicos
    observeDOMChanges() {
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'childList') {
                    mutation.addedNodes.forEach((node) => {
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            this.applyDynamicStylesToElement(node);
                        }
                    });
                }
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    // Aplicar estilos dinámicos a un elemento
    applyDynamicStylesToElement(element) {
        // Buscar badges de estado en el elemento
        const statusBadges = element.querySelectorAll('.order-item-status-badge, .order-status-badge');
        statusBadges.forEach(badge => {
            this.updateBadgeStyles(badge);
        });

        // Buscar filas de items
        const orderItemRows = element.querySelectorAll('.order-item-row');
        orderItemRows.forEach(row => {
            this.updateRowStyles(row);
        });
    }

    // Actualizar estilos de un badge específico
    updateBadgeStyles(badge) {
        const status = badge.textContent.trim();
        
        if (badge.classList.contains('order-item-status-badge')) {
            const newClass = getStatusBadgeClass(status);
            badge.className = `order-item-status-badge ${newClass}`;
        } else if (badge.classList.contains('order-status-badge')) {
            const newClass = getOrderStatusBadgeClass(status);
            badge.className = `order-status-badge ${newClass}`;
        }
    }

    // Actualizar estilos de una fila de item
    updateRowStyles(row) {
        const statusBadge = row.querySelector('.order-item-status-badge');
        if (statusBadge) {
            const status = statusBadge.textContent.trim();
            const rowClass = getOrderItemRowClass(status);
            
            // Remover clases anteriores
            row.classList.remove('status-pending', 'status-preparing', 'status-ready', 'status-served', 'status-cancelled');
            
            // Agregar nueva clase
            row.classList.add(rowClass);
        }
    }

    // Actualizar estado de orden
    updateOrderStatus(orderId, newStatus) {
        const orderElements = document.querySelectorAll(`[data-order-id="${orderId}"]`);
        
        orderElements.forEach(element => {
            const statusBadge = element.querySelector('.order-status-badge');
            if (statusBadge) {
                statusBadge.textContent = getStatusDisplayText(newStatus);
                this.updateBadgeStyles(statusBadge);
                
                // Agregar efecto de transición
                this.addTransitionEffect(statusBadge);
            }
        });
    }

    // Actualizar estado de item de orden
    updateOrderItemStatus(orderId, itemId, newStatus) {
        const itemElements = document.querySelectorAll(`[data-order-item-id="${itemId}"]`);
        
        itemElements.forEach(element => {
            const statusBadge = element.querySelector('.order-item-status-badge');
            if (statusBadge) {
                statusBadge.textContent = getStatusDisplayText(newStatus);
                this.updateBadgeStyles(statusBadge);
                this.updateRowStyles(element);
                
                // Agregar efecto de transición
                this.addTransitionEffect(statusBadge);
            }
        });
    }

    // Agregar efecto de transición
    addTransitionEffect(element) {
        element.style.transition = 'all 0.3s ease';
        element.style.transform = 'scale(1.1)';
        
        setTimeout(() => {
            element.style.transform = 'scale(1)';
        }, 300);
    }

    // Iniciar actualizaciones periódicas
    startPeriodicUpdates() {
        this.updateInterval = setInterval(() => {
            this.updateAllDynamicStyles();
        }, 5000); // Actualizar cada 5 segundos
    }

    // Detener actualizaciones periódicas
    stopPeriodicUpdates() {
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
            this.updateInterval = null;
        }
    }

    // Actualizar todos los estilos dinámicos
    updateAllDynamicStyles() {
        // Actualizar badges de items
        const itemBadges = document.querySelectorAll('.order-item-status-badge');
        itemBadges.forEach(badge => {
            this.updateBadgeStyles(badge);
        });

        // Actualizar badges de orden
        const orderBadges = document.querySelectorAll('.order-status-badge');
        orderBadges.forEach(badge => {
            this.updateBadgeStyles(badge);
        });

        // Actualizar filas de items
        const orderItemRows = document.querySelectorAll('.order-item-row');
        orderItemRows.forEach(row => {
            this.updateRowStyles(row);
        });
    }

    // Función para crear un badge de estado con estilos dinámicos
    createDynamicStatusBadge(status, type = 'item') {
        if (type === 'order') {
            return createOrderStatusBadge(status);
        } else {
            return createStatusBadge(status);
        }
    }

    // Función para actualizar un elemento específico
    updateElement(element, newStatus, type = 'item') {
        if (type === 'order') {
            element.innerHTML = createOrderStatusBadge(newStatus);
        } else {
            element.innerHTML = createStatusBadge(newStatus);
        }
        
        this.addTransitionEffect(element);
    }
}

// ✅ FUNCIONES DE UTILIDAD PARA ESTADOS DINÁMICOS

// Función para inicializar el sistema de estados dinámicos
function initializeDynamicStatusSystem() {
    if (typeof window.dynamicStatusManager === 'undefined') {
        window.dynamicStatusManager = new DynamicStatusManager();
        console.log('✅ Sistema de estados dinámicos inicializado');
    }
}

// Función para actualizar manualmente todos los estilos
function refreshAllDynamicStyles() {
    if (window.dynamicStatusManager) {
        window.dynamicStatusManager.updateAllDynamicStyles();
    }
}

// Función para actualizar un estado específico
function updateSpecificStatus(elementId, newStatus, type = 'item') {
    const element = document.getElementById(elementId);
    if (element && window.dynamicStatusManager) {
        window.dynamicStatusManager.updateElement(element, newStatus, type);
    }
}

// Función para agregar efectos de hover dinámicos
function addDynamicHoverEffects() {
    const statusBadges = document.querySelectorAll('.order-item-status-badge, .order-status-badge');
    
    statusBadges.forEach(badge => {
        badge.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px) scale(1.05)';
            this.style.boxShadow = '0 4px 12px rgba(0,0,0,0.3)';
        });
        
        badge.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
            this.style.boxShadow = '0 2px 4px rgba(0,0,0,0.1)';
        });
    });
}

// Función para crear un badge con animación de entrada
function createAnimatedStatusBadge(status, type = 'item') {
    const badgeHtml = type === 'order' ? 
        createOrderStatusBadge(status) : 
        createStatusBadge(status);
    
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = badgeHtml;
    const badge = tempDiv.firstElementChild;
    
    // Agregar animación de entrada
    badge.style.opacity = '0';
    badge.style.transform = 'scale(0.8)';
    
    setTimeout(() => {
        badge.style.transition = 'all 0.3s ease';
        badge.style.opacity = '1';
        badge.style.transform = 'scale(1)';
    }, 100);
    
    return badge;
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    initializeDynamicStatusSystem();
    addDynamicHoverEffects();
    
    // Actualizar estilos iniciales
    setTimeout(() => {
        refreshAllDynamicStyles();
    }, 1000);
});

// Exportar para uso global
window.DynamicStatusManager = DynamicStatusManager;
window.initializeDynamicStatusSystem = initializeDynamicStatusSystem;
window.refreshAllDynamicStyles = refreshAllDynamicStyles;
window.updateSpecificStatus = updateSpecificStatus;
window.createAnimatedStatusBadge = createAnimatedStatusBadge; 