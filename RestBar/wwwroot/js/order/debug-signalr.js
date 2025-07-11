// Debug SignalR - Script para verificar el funcionamiento del sistema

console.log('[Debug] ===== VERIFICACIÓN DE SIGNALR Y ACTUALIZACIÓN DE MESAS =====');

// Función para verificar el estado de SignalR
function checkSignalRStatus() {
    console.log('[Debug] Estado de SignalR:');
    console.log('[Debug]   - signalRConnection:', signalRConnection ? '✅ CONECTADO' : '❌ NO CONECTADO');
    if (signalRConnection) {
        console.log('[Debug]   - Estado de conexión:', signalRConnection.state);
        console.log('[Debug]   - URL del hub:', signalRConnection.baseUrl);
    }
}

// Función para verificar el estado de las mesas
function checkTablesStatus() {
    console.log('[Debug] Estado de las mesas:');
    const tableButtons = document.querySelectorAll('.mesa-btn');
    console.log('[Debug]   - Total de mesas encontradas:', tableButtons.length);
    
    tableButtons.forEach((btn, index) => {
        const tableId = btn.dataset.id;
        const status = btn.dataset.status;
        const classes = btn.className;
        console.log(`[Debug]   Mesa ${index + 1}: ID=${tableId}, Status=${status}, Classes=${classes}`);
    });
}

// Función para simular cambio de estado de mesa
function simulateTableStatusChange(tableId, newStatus) {
    console.log(`[Debug] Simulando cambio de estado: ${tableId} -> ${newStatus}`);
    
    if (typeof updateTableStatus === 'function') {
        updateTableStatus(tableId, newStatus);
        console.log('[Debug] ✅ Función updateTableStatus ejecutada');
    } else {
        console.error('[Debug] ❌ Función updateTableStatus no disponible');
    }
}

// Función para verificar la conexión a grupos de SignalR
function checkSignalRGroups() {
    console.log('[Debug] Verificando grupos de SignalR:');
    console.log('[Debug]   - currentOrder.tableId:', currentOrder?.tableId);
    console.log('[Debug]   - currentOrder.orderId:', currentOrder?.orderId);
    
    if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
        console.log('[Debug] ✅ SignalR conectado, puede unirse a grupos');
    } else {
        console.log('[Debug] ❌ SignalR no conectado, no puede unirse a grupos');
    }
}

// Función para probar el flujo completo
function testCompleteFlow() {
    console.log('[Debug] ===== PRUEBA DE FLUJO COMPLETO =====');
    
    // 1. Verificar SignalR
    checkSignalRStatus();
    
    // 2. Verificar mesas
    checkTablesStatus();
    
    // 3. Verificar grupos
    checkSignalRGroups();
    
    // 4. Simular cambio de estado
    const tableButtons = document.querySelectorAll('.mesa-btn');
    if (tableButtons.length > 0) {
        const firstTable = tableButtons[0];
        const tableId = firstTable.dataset.id;
        console.log(`[Debug] Probando con mesa: ${tableId}`);
        simulateTableStatusChange(tableId, 'EnPreparacion');
    }
    
    console.log('[Debug] ===== PRUEBA COMPLETADA =====');
}

// Función para monitorear eventos de SignalR
function monitorSignalREvents() {
    console.log('[Debug] Monitoreando eventos de SignalR...');
    
    if (signalRConnection) {
        // Agregar listeners temporales para monitorear
        const originalTableStatusChanged = signalRConnection.on;
        
        signalRConnection.on = function(eventName, handler) {
            console.log(`[Debug] Evento SignalR registrado: ${eventName}`);
            return originalTableStatusChanged.call(this, eventName, handler);
        };
    }
}

// Función para verificar logs en tiempo real
function enableRealTimeLogging() {
    console.log('[Debug] Habilitando logs en tiempo real...');
    
    // Interceptar console.log para agregar timestamps
    const originalLog = console.log;
    console.log = function(...args) {
        const timestamp = new Date().toLocaleTimeString();
        originalLog.call(console, `[${timestamp}]`, ...args);
    };
}

// Exponer funciones para uso en consola
window.debugSignalR = {
    checkStatus: checkSignalRStatus,
    checkTables: checkTablesStatus,
    checkGroups: checkSignalRGroups,
    simulateChange: simulateTableStatusChange,
    testFlow: testCompleteFlow,
    monitorEvents: monitorSignalREvents,
    enableLogging: enableRealTimeLogging
};

console.log('[Debug] Funciones de debug disponibles:');
console.log('[Debug]   - debugSignalR.checkStatus() - Verificar estado de SignalR');
console.log('[Debug]   - debugSignalR.checkTables() - Verificar estado de mesas');
console.log('[Debug]   - debugSignalR.checkGroups() - Verificar grupos de SignalR');
console.log('[Debug]   - debugSignalR.simulateChange(tableId, status) - Simular cambio');
console.log('[Debug]   - debugSignalR.testFlow() - Prueba completa del flujo');
console.log('[Debug]   - debugSignalR.monitorEvents() - Monitorear eventos SignalR');
console.log('[Debug]   - debugSignalR.enableLogging() - Habilitar logs con timestamp');

// Ejecutar verificación automática después de 3 segundos
setTimeout(() => {
    console.log('[Debug] Ejecutando verificación automática...');
    checkSignalRStatus();
    checkTablesStatus();
}, 3000); 