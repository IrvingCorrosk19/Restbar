// ============================================================
// UTILIDADES GENERALES PARA EL SISTEMA DE ÓRDENES
// ============================================================

// Función para parsear JSON de forma segura
function safeJsonParse(jsonString, defaultValue = {}) {
    try {
        return JSON.parse(jsonString);
    } catch (e) {
        
        return defaultValue;
    }
}

// Función para formatear moneda
function formatCurrency(amount) {
    try {
        const numAmount = typeof amount === 'string' ? parseFloat(amount) : amount;
        return new Intl.NumberFormat('es-CR', {
            style: 'currency',
            currency: 'CRC'
        }).format(numAmount);
    } catch (e) {
        
        return `₡${amount || 0}`;
    }
}

// Función para generar GUIDs únicos (declaración al final del archivo)

// Función para mostrar mensajes de error
function showError(message, title = 'Error') {
    
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: title,
            text: message,
            icon: 'error',
            confirmButtonText: 'Entendido'
        });
    } else {
        alert(`${title}: ${message}`);
    }
}

// Función para mostrar mensajes de éxito
function showSuccess(message, title = 'Éxito') {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: title,
            text: message,
            icon: 'success',
            timer: 3000,
            timerProgressBar: true,
            confirmButtonText: 'Entendido'
        });
    } else {
        alert(`${title}: ${message}`);
    }
}

// ============================================================
// GESTIÓN DE USUARIO AUTENTICADO
// ============================================================

// Variable global para almacenar información del usuario actual
let currentUser = null;

// Función para obtener el usuario actual desde la API
async function getCurrentUser() {
    try {
        if (currentUser) {
            return currentUser;
        }

        const response = await fetch('/Auth/CurrentUser', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            if (response.status === 401) {
                window.location.href = '/Auth/Login';
                return null;
            }
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();
        
        if (result.success && result.user) {
            currentUser = result.user;
            return currentUser;
        } else {
            
            return null;
        }
    } catch (error) {
        
        return null;
    }
}

// Función para obtener el ID del usuario actual
async function getCurrentUserId() {
    try {
        const user = await getCurrentUser();
        return user ? user.id : null;
    } catch (error) {
        
        return null;
    }
}

// Función para verificar permisos del usuario
async function checkUserPermission(action) {
    try {
        const response = await fetch('/Auth/CheckPermission', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify({ action: action })
        });

        if (!response.ok) {
            return false;
        }

        const result = await response.json();
        return result.success && result.hasPermission;
    } catch (error) {
        return false;
    }
}

// Función para limpiar información del usuario (logout)
function clearCurrentUser() {
    currentUser = null;
}

// Función para mostrar información del usuario en la UI
function displayUserInfo() {
    if (!currentUser) {
        return;
    }

    // Mostrar nombre del usuario en la barra de navegación si existe
    const userNameElement = document.getElementById('current-user-name');
    if (userNameElement) {
        userNameElement.textContent = currentUser.fullName || currentUser.email;
    }

    // Mostrar rol del usuario si existe
    const userRoleElement = document.getElementById('current-user-role');
    if (userRoleElement) {
        userRoleElement.textContent = currentUser.role;
    }
}

// Inicializar información del usuario cuando se carga la página
document.addEventListener('DOMContentLoaded', async function() {
    await getCurrentUser();
    displayUserInfo();
});

// Utility Functions and Helpers

// Obtener descripción del estado
function getStatusDescription(status) {
    switch (status) {
        case 'Disponible':
            return 'Mesa disponible para nuevos clientes';
        case 'Ocupada':
            return 'Mesa ocupada con clientes';
        case 'Reservada':
            return 'Mesa reservada para más tarde';
        case 'EnEspera':
            return 'Clientes esperando ser atendidos';
        case 'Atendida':
            return 'Clientes atendidos, tomando pedido';
        case 'EnPreparacion':
            return 'Pedido en preparación en cocina';
        case 'Servida':
            return 'Pedido servido a los clientes';
        case 'ParaPago':
            return 'Lista para cobrar';
        case 'Pagada':
            return 'Cuenta pagada';
        case 'Bloqueada':
            return 'Mesa bloqueada por mantenimiento';
        default:
            return 'Estado desconocido';
    }
}

