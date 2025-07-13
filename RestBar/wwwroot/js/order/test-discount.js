// Test Discount - Script para probar funcionalidad de descuentos

// Función para probar el modal de descuento
function testDiscountModal() {
    const discountBtn = document.getElementById('discountBtn');
    const discountModal = document.getElementById('discountModal');
    const showDiscountModalFunc = typeof showDiscountModal === 'function';
    const bootstrapAvailable = typeof bootstrap !== 'undefined';

    if (discountBtn && discountModal && showDiscountModalFunc && bootstrapAvailable) {
        const currentOrder = window.currentOrder || {};
        
        // Intentar abrir modal manualmente
        try {
            const modal = new bootstrap.Modal(discountModal);
            modal.show();
        } catch (error) {
            // Fallback: usar función nativa
            discountModal.style.display = 'block';
            discountModal.classList.add('show');
        }
    }
}

// Función para simular click en botón de descuento
function simulateDiscountClick() {
    const discountBtn = document.getElementById('discountBtn');
    if (discountBtn) {
        discountBtn.click();
    }
}

// Función para verificar todos los elementos relacionados con descuentos
function checkAllElements() {
    const elements = {
        'discountBtn': document.getElementById('discountBtn'),
        'discountModal': document.getElementById('discountModal'),
        'discountType': document.getElementById('discountType'),
        'discountValue': document.getElementById('discountValue'),
        'discountReason': document.getElementById('discountReason'),
        'applyDiscountBtn': document.getElementById('applyDiscountBtn'),
        'cancelDiscountBtn': document.getElementById('cancelDiscountBtn'),
        'discountSummary': document.getElementById('discountSummary'),
        'discountAmount': document.getElementById('discountAmount'),
        'totalWithDiscount': document.getElementById('totalWithDiscount')
    };

    const functions = {
        'showDiscountModal': typeof showDiscountModal === 'function',
        'calculateTotalWithDiscount': typeof calculateTotalWithDiscount === 'function',
        'initializeDiscount': typeof initializeDiscount === 'function'
    };

    // Verificar elementos
    Object.keys(elements).forEach(key => {
        const found = elements[key] ? true : false;
    });

    // Verificar funciones
    Object.keys(functions).forEach(key => {
        const available = functions[key];
    });
}

// Exponer funciones para uso en consola
window.testDiscount = {
    testDiscountModal: testDiscountModal,
    simulateDiscountClick: simulateDiscountClick,
    checkAllElements: checkAllElements
}; 