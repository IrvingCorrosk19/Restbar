// Test Multi-Screen - Script para verificar actualización en múltiples pantallas

console.log('[MultiScreen] ===== PRUEBA DE ACTUALIZACIÓN EN MÚLTIPLES PANTALLAS =====');

// Función para simular notificación SignalR desde otra pantalla
function simulateSignalRNotification(tableId, newStatus) {
    console.log(`[MultiScreen] Simulando notificación SignalR: ${tableId} -> ${newStatus}`);
    
    // Simular el evento SignalR
    if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
        // Disparar el evento manualmente
        const event = new CustomEvent('tableStatusChanged', {
            detail: { tableId, newStatus }
        });
        document.dispatchEvent(event);
        
        // También llamar directamente a la función
        if (typeof updateTableStatus === 'function') {
            updateTableStatus(tableId, newStatus);
        }
    } else {
        console.error('[MultiScreen] SignalR no está conectado');
    }
}

// Función para verificar el estado actual de todas las mesas
function checkAllTablesStatus() {
    console.log('[MultiScreen] Estado actual de todas las mesas:');
    const tableButtons = document.querySelectorAll('.mesa-btn');
    
    tableButtons.forEach((btn, index) => {
        const tableId = btn.dataset.id;
        const status = btn.dataset.status;
        const classes = btn.className;
        const hasEnPreparacion = classes.includes('mesa-en-preparacion');
        const hasDisponible = classes.includes('mesa-disponible');
        
        console.log(`[MultiScreen] Mesa ${index + 1}:`);
        console.log(`[MultiScreen]   - ID: ${tableId}`);
        console.log(`[MultiScreen]   - Status: ${status}`);
        console.log(`[MultiScreen]   - Classes: ${classes}`);
        console.log(`[MultiScreen]   - EnPreparacion: ${hasEnPreparacion ? '✅' : '❌'}`);
        console.log(`[MultiScreen]   - Disponible: ${hasDisponible ? '✅' : '❌'}`);
    });
}

// Función para probar el flujo completo de múltiples pantallas
function testMultiScreenFlow() {
    console.log('[MultiScreen] ===== INICIANDO PRUEBA DE MÚLTIPLES PANTALLAS =====');
    
    // 1. Verificar estado inicial
    console.log('[MultiScreen] 1. Estado inicial:');
    checkAllTablesStatus();
    
    // 2. Simular cambio de estado
    const tableButtons = document.querySelectorAll('.mesa-btn');
    if (tableButtons.length > 0) {
        const testTable = tableButtons[0];
        const tableId = testTable.dataset.id;
        const currentStatus = testTable.dataset.status;
        
        console.log(`[MultiScreen] 2. Simulando cambio de estado:`);
        console.log(`[MultiScreen]   - Mesa: ${tableId}`);
        console.log(`[MultiScreen]   - Estado actual: ${currentStatus}`);
        console.log(`[MultiScreen]   - Nuevo estado: EnPreparacion`);
        
        // Simular notificación
        simulateSignalRNotification(tableId, 'EnPreparacion');
        
        // 3. Verificar estado después del cambio
        setTimeout(() => {
            console.log('[MultiScreen] 3. Estado después del cambio:');
            checkAllTablesStatus();
            
            // 4. Simular cambio de vuelta
            setTimeout(() => {
                console.log('[MultiScreen] 4. Simulando cambio de vuelta a Disponible:');
                simulateSignalRNotification(tableId, 'Disponible');
                
                setTimeout(() => {
                    console.log('[MultiScreen] 5. Estado final:');
                    checkAllTablesStatus();
                    console.log('[MultiScreen] ===== PRUEBA COMPLETADA =====');
                }, 1000);
            }, 2000);
        }, 1000);
    } else {
        console.error('[MultiScreen] No se encontraron mesas para probar');
    }
}

// Función para verificar conectividad SignalR
function checkSignalRConnectivity() {
    console.log('[MultiScreen] Verificando conectividad SignalR:');
    console.log(`[MultiScreen]   - SignalR conectado: ${signalRConnection ? '✅' : '❌'}`);
    if (signalRConnection) {
        console.log(`[MultiScreen]   - Estado: ${signalRConnection.state}`);
        console.log(`[MultiScreen]   - URL: ${signalRConnection.baseUrl}`);
    }
}

// Función para forzar actualización de una mesa específica
function forceUpdateTable(tableId, newStatus) {
    console.log(`[MultiScreen] Forzando actualización: ${tableId} -> ${newStatus}`);
    
    // Buscar el botón
    const tableButton = document.querySelector(`.mesa-btn[data-id='${tableId}']`);
    if (!tableButton) {
        console.error(`[MultiScreen] No se encontró la mesa ${tableId}`);
        return;
    }
    
    // Actualizar manualmente
    const statusClasses = [
        'mesa-disponible', 'mesa-ocupada', 'mesa-reservada', 'mesa-en-espera', 
        'mesa-atendida', 'mesa-en-preparacion', 'mesa-servida', 'mesa-para-pago', 
        'mesa-pagada', 'mesa-bloqueada'
    ];
    tableButton.classList.remove(...statusClasses);
    
    const newStatusClass = getTableStatusClass(newStatus);
    tableButton.classList.add(newStatusClass);
    tableButton.dataset.status = newStatus;
    
    console.log(`[MultiScreen] ✅ Mesa actualizada manualmente: ${newStatusClass}`);
}

// Exponer funciones para uso en consola
window.multiScreenTest = {
    simulateNotification: simulateSignalRNotification,
    checkStatus: checkAllTablesStatus,
    testFlow: testMultiScreenFlow,
    checkConnectivity: checkSignalRConnectivity,
    forceUpdate: forceUpdateTable
};

console.log('[MultiScreen] Funciones disponibles:');
console.log('[MultiScreen]   - multiScreenTest.simulateNotification(tableId, status)');
console.log('[MultiScreen]   - multiScreenTest.checkStatus()');
console.log('[MultiScreen]   - multiScreenTest.testFlow()');
console.log('[MultiScreen]   - multiScreenTest.checkConnectivity()');
console.log('[MultiScreen]   - multiScreenTest.forceUpdate(tableId, status)');

// Ejecutar verificación automática
setTimeout(() => {
    console.log('[MultiScreen] Ejecutando verificación automática...');
    checkSignalRConnectivity();
    checkAllTablesStatus();
}, 2000); 