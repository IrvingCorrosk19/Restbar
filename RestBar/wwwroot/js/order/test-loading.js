// Test Loading Script - Verificar que todos los módulos se cargan correctamente

// Verificar que las funciones principales están disponibles
const requiredFunctions = [
    'initializeSignalR',
    'loadTables', 
    'loadCategories',
    'handleTableClick',
    'sendToKitchen',
    'updateTableStatus',
    'updateTableUI',
    'getTableStatusClass',
    'getStatusDescription'
];

// Verificar variables globales
const currentOrderAvailable = typeof currentOrder !== 'undefined';
const signalRConnectionAvailable = typeof signalRConnection !== 'undefined';

// Verificar elementos del DOM
const requiredElements = [
    'tables',
    'categories',
    'products',
    'orderItems',
    'orderTotal',
    'sendToKitchen',
    'signalrStatus'
];

// Función para probar la actualización de mesa
window.testTableUpdate = function(tableId, newStatus) {
    if (typeof updateTableStatus === 'function') {
        updateTableStatus(tableId, newStatus);
    }
};

// Función para simular notificación SignalR
window.testSignalRNotification = function(tableId, newStatus) {
    if (typeof updateTableStatus === 'function') {
        // Simular el evento SignalR
        updateTableStatus(tableId, newStatus);
    }
};

// Script de prueba para verificar carga de funciones

// Lista de funciones que deben estar disponibles
const requiredOrderFunctions = [
    'increaseQuantity',
    'decreaseQuantity',
    'updateOrderUI',
    'enableConfirmButton',
    'disableConfirmButton',
    'getStatusDisplay',
    'getStatusDisplayText',
    'getStatusBadgeClass',
    'showQuantityUpdateFeedback'
];

// Verificar que todas las funciones estén disponibles
let allFunctionsLoaded = true;
requiredOrderFunctions.forEach(funcName => {
    if (typeof window[funcName] !== 'function') {
        allFunctionsLoaded = false;
    }
});