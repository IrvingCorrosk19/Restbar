// Debug SignalR - Script para verificar el funcionamiento del sistema

// Función para verificar el estado de SignalR
function checkSignalRStatus() {
    // Verificar estado de SignalR
}

// Función para verificar el estado de las mesas
function checkTablesStatus() {
    const tableButtons = document.querySelectorAll('.mesa-btn');
    
    tableButtons.forEach((btn, index) => {
        const tableId = btn.dataset.id;
        const status = btn.dataset.status;
        const classes = btn.className;
    });
}

// Función para simular cambio de estado de mesa
function simulateTableStatusChange(tableId, newStatus) {
    if (typeof updateTableStatus === 'function') {
        updateTableStatus(tableId, newStatus);
    }
}

// Función para verificar la conexión a grupos de SignalR
function checkSignalRGroups() {
    if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
        // SignalR conectado, puede unirse a grupos
    }
}

// Función para probar el flujo completo
function testCompleteFlow() {
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
        simulateTableStatusChange(tableId, 'EnPreparacion');
    }
}

// Función para monitorear eventos de SignalR
function monitorSignalREvents() {
    if (signalRConnection) {
        // Agregar listeners temporales para monitorear
        const originalTableStatusChanged = signalRConnection.on;
        
        signalRConnection.on = function(eventName, handler) {
            return originalTableStatusChanged.call(this, eventName, handler);
        };
    }
}

// Función para verificar logs en tiempo real
function enableRealTimeLogging() {
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

// Ejecutar verificación automática después de 3 segundos
setTimeout(() => {
    checkSignalRStatus();
    checkTablesStatus();
}, 3000); 