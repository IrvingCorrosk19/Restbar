// Variables globales para pagos
let paymentSummary = null;
let splitPayments = [];

// Función para mostrar el modal de pagos
function showPaymentModal() {
    if (!currentOrder || !currentOrder.orderId) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No hay una orden activa para procesar pagos'
        });
        return;
    }

    // Cargar información de pagos
    loadPaymentSummary();
    
    // Mostrar el modal
    const paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
    paymentModal.show();
}

// Función para cargar el resumen de pagos
async function loadPaymentSummary() {
    try {
        const response = await fetch(`/api/payment/order/${currentOrder.orderId}/summary`);
        if (!response.ok) {
            throw new Error('Error al cargar información de pagos');
        }

        paymentSummary = await response.json();
        updatePaymentModal();
    } catch (error) {
        console.error('Error cargando resumen de pagos:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudo cargar la información de pagos'
        });
    }
}

// Función para actualizar el modal de pagos
function updatePaymentModal() {
    if (!paymentSummary) return;

    // Actualizar totales
    document.getElementById('paymentOrderTotal').textContent = `$${paymentSummary.totalOrderAmount.toFixed(2)}`;
    document.getElementById('paymentTotalPaid').textContent = `$${paymentSummary.totalPaidAmount.toFixed(2)}`;
    document.getElementById('paymentRemaining').textContent = `$${paymentSummary.remainingAmount.toFixed(2)}`;

    // Actualizar monto máximo a pagar
    const paymentAmountInput = document.getElementById('paymentAmount');
    paymentAmountInput.max = paymentSummary.remainingAmount;
    paymentAmountInput.value = paymentSummary.remainingAmount.toFixed(2);

    // Cargar items de la orden
    loadOrderItemsForPayment();

    // Mostrar historial de pagos si existe
    if (paymentSummary.payments && paymentSummary.payments.length > 0) {
        showPaymentHistory();
    }
}

// Función para cargar items de la orden en el modal
function loadOrderItemsForPayment() {
    const container = document.getElementById('paymentOrderItems');
    container.innerHTML = '';

    if (!currentOrder || !currentOrder.items) return;

    currentOrder.items.forEach(item => {
        const totalPrice = (item.quantity * item.price).toFixed(2);
        const itemDiv = document.createElement('div');
        itemDiv.className = 'order-item-summary';
        itemDiv.innerHTML = `
            <div class="d-flex justify-content-between align-items-center">
                <span>${item.quantity}x ${item.productName}</span>
                <span class="badge bg-secondary">$${totalPrice}</span>
            </div>
            ${item.notes ? `<div class="text-muted small">${item.notes}</div>` : ''}
        `;
        container.appendChild(itemDiv);
    });
}

// Función para mostrar historial de pagos
function showPaymentHistory() {
    const historyHtml = paymentSummary.payments.map(payment => `
        <div class="payment-history-item mb-2 p-2 border rounded">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <span><strong>$${payment.amount.toFixed(2)}</strong> - ${payment.method}</span>
                    <small class="text-muted d-block">${new Date(payment.paidAt).toLocaleTimeString()}</small>
                </div>
                <div class="d-flex align-items-center">
                    ${payment.isVoided ? 
                        '<span class="badge bg-secondary me-2">Anulado</span>' : 
                        '<button class="btn btn-sm btn-outline-danger me-2" onclick="voidPayment(\'' + payment.id + '\', \'' + payment.amount + '\', \'' + payment.method + '\')">Anular</button>'
                    }
                </div>
            </div>
            ${payment.splitPayments.length > 0 ? `
                <div class="mt-1">
                    <small class="text-muted">Dividido entre:</small>
                    ${payment.splitPayments.map(split => `
                        <div class="ms-2">${split.personName}: $${split.amount.toFixed(2)}</div>
                    `).join('')}
                </div>
            ` : ''}
        </div>
    `).join('');

    // Agregar sección de historial al modal
    const modalBody = document.querySelector('#paymentModal .modal-body');
    const existingHistory = modalBody.querySelector('.payment-history');
    
    if (existingHistory) {
        existingHistory.remove();
    }

    const historySection = document.createElement('div');
    historySection.className = 'payment-history mt-3';
    historySection.innerHTML = `
        <div class="mb-2">
            <strong>Historial de Pagos:</strong>
        </div>
        ${historyHtml}
    `;
    
    modalBody.appendChild(historySection);
}

