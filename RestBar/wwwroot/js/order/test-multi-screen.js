// Test Multi Screen - Script para probar actualizaciones en múltiples pantallas

// Función para simular notificación SignalR
function simulateNotification(tableId, newStatus) {
    if (typeof updateTableStatus === 'function') {
        updateTableStatus(tableId, newStatus);
    }
}

// Función para verificar el estado actual de todas las mesas
function checkCurrentStatus() {
    const tableButtons = document.querySelectorAll('.mesa-btn');
    
    tableButtons.forEach((btn, index) => {
        const tableId = btn.dataset.id;
        const status = btn.dataset.status;
        const classes = btn.className;
        const hasEnPreparacion = classes.includes('btn-warning');
        const hasDisponible = classes.includes('btn-success');
    });
}

// Función para probar el flujo completo de actualización
function testUpdateFlow() {
    const tableButtons = document.querySelectorAll('.mesa-btn');
    if (tableButtons.length === 0) {
        return;
    }

    const firstTable = tableButtons[0];
    const tableId = firstTable.dataset.id;
    const currentStatus = firstTable.dataset.status;

    // 1. Estado inicial
    checkCurrentStatus();

    // 2. Simular cambio de estado
    simulateNotification(tableId, 'EnPreparacion');

    // 3. Estado después del cambio
    setTimeout(() => {
        checkCurrentStatus();
        
        // 4. Simulando cambio de vuelta a Disponible
        simulateNotification(tableId, 'Disponible');
        
        // 5. Estado final
        setTimeout(() => {
            checkCurrentStatus();
        }, 1000);
    }, 1000);
}

// Función para verificar conectividad SignalR
function checkConnectivity() {
    if (signalRConnection) {
        const state = signalRConnection.state;
        const url = signalRConnection.baseUrl;
    }
}

// Función para forzar actualización manual
function forceUpdate(tableId, newStatus) {
    const tableButton = document.querySelector(`[data-id="${tableId}"]`);
    if (tableButton) {
        const currentClasses = tableButton.className;
        let newStatusClass = '';
        
        switch (newStatus) {
            case 'Disponible':
                newStatusClass = 'btn-success';
                break;
            case 'Ocupada':
                newStatusClass = 'btn-warning';
                break;
            case 'EnPreparacion':
                newStatusClass = 'btn-warning';
                break;
            case 'Servida':
                newStatusClass = 'btn-info';
                break;
            case 'ParaPago':
                newStatusClass = 'btn-success';
                break;
            case 'Pagada':
                newStatusClass = 'btn-success';
                break;
            default:
                newStatusClass = 'btn-secondary';
        }
        
        // Limpiar clases de estado anteriores
        const cleanClasses = currentClasses.replace(/btn-(success|warning|info|danger|secondary)/g, '');
        tableButton.className = `${cleanClasses} ${newStatusClass}`.trim();
        tableButton.dataset.status = newStatus;
    }
}

// Exponer funciones para uso en consola
window.multiScreenTest = {
    simulateNotification: simulateNotification,
    checkStatus: checkCurrentStatus,
    testFlow: testUpdateFlow,
    checkConnectivity: checkConnectivity,
    forceUpdate: forceUpdate
};

// Ejecutar verificación automática después de 3 segundos
setTimeout(() => {
    checkConnectivity();
}, 3000); 