// Test Loading Script - Verificar que todos los módulos se cargan correctamente

console.log('[Test] ===== VERIFICACIÓN DE CARGA DE MÓDULOS =====');

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

console.log('[Test] Verificando funciones requeridas:');
requiredFunctions.forEach(funcName => {
    if (typeof window[funcName] === 'function') {
        console.log(`[Test] ✅ ${funcName} - DISPONIBLE`);
    } else {
        console.log(`[Test] ❌ ${funcName} - NO DISPONIBLE`);
    }
});

// Verificar variables globales
console.log('[Test] Verificando variables globales:');
console.log('[Test] currentOrder:', typeof currentOrder !== 'undefined' ? '✅ DISPONIBLE' : '❌ NO DISPONIBLE');
console.log('[Test] signalRConnection:', typeof signalRConnection !== 'undefined' ? '✅ DISPONIBLE' : '❌ NO DISPONIBLE');

// Verificar elementos del DOM
console.log('[Test] Verificando elementos del DOM:');
const requiredElements = [
    'tables',
    'categories',
    'products',
    'orderItems',
    'orderTotal',
    'sendToKitchen',
    'signalrStatus'
];

requiredElements.forEach(elementId => {
    const element = document.getElementById(elementId);
    if (element) {
        console.log(`[Test] ✅ ${elementId} - ENCONTRADO`);
    } else {
        console.log(`[Test] ❌ ${elementId} - NO ENCONTRADO`);
    }
});

// Función para probar la actualización de mesa
window.testTableUpdate = function(tableId, newStatus) {
    console.log(`[Test] Probando actualización de mesa: ${tableId} -> ${newStatus}`);
    if (typeof updateTableStatus === 'function') {
        updateTableStatus(tableId, newStatus);
    } else {
        console.error('[Test] updateTableStatus no está disponible');
    }
};

// Función para simular notificación SignalR
window.testSignalRNotification = function(tableId, newStatus) {
    console.log(`[Test] Simulando notificación SignalR: ${tableId} -> ${newStatus}`);
    if (typeof updateTableStatus === 'function') {
        // Simular el evento SignalR
        updateTableStatus(tableId, newStatus);
    } else {
        console.error('[Test] updateTableStatus no está disponible');
    }
};

console.log('[Test] ===== VERIFICACIÓN COMPLETADA =====');
console.log('[Test] Para probar actualización de mesa, usa: testTableUpdate("table-id", "EnPreparacion")');
console.log('[Test] Para simular SignalR, usa: testSignalRNotification("table-id", "EnPreparacion")');

// Script de prueba para verificar carga de funciones
console.log('[Test Loading] Verificando carga de funciones...');

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
    if (typeof window[funcName] === 'function') {
        console.log(`[Test Loading] ✅ ${funcName} está disponible`);
    } else {
        console.error(`[Test Loading] ❌ ${funcName} NO está disponible`);
        allFunctionsLoaded = false;
    }
});

if (allFunctionsLoaded) {
    console.log('[Test Loading] ✅ Todas las funciones están cargadas correctamente');
} else {
    console.error('[Test Loading] ❌ Algunas funciones no están disponibles');
}

// Verificar variables globales importantes
console.log('[Test Loading] Verificando variables globales...');
console.log('[Test Loading] currentOrder:', typeof currentOrder !== 'undefined' ? 'disponible' : 'NO disponible');
console.log('[Test Loading] guid:', typeof guid !== 'undefined' ? 'disponible' : 'NO disponible');

// Verificar elementos del DOM
document.addEventListener('DOMContentLoaded', function() {
    console.log('[Test Loading] Verificando elementos del DOM...');
    const importantElements = [
        'orderItems',
        'sendToKitchen',
        'clearOrderBtn',
        'cancelOrder',
        'orderTotal',
        'itemCount'
    ];
    
    importantElements.forEach(elementId => {
        const element = document.getElementById(elementId);
        if (element) {
            console.log(`[Test Loading] ✅ Elemento ${elementId} encontrado`);
        } else {
            console.error(`[Test Loading] ❌ Elemento ${elementId} NO encontrado`);
        }
    });
});

// ✅ Test-loading cargado correctamente - v2.0
console.log('[Test Loading] ✅ Módulo de prueba cargado correctamente v2.0');