// Función para anular un pago específico
async function voidPayment(paymentId, amount, method) {
    try {
        console.log('[Frontend] voidPayment iniciado');
        console.log('[Frontend] paymentId:', paymentId);
        console.log('[Frontend] amount:', amount);
        console.log('[Frontend] method:', method);
        
        // Confirmación antes de anular
        const result = await Swal.fire({
            title: '¿Anular Pago?',
            text: `¿Estás seguro de que deseas anular el pago de $${amount} (${method})? Esta acción no se puede deshacer.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, Anular',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33'
        });

        if (!result.isConfirmed) {
            console.log('[Frontend] Usuario canceló la anulación del pago');
            return;
        }

        console.log('[Frontend] Usuario confirmó anulación, llamando al backend...');
        
        // Mostrar loading
        Swal.fire({
            title: 'Anulando pago...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        // Llamar al endpoint de anulación
        const response = await fetch(`/api/payment/${paymentId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            console.log('[Frontend] Pago anulado exitosamente');
            
            // Cerrar loading y mostrar éxito
            Swal.fire({
                title: 'Pago Anulado',
                text: `El pago de $${amount} ha sido anulado correctamente`,
                icon: 'success',
                timer: 2000,
                showConfirmButton: false
            });

            // Recargar información de pagos
            await loadPaymentSummary();
            
            // Actualizar información de pagos en la interfaz principal
            await updatePaymentInfo();
            
        } else {
            let errorMessage = 'Error al anular el pago';
            try {
                const errorData = await response.json();
                errorMessage = errorData.message || errorData.error || errorMessage;
            } catch (e) {
                errorMessage = `Error ${response.status}: ${response.statusText}`;
            }
            throw new Error(errorMessage);
        }
        
    } catch (error) {
        console.error('[Frontend] Error anulando pago:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.message || 'No se pudo anular el pago'
        });
    }
}

// Función para agregar pago dividido
function addSplitPayment() {
    const container = document.getElementById('splitPaymentsContainer');
    const splitIndex = splitPayments.length;
    
    const splitDiv = document.createElement('div');
    splitDiv.className = 'split-payment-item mb-2 p-2 border rounded';
    splitDiv.innerHTML = `
        <div class="row">
            <div class="col-md-5">
                <input type="text" class="form-control" placeholder="Nombre de la persona" 
                       oninput="updateSplitPayment(${splitIndex}, 'personName', this.value)">
            </div>
            <div class="col-md-5">
                <input type="number" class="form-control" placeholder="Monto" step="0.01" min="0.01"
                       oninput="updateSplitPayment(${splitIndex}, 'amount', parseFloat(this.value) || 0)">
            </div>
            <div class="col-md-2">
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeSplitPayment(${splitIndex})">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
    `;
    
    container.appendChild(splitDiv);
    
    // Agregar al array
    splitPayments.push({
        personName: '',
        amount: 0
    });
}

// Función para actualizar pago dividido
function updateSplitPayment(index, field, value) {
    if (splitPayments[index]) {
        splitPayments[index][field] = value;
        validateSplitPayments();
        updateRemainingBalance();
    }
}

// Función para remover pago dividido
function removeSplitPayment(index) {
    splitPayments.splice(index, 1);
    
    // Recrear la lista visual
    const container = document.getElementById('splitPaymentsContainer');
    container.innerHTML = '';
    
    splitPayments.forEach((split, idx) => {
        const splitDiv = document.createElement('div');
        splitDiv.className = 'split-payment-item mb-2 p-2 border rounded';
        splitDiv.innerHTML = `
            <div class="row">
                <div class="col-md-5">
                    <input type="text" class="form-control" placeholder="Nombre de la persona" 
                           value="${split.personName}" oninput="updateSplitPayment(${idx}, 'personName', this.value)">
                </div>
                <div class="col-md-5">
                    <input type="number" class="form-control" placeholder="Monto" step="0.01" min="0.01"
                           value="${split.amount}" oninput="updateSplitPayment(${idx}, 'amount', parseFloat(this.value) || 0)">
                </div>
                <div class="col-md-2">
                    <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeSplitPayment(${idx})">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `;
        container.appendChild(splitDiv);
    });
    
    validateSplitPayments();
}

// Función para validar pagos divididos
function validateSplitPayments() {
    const totalAmount = parseFloat(document.getElementById('paymentAmount').value) || 0;
    const splitTotal = splitPayments.reduce((sum, split) => sum + (parseFloat(split.amount) || 0), 0);
    
    const difference = Math.abs(totalAmount - splitTotal);
    const isValid = difference < 0.01; // Tolerancia de 1 centavo
    
    // Actualizar saldo pendiente
    updateRemainingBalance();
    
    // Mostrar/ocultar alerta de validación en una ubicación mejor
    let validationAlert = document.getElementById('splitValidationAlert');
    if (!validationAlert) {
        validationAlert = document.createElement('div');
        validationAlert.id = 'splitValidationAlert';
        validationAlert.className = 'alert mt-2';
        // Insertar después del contenedor de pagos divididos
        const container = document.getElementById('splitPaymentsContainer');
        container.parentNode.insertBefore(validationAlert, container.nextSibling);
    }
    
    if (splitPayments.length > 0) {
        if (isValid) {
            validationAlert.className = 'alert alert-success mt-2';
            validationAlert.innerHTML = `<i class="fas fa-check"></i> Montos válidos ($${splitTotal.toFixed(2)})`;
        } else {
            validationAlert.className = 'alert alert-warning mt-2';
            validationAlert.innerHTML = `<i class="fas fa-exclamation-triangle"></i> La suma debe ser $${totalAmount.toFixed(2)}. Actual: $${splitTotal.toFixed(2)}`;
        }
        
        // Mostrar resumen de pagos divididos
        showSplitPaymentSummary();
    } else {
        validationAlert.remove();
        hideSplitPaymentSummary();
    }
    
    return isValid;
}

// Función para actualizar el saldo pendiente
function updateRemainingBalance() {
    const totalAmount = parseFloat(document.getElementById('paymentAmount').value) || 0;
    const splitTotal = splitPayments.reduce((sum, split) => sum + (parseFloat(split.amount) || 0), 0);
    const remaining = totalAmount - splitTotal;
    
    const remainingDisplay = document.getElementById('remainingBalanceDisplay');
    if (remainingDisplay) {
        if (remaining > 0) {
            remainingDisplay.textContent = `$${remaining.toFixed(2)}`;
            remainingDisplay.className = 'text-danger fw-bold';
        } else if (remaining < 0) {
            remainingDisplay.textContent = `$${Math.abs(remaining).toFixed(2)} (exceso)`;
            remainingDisplay.className = 'text-warning fw-bold';
        } else {
            remainingDisplay.textContent = '$0.00';
            remainingDisplay.className = 'text-success fw-bold';
        }
    }
}

// Función para mostrar resumen de pagos divididos
function showSplitPaymentSummary() {
    const summaryHtml = splitPayments.map((split, index) => `
        <div class="d-flex justify-content-between align-items-center mb-1">
            <span>${split.personName || `Persona ${index + 1}`}</span>
            <span class="fw-bold">$${(parseFloat(split.amount) || 0).toFixed(2)}</span>
        </div>
    `).join('');
    
    const totalSplit = splitPayments.reduce((sum, split) => sum + (parseFloat(split.amount) || 0), 0);
    
    const summaryDiv = document.getElementById('splitPaymentSummary');
    if (summaryDiv) {
        summaryDiv.style.display = 'block';
        summaryDiv.innerHTML = `
            <div class="card">
                <div class="card-header">
                    <strong>Resumen de Pagos Divididos</strong>
                </div>
                <div class="card-body">
                    ${summaryHtml}
                    <hr>
                    <div class="d-flex justify-content-between align-items-center">
                        <strong>Total:</strong>
                        <strong>$${totalSplit.toFixed(2)}</strong>
                    </div>
                </div>
            </div>
        `;
    }
}

// Función para ocultar resumen de pagos divididos
function hideSplitPaymentSummary() {
    const summaryDiv = document.getElementById('splitPaymentSummary');
    if (summaryDiv) {
        summaryDiv.style.display = 'none';
    }
}

// Función para procesar el pago
async function processPayment() {
    try {
        // Validar formulario
        const amount = parseFloat(document.getElementById('paymentAmount').value);
        const method = document.getElementById('paymentMethod').value;
        
        if (!amount || amount <= 0) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Ingresa un monto válido'
            });
            return;
        }
        
        if (!method) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Selecciona un método de pago'
            });
            return;
        }
        
        // Validar pagos divididos si existen
        if (splitPayments.length > 0 && !validateSplitPayments()) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Los montos de los pagos divididos no coinciden'
            });
            return;
        }
        
        // Preparar datos del pago
        const paymentData = {
            orderId: currentOrder.orderId,
            amount: amount,
            method: method,
            splitPayments: splitPayments.length > 0 ? splitPayments : null
        };
        
        // Mostrar loading
        Swal.fire({
            title: 'Procesando pago...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
        
        // Enviar pago al servidor
        const response = await fetch('/api/payment/partial', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(paymentData)
        });
        
        if (!response.ok) {
            let errorMessage = 'Error al procesar el pago';
            try {
                const errorData = await response.json();
                errorMessage = errorData.message || errorData.error || errorMessage;
            } catch (e) {
                // Si no se puede parsear JSON, usar el status y statusText
                errorMessage = `Error ${response.status}: ${response.statusText}`;
            }
            throw new Error(errorMessage);
        }
        
        const paymentResult = await response.json();
        
        // Cerrar loading y mostrar éxito
        Swal.fire({
            icon: 'success',
            title: 'Pago Procesado',
            text: `Pago de $${amount.toFixed(2)} registrado correctamente`,
            confirmButtonText: 'Aceptar'
        });
        
        // Cerrar modal y limpiar
        const paymentModal = bootstrap.Modal.getInstance(document.getElementById('paymentModal'));
        paymentModal.hide();
        
        // Limpiar formulario
        clearPaymentForm();
        
        // Actualizar información de pagos en la interfaz
        updatePaymentInfo();
        
    } catch (error) {
        console.error('Error procesando pago:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.message || 'Error al procesar el pago'
        });
    }
}