// Obtener clase de botón según el estado
function getStatusButtonClass(status) {
    switch (status) {
        case 'Disponible':
            return 'btn-success';
        case 'Ocupada':
            return 'btn-warning';
        case 'Reservada':
            return 'btn-info';
        case 'EnEspera':
            return 'btn-secondary';
        case 'Atendida':
            return 'btn-primary';
        case 'EnPreparacion':
            return 'btn-warning';
        case 'Servida':
            return 'btn-info';
        case 'ParaPago':
            return 'btn-success';
        case 'Pagada':
            return 'btn-success';
        case 'Bloqueada':
            return 'btn-danger';
        default:
            return 'btn-secondary';
    }
}

// Función para obtener la clase CSS del badge según el status
function getStatusBadgeClass(status) {
    switch (status) {
        case 'Pending':
            return 'pending';
        case 'Preparing':
            return 'preparing';
        case 'Ready':
            return 'ready';
        case 'Served':
            return 'served';
        case 'Cancelled':
            return 'cancelled';
        default:
            return 'pending';
    }
}

// Función para obtener la clase CSS del badge de orden según el status
function getOrderStatusBadgeClass(status) {
    switch (status) {
        case 'Pending':
            return 'pending';
        case 'SentToKitchen':
            return 'sent-to-kitchen';
        case 'Preparing':
            return 'preparing';
        case 'Ready':
            return 'ready';
        case 'ReadyToPay':
            return 'ready-to-pay';
        case 'Served':
            return 'served';
        case 'Completed':
            return 'completed';
        case 'Cancelled':
            return 'cancelled';
        default:
            return 'pending';
    }
}

// Función para obtener la clase CSS de la fila según el status del item
function getOrderItemRowClass(status) {
    switch (status) {
        case 'Pending':
            return 'status-pending';
        case 'Preparing':
            return 'status-preparing';
        case 'Ready':
            return 'status-ready';
        case 'Served':
            return 'status-served';
        case 'Cancelled':
            return 'status-cancelled';
        default:
            return 'status-pending';
    }
}

// Función para obtener el texto de display del status
function getStatusDisplayText(status) {
    switch (status) {
        case 'Pending':
            return 'Pendiente';
        case 'Preparing':
            return 'Preparando';
        case 'Ready':
            return 'Listo';
        case 'Served':
            return 'Servido';
        case 'SentToKitchen':
            return 'Enviado a Cocina';
        case 'ReadyToPay':
            return 'Listo para Pagar';
        case 'Completed':
            return 'Completado';
        case 'Cancelled':
            return 'Cancelado';
        default:
            return status;
    }
}

// Función para obtener la descripción de los estados de los items de la orden
function getOrderItemStatusDescription(status) {
    switch (status) {
        case 'Pending':
            return 'Item pendiente de envío a cocina';
        case 'Preparing':
            return 'Item en preparación en cocina';
        case 'Ready':
            return 'Item listo para servir';
        case 'Served':
            return 'Item ya servido al cliente';
        case 'Cancelled':
            return 'Item cancelado';
        default:
            return 'Estado desconocido del item';
    }
}

// Función para obtener el color del estado
function getStatusColor(status) {
    switch (status) {
        case 'Pending':
            return 'bg-secondary';
        case 'Preparing':
            return 'bg-warning';
        case 'Ready':
            return 'bg-success';
        case 'Served':
            return 'bg-info';
        case 'SentToKitchen':
            return 'bg-primary';
        case 'ReadyToPay':
            return 'bg-success';
        case 'Completed':
            return 'bg-success';
        case 'Cancelled':
            return 'bg-danger';
        default:
            return 'bg-secondary';
    }
}

// Cargar supervisores para autorización de cancelación
async function loadSupervisors() {
    try {
        const response = await fetch('/User/GetSupervisors');
        const result = await response.json();
        if (result.success) {
            return result.data;
        } else {
            
            return [];
        }
    } catch (error) {
        
        return [];
    }
}

