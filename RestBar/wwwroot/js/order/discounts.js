// ✅ Variables globales para descuentos
let currentDiscount = {
    type: null,
    value: 0,
    amount: 0,
    reason: '',
    applied: false
};

// ✅ Función para mostrar el modal de descuentos
function showDiscountModal() {
    if (!currentOrder || !currentOrder.items || currentOrder.items.length === 0) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No hay items en la orden para aplicar descuento'
        });
        return;
    }

    const discountModalElement = document.getElementById('discountModal');
    
    if (!discountModalElement) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudo abrir el modal de descuento'
        });
        return;
    }

    // Cargar información actual
    loadDiscountInfo();
    
    // Resetear formulario
    resetDiscountForm();
    
    // Mostrar el modal
    if (typeof bootstrap === 'undefined') {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Bootstrap no está disponible'
        });
        return;
    }
    
    try {
        const discountModal = new bootstrap.Modal(discountModalElement);
        discountModal.show();
    } catch (error) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudo abrir el modal: ' + error.message
        });
    }
}

// ✅ Función para cargar información de descuento
function loadDiscountInfo() {
    const taxBreakdown = calculateOrderTaxBreakdown();
    
    // Actualizar resumen en el modal
    document.getElementById('discountSubtotal').textContent = `$${taxBreakdown.subtotal.toFixed(2)}`;
    document.getElementById('discountTax').textContent = `$${taxBreakdown.totalTax.toFixed(2)}`;
    document.getElementById('discountTotal').textContent = `$${taxBreakdown.totalWithTax.toFixed(2)}`;
    document.getElementById('discountCurrent').textContent = `$${currentDiscount.amount.toFixed(2)}`;
    
    // Mostrar/ocultar botón de quitar descuento
    const removeBtn = document.getElementById('removeDiscountBtn');
    if (currentDiscount.applied && currentDiscount.amount > 0) {
        removeBtn.style.display = 'inline-block';
    } else {
        removeBtn.style.display = 'none';
    }
}

// ✅ Función para alternar tipo de descuento
function toggleDiscountType() {
    const discountType = document.getElementById('discountType').value;
    const discountValue = document.getElementById('discountValue');
    const discountUnit = document.getElementById('discountUnit');
    const discountHelp = document.getElementById('discountHelp');
    
    if (discountType === 'percentage') {
        discountValue.max = '100';
        discountValue.step = '0.01';
        discountUnit.textContent = '%';
        discountHelp.textContent = 'Ingresa el porcentaje de descuento (0-100%)';
    } else {
        discountValue.max = '';
        discountValue.step = '0.01';
        discountUnit.textContent = '$';
        discountHelp.textContent = 'Ingresa el monto fijo de descuento';
    }
    
    // Limpiar valor y actualizar vista previa
    discountValue.value = '';
    updateDiscountPreview();
}

// ✅ Función para actualizar vista previa del descuento
function updateDiscountPreview() {
    const discountType = document.getElementById('discountType').value;
    const discountValue = parseFloat(document.getElementById('discountValue').value) || 0;
    const taxBreakdown = calculateOrderTaxBreakdown();
    const previewDiv = document.getElementById('discountPreview');
    
    let discountAmount = 0;
    
    if (discountType === 'percentage') {
        if (discountValue > 100) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'El porcentaje no puede ser mayor al 100%'
            });
            document.getElementById('discountValue').value = '100';
            return;
        }
        discountAmount = (taxBreakdown.totalWithTax * discountValue) / 100;
    } else {
        if (discountValue > taxBreakdown.totalWithTax) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'El descuento no puede ser mayor al total'
            });
            document.getElementById('discountValue').value = taxBreakdown.totalWithTax.toFixed(2);
            return;
        }
        discountAmount = discountValue;
    }
    
    const finalTotal = taxBreakdown.totalWithTax - discountAmount;
    
    // Actualizar vista previa
    document.getElementById('previewDiscount').textContent = `$${discountAmount.toFixed(2)}`;
    document.getElementById('previewFinal').textContent = `$${finalTotal.toFixed(2)}`;
    
    // Mostrar vista previa si hay valor
    if (discountValue > 0) {
        previewDiv.style.display = 'block';
    } else {
        previewDiv.style.display = 'none';
    }
}