// Función para limpiar formulario de pago
function clearPaymentForm() {
    document.getElementById('paymentAmount').value = '';
    document.getElementById('paymentMethod').value = '';
    document.getElementById('splitPaymentsContainer').innerHTML = '';
    splitPayments = [];
}

// Función para actualizar información de pagos en la interfaz principal
async function updatePaymentInfo() {
    if (!currentOrder || !currentOrder.orderId) return;
    
    try {
        const response = await fetch(`/api/payment/order/${currentOrder.orderId}/summary`);
        if (response.ok) {
            const summary = await response.json();
            
            document.getElementById('totalPaid').textContent = `$${summary.totalPaidAmount.toFixed(2)}`;
            document.getElementById('remainingAmount').textContent = `$${summary.remainingAmount.toFixed(2)}`;
            
            // Mostrar/ocultar botón de pago parcial
            const paymentBtn = document.getElementById('partialPaymentBtn');
            if (summary.remainingAmount > 0) {
                paymentBtn.style.display = 'inline-block';
            } else {
                paymentBtn.style.display = 'none';
            }
            
            // Mostrar/ocultar botón de historial de pagos
            const historyBtn = document.getElementById('paymentHistoryBtn');
            if (summary.payments && summary.payments.length > 0) {
                historyBtn.style.display = 'inline-block';
            } else {
                historyBtn.style.display = 'none';
            }
        }
    } catch (error) {
        console.error('Error actualizando información de pagos:', error);
    }
}

