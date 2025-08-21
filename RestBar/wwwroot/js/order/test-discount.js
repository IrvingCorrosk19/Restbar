// ✅ Script de prueba para debuggear el modal de descuento
console.log('[Test] Script de prueba de descuento cargado');

// Función para probar el modal de descuento
function testDiscountModal() {
    console.log('[Test] === PRUEBA DE MODAL DE DESCUENTO ===');
    
    // Verificar elementos
    const discountBtn = document.getElementById('discountBtn');
    const discountModal = document.getElementById('discountModal');
    const showDiscountModalFunc = typeof showDiscountModal === 'function';
    const bootstrapAvailable = typeof bootstrap !== 'undefined';
    
    console.log('[Test] Botón de descuento:', discountBtn);
    console.log('[Test] Modal de descuento:', discountModal);
    console.log('[Test] Función showDiscountModal disponible:', showDiscountModalFunc);
    console.log('[Test] Bootstrap disponible:', bootstrapAvailable);
    
    // Verificar currentOrder
    console.log('[Test] currentOrder:', currentOrder);
    
    // Intentar abrir el modal manualmente
    if (discountModal && bootstrapAvailable) {
        console.log('[Test] Intentando abrir modal manualmente...');
        try {
            const modal = new bootstrap.Modal(discountModal);
            modal.show();
            console.log('[Test] Modal abierto manualmente');
        } catch (error) {
            console.error('[Test] Error al abrir modal:', error);
        }
    } else {
        console.error('[Test] No se puede abrir modal - elementos faltantes');
    }
    
    return {
        buttonExists: !!discountBtn,
        modalExists: !!discountModal,
        functionExists: showDiscountModalFunc,
        bootstrapExists: bootstrapAvailable,
        currentOrderExists: !!currentOrder
    };
}

// Función para simular click en el botón
function simulateDiscountClick() {
    console.log('[Test] Simulando click en botón de descuento...');
    const discountBtn = document.getElementById('discountBtn');
    if (discountBtn) {
        discountBtn.click();
        console.log('[Test] Click simulado');
    } else {
        console.error('[Test] Botón no encontrado');
    }
}

// Función para verificar todos los elementos
function checkAllElements() {
    console.log('[Test] === VERIFICACIÓN COMPLETA DE ELEMENTOS ===');
    
    const elements = {
        discountBtn: document.getElementById('discountBtn'),
        discountModal: document.getElementById('discountModal'),
        discountType: document.getElementById('discountType'),
        discountValue: document.getElementById('discountValue'),
        discountReason: document.getElementById('discountReason'),
        discountSubtotal: document.getElementById('discountSubtotal'),
        discountTax: document.getElementById('discountTax'),
        discountTotal: document.getElementById('discountTotal'),
        discountCurrent: document.getElementById('discountCurrent')
    };
    
    Object.keys(elements).forEach(key => {
        console.log(`[Test] ${key}:`, elements[key] ? '✅ Encontrado' : '❌ No encontrado');
    });
    
    return elements;
}

// Hacer funciones disponibles globalmente
window.testDiscountModal = testDiscountModal;
window.simulateDiscountClick = simulateDiscountClick;
window.checkAllElements = checkAllElements;

console.log('[Test] Funciones de prueba disponibles:');
console.log('[Test] - testDiscountModal()');
console.log('[Test] - simulateDiscountClick()');
console.log('[Test] - checkAllElements()'); 