// ✅ Función para aplicar descuento
function applyDiscount() {
    const discountType = document.getElementById('discountType').value;
    const discountValue = parseFloat(document.getElementById('discountValue').value) || 0;
    const discountReason = document.getElementById('discountReason').value;
    
    if (discountValue <= 0) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'El valor del descuento debe ser mayor a 0'
        });
        return;
    }
    
    const taxBreakdown = calculateOrderTaxBreakdown();
    let discountAmount = 0;
    
    if (discountType === 'percentage') {
        discountAmount = (taxBreakdown.totalWithTax * discountValue) / 100;
    } else {
        discountAmount = discountValue;
    }
    
    // Guardar descuento
    currentDiscount = {
        type: discountType,
        value: discountValue,
        amount: discountAmount,
        reason: discountReason,
        applied: true
    };
    
    // Actualizar UI
    updateOrderUI();
    
    // Cerrar modal
    const discountModal = bootstrap.Modal.getInstance(document.getElementById('discountModal'));
    discountModal.hide();
    
    // Mostrar confirmación
    Swal.fire({
        icon: 'success',
        title: 'Descuento Aplicado',
        text: `Se aplicó un descuento de $${discountAmount.toFixed(2)}`,
        timer: 2000,
        showConfirmButton: false
    });
}

// ✅ Función para quitar descuento
function removeDiscount() {
    Swal.fire({
        title: '¿Quitar descuento?',
        text: '¿Estás seguro de que quieres quitar el descuento aplicado?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, quitar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            // Resetear descuento
            currentDiscount = {
                type: null,
                value: 0,
                amount: 0,
                reason: '',
                applied: false
            };
            
            // Actualizar UI
            updateOrderUI();
            
            // Cerrar modal
            const discountModal = bootstrap.Modal.getInstance(document.getElementById('discountModal'));
            discountModal.hide();
            
            // Mostrar confirmación
            Swal.fire({
                icon: 'success',
                title: 'Descuento Quitado',
                text: 'El descuento ha sido removido',
                timer: 2000,
                showConfirmButton: false
            });
        }
    });
}

// ✅ Función para resetear formulario de descuento
function resetDiscountForm() {
    document.getElementById('discountType').value = 'percentage';
    document.getElementById('discountValue').value = '';
    document.getElementById('discountReason').value = '';
    document.getElementById('discountPreview').style.display = 'none';
    toggleDiscountType();
}

// ✅ Función para calcular total con descuento
function calculateTotalWithDiscount() {
    const taxBreakdown = calculateOrderTaxBreakdown();
    const totalWithTax = taxBreakdown.totalWithTax;
    const discountAmount = currentDiscount.amount || 0;
    
    const finalTotal = Math.max(0, totalWithTax - discountAmount);
    

    
    return finalTotal;
}

// ✅ Event listeners para el modal de descuento
document.addEventListener('DOMContentLoaded', function() {
    // Escuchar cambios en el valor del descuento
    const discountValue = document.getElementById('discountValue');
    if (discountValue) {
        discountValue.addEventListener('input', updateDiscountPreview);
    }
    
    // Escuchar cambios en el tipo de descuento
    const discountType = document.getElementById('discountType');
    if (discountType) {
        discountType.addEventListener('change', function() {
            toggleDiscountType();
            updateDiscountPreview();
        });
    }
    
    // ✅ NUEVO: Inicializar descuento
    initializeDiscount();
});

// ✅ NUEVA: Función para inicializar descuento
function initializeDiscount() {
    // Resetear descuento al cargar la página
    currentDiscount = {
        type: null,
        value: 0,
        amount: 0,
        reason: '',
        applied: false
    };
    

}

// ✅ NUEVA: Función de prueba para verificar disponibilidad
function testDiscountFunction() {
    // Verificar si el modal existe
    const modalElement = document.getElementById('discountModal');
    
    return {
        showDiscountModalAvailable: typeof showDiscountModal === 'function',
        currentDiscount: currentDiscount,
        bootstrapAvailable: typeof bootstrap !== 'undefined',
        modalElementExists: !!modalElement
    };
}

// ✅ NUEVA: Hacer la función de prueba disponible globalmente
window.testDiscountFunction = testDiscountFunction;
window.showDiscountModal = showDiscountModal; 