// Función para mostrar el modal de historial de pagos
async function showPaymentHistoryModal() {
    if (!currentOrder || !currentOrder.orderId) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No hay una orden activa para ver el historial'
        });
        return;
    }

    try {
        // Cargar información de pagos
        const response = await fetch(`/api/payment/order/${currentOrder.orderId}/summary`);
        if (!response.ok) {
            throw new Error('Error al cargar información de pagos');
        }

        const paymentSummary = await response.json();
        
        // Crear contenido del modal
        let historyHtml = '';
        if (paymentSummary.payments && paymentSummary.payments.length > 0) {
            historyHtml = paymentSummary.payments.map(payment => `
                <div class="payment-history-item mb-3 p-3 border rounded">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="mb-1">$${payment.amount.toFixed(2)} - ${payment.method}</h6>
                            <small class="text-muted">${new Date(payment.paidAt).toLocaleString()}</small>
                        </div>
                        <div class="d-flex align-items-center">
                            ${payment.isVoided ? 
                                '<span class="badge bg-secondary">Anulado</span>' : 
                                '<button class="btn btn-sm btn-outline-danger" onclick="voidPaymentFromHistory(\'' + payment.id + '\', \'' + payment.amount + '\', \'' + payment.method + '\')">Anular</button>'
                            }
                        </div>
                    </div>
                    ${payment.splitPayments.length > 0 ? `
                        <div class="mt-2">
                            <small class="text-muted">Dividido entre:</small>
                            ${payment.splitPayments.map(split => `
                                <div class="ms-2 small">${split.personName}: $${split.amount.toFixed(2)}</div>
                            `).join('')}
                        </div>
                    ` : ''}
                </div>
            `).join('');
        } else {
            historyHtml = '<div class="text-center text-muted">No hay pagos registrados</div>';
        }

        // Mostrar modal con SweetAlert2
        await Swal.fire({
            title: 'Historial de Pagos',
            html: `
                <div class="text-start">
                    <div class="mb-3">
                        <strong>Resumen:</strong>
                        <div class="row mt-2">
                            <div class="col-4">
                                <small class="text-muted">Total Orden:</small><br>
                                <strong>$${paymentSummary.totalOrderAmount.toFixed(2)}</strong>
                            </div>
                            <div class="col-4">
                                <small class="text-muted">Pagado:</small><br>
                                <strong>$${paymentSummary.totalPaidAmount.toFixed(2)}</strong>
                            </div>
                            <div class="col-4">
                                <small class="text-muted">Pendiente:</small><br>
                                <strong>$${paymentSummary.remainingAmount.toFixed(2)}</strong>
                            </div>
                        </div>
                    </div>
                    <div class="payment-history-list" style="max-height: 300px; overflow-y: auto;">
                        ${historyHtml}
                    </div>
                </div>
            `,
            width: '600px',
            showConfirmButton: true,
            confirmButtonText: 'Cerrar',
            showCloseButton: true
        });

    } catch (error) {
        console.error('Error cargando historial de pagos:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudo cargar el historial de pagos'
        });
    }
}