// ✅ FUNCIONES DINÁMICAS PARA ESTADOS

// Función para crear un badge de estado
function createStatusBadge(status, showDescription = false) {
    const badgeClass = getStatusBadgeClass(status);
    const displayText = getStatusDisplayText(status);
    const description = showDescription ? getOrderItemStatusDescription(status) : '';
    
    let badgeHtml = `<span class="order-item-status-badge ${badgeClass}" title="${description}">${displayText}</span>`;
    
    if (showDescription && description) {
        badgeHtml += `<small class="text-muted d-block mt-1">${description}</small>`;
    }
    
    return badgeHtml;
}

// Función para crear un badge de estado de orden
function createOrderStatusBadge(status, showDescription = false) {
    const badgeClass = getOrderStatusBadgeClass(status);
    const displayText = getStatusDisplayText(status);
    
    return `<span class="order-status-badge ${badgeClass}" title="${displayText}">${displayText}</span>`;
}

// Función para actualizar dinámicamente las clases de las filas de items
function updateOrderItemRowClasses() {
    const orderItemRows = document.querySelectorAll('.order-item-row');
    
    orderItemRows.forEach(row => {
        const statusCell = row.querySelector('.order-item-status-badge');
        if (statusCell) {
            const currentStatus = statusCell.textContent.trim();
            const rowClass = getOrderItemRowClass(currentStatus);
            
            // Remover clases de estado anteriores
            row.classList.remove('status-pending', 'status-preparing', 'status-ready', 'status-served', 'status-cancelled');
            
            // Agregar nueva clase de estado
            row.classList.add(rowClass);
        }
    });
}

// Función para actualizar dinámicamente los badges de estado
function updateStatusBadges() {
    const statusBadges = document.querySelectorAll('.order-item-status-badge');
    
    statusBadges.forEach(badge => {
        const currentStatus = badge.textContent.trim();
        const newClass = getStatusBadgeClass(currentStatus);
        
        // Remover clases anteriores
        badge.classList.remove('pending', 'preparing', 'ready', 'served', 'cancelled');
        
        // Agregar nueva clase
        badge.classList.add(newClass);
    });
}

// Función para actualizar dinámicamente los badges de estado de orden
function updateOrderStatusBadges() {
    const orderStatusBadges = document.querySelectorAll('.order-status-badge');
    
    orderStatusBadges.forEach(badge => {
        const currentStatus = badge.textContent.trim();
        const newClass = getOrderStatusBadgeClass(currentStatus);
        
        // Remover clases anteriores
        badge.classList.remove('pending', 'sent-to-kitchen', 'preparing', 'ready', 'ready-to-pay', 'served', 'completed', 'cancelled');
        
        // Agregar nueva clase
        badge.classList.add(newClass);
    });
}

// Función para aplicar estilos dinámicos a un elemento específico
function applyDynamicStyles(element, status, type = 'item') {
    if (type === 'order') {
        const badgeClass = getOrderStatusBadgeClass(status);
        element.className = `order-status-badge ${badgeClass}`;
    } else {
        const badgeClass = getStatusBadgeClass(status);
        element.className = `order-item-status-badge ${badgeClass}`;
    }
}

// Función para actualizar todos los estilos dinámicos
function updateAllDynamicStyles() {
    updateStatusBadges();
    updateOrderStatusBadges();
    updateOrderItemRowClasses();
}

// ✅ Función para generar GUID único para items individuales
function Guid() {
    this.newGuid = function() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random() * 16 | 0,
                v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };
}

// Instancia global de Guid
const guid = new Guid();

// ✅ Nueva función para obtener el display completo del status con badge
function getStatusDisplay(status) {
    const displayText = getStatusDisplayText(status);
    const badgeClass = getStatusBadgeClass(status);
    
    return `<span class="badge status-badge ${badgeClass}">${displayText}</span>`;
} 

// ✅ Utilities cargadas correctamente - v2.0