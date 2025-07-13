// 🚀 SISTEMA DE CUENTAS SEPARADAS - RestBar (Versión Simple)
console.log('🔍 [SeparateAccounts] Script cargado correctamente');

// Variables globales
let currentPersons = [];
let currentOrderId = null;

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE GESTIÓN DE PERSONAS
function showPersonsManagementModal() {
    try {
        console.log('🔍 [SeparateAccounts] showPersonsManagementModal() - Función llamada correctamente');
        
        if (!currentOrderId) {
            console.warn('⚠️ [SeparateAccounts] No hay orden actual');
            Swal.fire('Error', 'No hay orden actual para gestionar personas', 'error');
            return;
        }

        // Modal simple de prueba
        Swal.fire({
            title: 'Cuentas Separadas',
            html: `
                <div class="text-center">
                    <p>Sistema de cuentas separadas funcionando correctamente</p>
                    <p><strong>Orden ID:</strong> ${currentOrderId}</p>
                </div>
            `,
            icon: 'success',
            confirmButtonText: 'Cerrar'
        });

        console.log('✅ [SeparateAccounts] Modal mostrado exitosamente');
    } catch (error) {
        console.error('❌ [SeparateAccounts] Error:', error);
        Swal.fire('Error', 'Error al mostrar modal de gestión de personas', 'error');
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: INICIALIZAR SISTEMA DE CUENTAS SEPARADAS
function initializeSeparateAccounts(orderId) {
    try {
        console.log('🔍 [SeparateAccounts] initializeSeparateAccounts() - Inicializando para orden:', orderId);
        currentOrderId = orderId;
        console.log('✅ [SeparateAccounts] Sistema inicializado correctamente');
    } catch (error) {
        console.error('❌ [SeparateAccounts] Error al inicializar:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: OBTENER RESUMEN DE CUENTAS SEPARADAS
function getSeparateAccountsSummary() {
    try {
        console.log('🔍 [SeparateAccounts] getSeparateAccountsSummary() - Obteniendo resumen...');
        return {
            success: true,
            data: {
                persons: currentPersons,
                orderId: currentOrderId
            }
        };
    } catch (error) {
        console.error('❌ [SeparateAccounts] Error al obtener resumen:', error);
        return null;
    }
}

// Exportar funciones globales
window.showPersonsManagementModal = showPersonsManagementModal;
window.initializeSeparateAccounts = initializeSeparateAccounts;
window.getSeparateAccountsSummary = getSeparateAccountsSummary;

// 🔍 DEBUG: Verificar que las funciones se exportaron correctamente
console.log('🔍 [SeparateAccounts] Funciones exportadas:');
console.log('  - showPersonsManagementModal:', typeof window.showPersonsManagementModal);
console.log('  - initializeSeparateAccounts:', typeof window.initializeSeparateAccounts);
console.log('  - getSeparateAccountsSummary:', typeof window.getSeparateAccountsSummary);