// Función para anular pago desde el historial
async function voidPaymentFromHistory(paymentId, amount, method) {
    try {
        console.log('[Frontend] voidPaymentFromHistory iniciado');
        console.log('[Frontend] paymentId:', paymentId);
        console.log('[Frontend] amount:', amount);
        console.log('[Frontend] method:', method);
        
        // Confirmación antes de anular
        const result = await Swal.fire({
            title: '¿Anular Pago?',
            text: `¿Estás seguro de que deseas anular el pago de $${amount} (${method})? Esta acción no se puede deshacer.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, Anular',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33'
        });

        if (!result.isConfirmed) {
            console.log('[Frontend] Usuario canceló la anulación del pago');
            return;
        }

        console.log('[Frontend] Usuario confirmó anulación, llamando al backend...');
        
        // Mostrar loading
        Swal.fire({
            title: 'Anulando pago...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        // Llamar al endpoint de anulación
        const response = await fetch(`/api/payment/${paymentId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            console.log('[Frontend] Pago anulado exitosamente');
            
            // Cerrar loading y mostrar éxito
            Swal.fire({
                title: 'Pago Anulado',
                text: `El pago de $${amount} ha sido anulado correctamente`,
                icon: 'success',
                timer: 2000,
                showConfirmButton: false
            });

            // Actualizar información de pagos en la interfaz principal
            await updatePaymentInfo();
            
        } else {
            let errorMessage = 'Error al anular el pago';
            try {
                const errorData = await response.json();
                errorMessage = errorData.message || errorData.error || errorMessage;
            } catch (e) {
                errorMessage = `Error ${response.status}: ${response.statusText}`;
            }
            throw new Error(errorMessage);
        }
        
    } catch (error) {
        console.error('[Frontend] Error anulando pago:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.message || 'No se pudo anular el pago'
        });
    }
}

// Event listeners para el modal de pagos
document.addEventListener('DOMContentLoaded', function() {
    // Validar pagos divididos cuando cambie el monto
    const paymentAmountInput = document.getElementById('paymentAmount');
    if (paymentAmountInput) {
        paymentAmountInput.addEventListener('input', function() {
            validateSplitPayments();
            updateSplitPaymentAmounts();
            updateRemainingBalance();
        });
    }
    
    // Limpiar formulario cuando se cierre el modal
    const paymentModal = document.getElementById('paymentModal');
    if (paymentModal) {
        paymentModal.addEventListener('hidden.bs.modal', clearPaymentForm);
    }
});

// Función para actualizar automáticamente los montos de pagos divididos
function updateSplitPaymentAmounts() {
    const totalAmount = parseFloat(document.getElementById('paymentAmount').value) || 0;
    
    if (splitPayments.length > 0) {
        // Si hay pagos divididos, distribuir el monto automáticamente
        const amountPerPerson = totalAmount / splitPayments.length;
        
        splitPayments.forEach((split, index) => {
            split.amount = amountPerPerson;
        });
        
        // Recrear la lista visual con los nuevos montos
        const container = document.getElementById('splitPaymentsContainer');
        container.innerHTML = '';
        
        splitPayments.forEach((split, idx) => {
            const splitDiv = document.createElement('div');
            splitDiv.className = 'split-payment-item mb-2 p-2 border rounded';
            splitDiv.innerHTML = `
                <div class="row">
                    <div class="col-md-5">
                        <input type="text" class="form-control" placeholder="Nombre de la persona" 
                               value="${split.personName}" oninput="updateSplitPayment(${idx}, 'personName', this.value)">
                    </div>
                    <div class="col-md-5">
                        <input type="number" class="form-control" placeholder="Monto" step="0.01" min="0.01"
                               value="${split.amount.toFixed(2)}" oninput="updateSplitPayment(${idx}, 'amount', parseFloat(this.value) || 0)">
                    </div>
                    <div class="col-md-2">
                        <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeSplitPayment(${idx})">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            `;
            container.appendChild(splitDiv);
        });
        
        validateSplitPayments();
    }
}

// Exportar funciones para uso global
window.showPaymentModal = showPaymentModal;
window.addSplitPayment = addSplitPayment;
window.updateSplitPayment = updateSplitPayment;
window.removeSplitPayment = removeSplitPayment;
window.processPayment = processPayment;
window.updatePaymentInfo = updatePaymentInfo;
window.updateSplitPaymentAmounts = updateSplitPaymentAmounts;
window.showSplitPaymentSummary = showSplitPaymentSummary;
window.hideSplitPaymentSummary = hideSplitPaymentSummary;
window.updateRemainingBalance = updateRemainingBalance;
window.voidPayment = voidPayment; 
window.showPaymentHistoryModal = showPaymentHistoryModal;
window.voidPaymentFromHistory = voidPaymentFromHistory; 