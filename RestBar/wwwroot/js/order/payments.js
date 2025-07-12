// Variables globales para pagos
let paymentSummary = null;
let splitPayments = [];
let isSharedPayment = false;

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
    
    // Resetear el tipo de pago a individual
    document.getElementById('paymentTypeIndividual').checked = true;
    isSharedPayment = false;
    togglePaymentType();
    
    // Mostrar el modal
    const paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
    paymentModal.show();
}

// Función para alternar entre tipos de pago
function togglePaymentType() {
    const individualRadio = document.getElementById('paymentTypeIndividual');
    const sharedRadio = document.getElementById('paymentTypeShared');
    
    isSharedPayment = sharedRadio.checked;
    
    // Elementos del DOM a mostrar/ocultar
    const payerNameSection = document.getElementById('payerNameSection');
    const splitPaymentsSection = document.getElementById('splitPaymentsSection');
    const individualPaymentInfo = document.getElementById('individualPaymentInfo');
    const sharedPaymentInfo = document.getElementById('sharedPaymentInfo');
    const remainingBalanceSection = document.getElementById('remainingBalanceSection');
    
    console.log(`[Frontend] Cambiando tipo de pago a: ${isSharedPayment ? 'Compartido' : 'Individual'}`);
    
    const paymentMethodSelect = document.getElementById('paymentMethod');
    const paymentMethodLockIcon = document.getElementById('paymentMethodLockIcon');
    
    if (isSharedPayment) {
        // Pago compartido
        payerNameSection.style.display = 'none';
        splitPaymentsSection.style.display = 'block';
        individualPaymentInfo.style.display = 'none';
        sharedPaymentInfo.style.display = 'block';
        remainingBalanceSection.style.display = 'block';
        
        // Bloquear y establecer método de pago como "Compartido"
        paymentMethodSelect.value = 'Compartido';
        paymentMethodSelect.disabled = true;
        paymentMethodSelect.style.backgroundColor = '#e9ecef';
        paymentMethodLockIcon.style.display = 'inline';
        
        // Limpiar campo de nombre del pagador
        document.getElementById('payerName').value = '';
        
        // Si no hay split payments, agregar uno automáticamente
        if (splitPayments.length === 0) {
            addSplitPayment();
        }
    } else {
        // Pago individual
        payerNameSection.style.display = 'block';
        splitPaymentsSection.style.display = 'none';
        individualPaymentInfo.style.display = 'block';
        sharedPaymentInfo.style.display = 'none';
        remainingBalanceSection.style.display = 'none';
        
        // Desbloquear método de pago y resetear valor
        paymentMethodSelect.disabled = false;
        paymentMethodSelect.value = '';
        paymentMethodSelect.style.backgroundColor = '';
        paymentMethodLockIcon.style.display = 'none';
        
        // Limpiar split payments
        splitPayments = [];
        document.getElementById('splitPaymentsContainer').innerHTML = '';
        hideSplitPaymentSummary();
        
        // Limpiar alertas de validación
        const validationAlert = document.getElementById('splitValidationAlert');
        if (validationAlert) {
            validationAlert.remove();
        }
    }
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
                    ${payment.isShared ? 
                        '<span class="badge bg-info me-1">Compartido</span>' : 
                        (payment.payerName ? `<span class="badge bg-success me-1">${payment.payerName}</span>` : '<span class="badge bg-secondary me-1">Individual</span>')
                    }
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
                        <div class="ms-2">${split.personName}: $${split.amount.toFixed(2)} (${split.method})</div>
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
    const placeholder = `Nombre de la persona (opcional: se usará "Persona ${splitIndex + 1}")`;
    
    const splitDiv = document.createElement('div');
    splitDiv.className = 'split-payment-item mb-2 p-2 border rounded';
    splitDiv.innerHTML = `
        <div class="row">
            <div class="col-md-4">
                <input type="text" class="form-control" placeholder="${placeholder}" 
                       oninput="updateSplitPayment(${splitIndex}, 'personName', this.value)">
            </div>
            <div class="col-md-3">
                <input type="number" class="form-control" placeholder="Monto" step="0.01" min="0.01"
                       oninput="updateSplitPayment(${splitIndex}, 'amount', parseFloat(this.value) || 0)">
            </div>
            <div class="col-md-3">
                <select class="form-control" onchange="updateSplitPayment(${splitIndex}, 'method', this.value)">
                    <option value="">Método de pago</option>
                    <option value="Efectivo">Efectivo</option>
                    <option value="Tarjeta">Tarjeta</option>
                    <option value="Transferencia">Transferencia</option>
                    <option value="Otro">Otro</option>
                </select>
            </div>
            <div class="col-md-2">
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeSplitPayment(${splitIndex})">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
    `;
    
    container.appendChild(splitDiv);
    
    // Agregar al array con método de pago
    splitPayments.push({
        personName: '',
        amount: 0,
        method: ''
    });
    
    console.log(`[Frontend] Agregada persona ${splitIndex + 1} con placeholder: "${placeholder}"`);
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
        const displayName = split.personName && split.personName.trim().length > 0 
            ? split.personName 
            : '';
        const placeholder = `Nombre de la persona (opcional: se usará "Persona ${idx + 1}")`;
        
        const splitDiv = document.createElement('div');
        splitDiv.className = 'split-payment-item mb-2 p-2 border rounded';
        splitDiv.innerHTML = `
            <div class="row">
                <div class="col-md-4">
                    <input type="text" class="form-control" placeholder="${placeholder}" 
                           value="${displayName}" oninput="updateSplitPayment(${idx}, 'personName', this.value)">
                </div>
                <div class="col-md-3">
                    <input type="number" class="form-control" placeholder="Monto" step="0.01" min="0.01"
                           value="${split.amount}" oninput="updateSplitPayment(${idx}, 'amount', parseFloat(this.value) || 0)">
                </div>
                <div class="col-md-3">
                    <select class="form-control" onchange="updateSplitPayment(${idx}, 'method', this.value)">
                        <option value="">Método de pago</option>
                        <option value="Efectivo" ${split.method === 'Efectivo' ? 'selected' : ''}>Efectivo</option>
                        <option value="Tarjeta" ${split.method === 'Tarjeta' ? 'selected' : ''}>Tarjeta</option>
                        <option value="Transferencia" ${split.method === 'Transferencia' ? 'selected' : ''}>Transferencia</option>
                        <option value="Otro" ${split.method === 'Otro' ? 'selected' : ''}>Otro</option>
                    </select>
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
    
    // Usar el mismo redondeo que en el procesamiento
    const roundedTotalAmount = Math.round(totalAmount * 100) / 100;
    const roundedSplitTotal = Math.round(splitTotal * 100) / 100;
    
    const difference = Math.abs(roundedTotalAmount - roundedSplitTotal);
    const isValid = difference < 0.01; // Tolerancia de 1 centavo
    
    console.log('[Frontend] === VALIDACIÓN DE SPLIT PAYMENTS ===');
    console.log('[Frontend] Total amount:', roundedTotalAmount);
    console.log('[Frontend] Split total:', roundedSplitTotal);
    console.log('[Frontend] Diferencia:', difference);
    console.log('[Frontend] Es válido:', isValid);
    
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
            validationAlert.innerHTML = `<i class="fas fa-check"></i> Montos válidos ($${roundedSplitTotal.toFixed(2)})`;
        } else {
            validationAlert.className = 'alert alert-warning mt-2';
            validationAlert.innerHTML = `<i class="fas fa-exclamation-triangle"></i> La suma debe ser $${roundedTotalAmount.toFixed(2)}. Actual: $${roundedSplitTotal.toFixed(2)} (Diferencia: $${difference.toFixed(2)})`;
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
    
    // Usar el mismo redondeo que en el procesamiento
    const roundedTotalAmount = Math.round(totalAmount * 100) / 100;
    const roundedSplitTotal = Math.round(splitTotal * 100) / 100;
    const remaining = Math.round((roundedTotalAmount - roundedSplitTotal) * 100) / 100;
    
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
    const summaryHtml = splitPayments.map((split, index) => {
        const amount = parseFloat(split.amount) || 0;
        const roundedAmount = Math.round(amount * 100) / 100;
        const displayName = split.personName && split.personName.trim().length > 0 
            ? split.personName.trim() 
            : `Persona ${index + 1}`;
        const displayMethod = split.method || 'Sin especificar';
        return `
            <div class="d-flex justify-content-between align-items-center mb-1">
                <div>
                    <span class="fw-bold">${displayName}</span>
                    <small class="text-muted d-block">${displayMethod}</small>
                </div>
                <span class="fw-bold">$${roundedAmount.toFixed(2)}</span>
            </div>
        `;
    }).join('');
    
    const totalSplit = splitPayments.reduce((sum, split) => sum + (parseFloat(split.amount) || 0), 0);
    const roundedTotalSplit = Math.round(totalSplit * 100) / 100;
    
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
                        <strong>$${roundedTotalSplit.toFixed(2)}</strong>
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
        const payerName = document.getElementById('payerName').value.trim();
        
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
        
        // Validar que si es compartido, el método sea "Compartido"
        if (isSharedPayment && method !== 'Compartido') {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Para pagos compartidos, el método debe ser "Compartido"'
            });
            return;
        }
        
        console.log(`[Frontend] Procesando pago ${isSharedPayment ? 'COMPARTIDO' : 'INDIVIDUAL'}`);
        console.log(`[Frontend] Monto: $${amount}, Método: ${method}`);
        if (!isSharedPayment && payerName) {
            console.log(`[Frontend] Pagador: ${payerName}`);
        }
        
        // Validar pagos divididos si es pago compartido
        if (isSharedPayment) {
            // Validar que hay al menos un split payment
            if (splitPayments.length === 0) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Para pagos compartidos, debe agregar al menos una persona'
                });
                return;
            }
            console.log('[Frontend] Validando split payments...');
            
            // Verificar que todos los split payments tengan datos válidos
            const invalidSplits = [];
            splitPayments.forEach((split, index) => {
                console.log(`[Frontend] Validando split ${index}:`, split);
                
                if (!split.amount || split.amount <= 0) {
                    invalidSplits.push(`Split ${index + 1}: Monto inválido (${split.amount})`);
                }
                
                if (!split.method || split.method.trim().length === 0) {
                    const personName = split.personName && split.personName.trim().length > 0 
                        ? split.personName.trim() 
                        : `Persona ${index + 1}`;
                    invalidSplits.push(`${personName}: Debe seleccionar un método de pago`);
                }
            });
            
            if (invalidSplits.length > 0) {
                console.log('[Frontend] ERROR: Split payments inválidos:', invalidSplits);
                Swal.fire({
                    icon: 'error',
                    title: 'Error en Pagos Divididos',
                    html: `Corrige los siguientes problemas:<br><br>${invalidSplits.join('<br>')}<br><br><small>Nota: Los nombres se asignarán automáticamente si están vacíos (Persona 1, Persona 2, etc.)</small>`
                });
                return;
            }
            
            // Validar que los montos coincidan
            if (!validateSplitPayments()) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Los montos de los pagos divididos no coinciden'
                });
                return;
            }
            
            console.log('[Frontend] ✅ Split payments válidos');
        }
        
        // DEBUGGING: Verificar datos antes de preparar el pago
        console.log('[Frontend] === DATOS ANTES DE PREPARAR PAGO ===');
        console.log('[Frontend] currentOrder:', currentOrder);
        console.log('[Frontend] amount:', amount, 'tipo:', typeof amount);
        console.log('[Frontend] method:', method, 'tipo:', typeof method);
        console.log('[Frontend] splitPayments array:', splitPayments);
        console.log('[Frontend] splitPayments length:', splitPayments.length);
        
        // Verificar cada split payment individualmente
        if (splitPayments.length > 0) {
            splitPayments.forEach((split, index) => {
                console.log(`[Frontend] Split ${index}:`, split);
                console.log(`[Frontend] Split ${index} personName:`, split.personName, 'tipo:', typeof split.personName);
                console.log(`[Frontend] Split ${index} amount:`, split.amount, 'tipo:', typeof split.amount);
                console.log(`[Frontend] Split ${index} personName válido:`, split.personName && split.personName.trim().length > 0);
                console.log(`[Frontend] Split ${index} amount válido:`, split.amount && split.amount > 0);
            });
        }
        
        // Filtrar y limpiar split payments válidos
        const validSplitPayments = splitPayments.filter(split => {
            const hasValidAmount = split.amount && split.amount > 0;
            const hasValidMethod = split.method && split.method.trim().length > 0;
            console.log(`[Frontend] Split payment validación - amount: ${split.amount}, method: ${split.method}, válido: ${hasValidAmount && hasValidMethod}`);
            return hasValidAmount && hasValidMethod;
        }).map((split, index) => {
            // Asignar nombre por defecto si está vacío
            const personName = split.personName && split.personName.trim().length > 0 
                ? split.personName.trim() 
                : `Persona ${index + 1}`;
            
            const amount = Math.round(parseFloat(split.amount) * 100) / 100;
            const method = split.method.trim();
            
            console.log(`[Frontend] Split payment ${index + 1}: nombre="${personName}", monto=${amount}, método="${method}"`);
            
            return {
                personName: personName,
                amount: amount,
                method: method
            };
        });
        
        console.log('[Frontend] Split payments válidos filtrados:', validSplitPayments);
        
        // Validar que la suma coincida exactamente
        if (validSplitPayments.length > 0) {
            const splitTotal = validSplitPayments.reduce((sum, sp) => sum + sp.amount, 0);
            const roundedSplitTotal = Math.round(splitTotal * 100) / 100;
            const roundedAmount = Math.round(amount * 100) / 100;
            
            console.log('[Frontend] === VALIDACIÓN DE SUMA ===');
            console.log('[Frontend] Monto total del pago:', roundedAmount);
            console.log('[Frontend] Suma de split payments:', roundedSplitTotal);
            console.log('[Frontend] Diferencia:', Math.abs(roundedSplitTotal - roundedAmount));
            
            if (Math.abs(roundedSplitTotal - roundedAmount) > 0.01) {
                console.log('[Frontend] ERROR: La suma no coincide');
                Swal.fire({
                    icon: 'error',
                    title: 'Error en Pagos Divididos',
                    html: `La suma de los pagos divididos ($${roundedSplitTotal.toFixed(2)}) no coincide con el monto total ($${roundedAmount.toFixed(2)}).<br><br>Diferencia: $${Math.abs(roundedSplitTotal - roundedAmount).toFixed(2)}`
                });
                return;
            }
            
            console.log('[Frontend] ✅ Suma de split payments válida');
        }
        
        // Preparar datos del pago con montos redondeados
        const roundedAmount = Math.round(amount * 100) / 100;
        const paymentData = {
            orderId: currentOrder.orderId,
            amount: roundedAmount,
            method: method,
            isShared: isSharedPayment,
            payerName: !isSharedPayment && payerName ? payerName : null,
            splitPayments: isSharedPayment && validSplitPayments.length > 0 ? validSplitPayments : null
        };
        
        // LOGGING DETALLADO DE DATOS A ENVIAR
        console.log('[Frontend] === DATOS DE PAGO A ENVIAR ===');
        console.log('[Frontend] PaymentData:', paymentData);
        console.log('[Frontend] OrderId:', paymentData.orderId);
        console.log('[Frontend] Amount original:', amount);
        console.log('[Frontend] Amount redondeado:', paymentData.amount);
        console.log('[Frontend] Method:', paymentData.method);
        console.log('[Frontend] IsShared:', paymentData.isShared);
        console.log('[Frontend] PayerName:', paymentData.payerName);
        console.log('[Frontend] Split Payments Count:', paymentData.splitPayments?.length || 0);
        
        if (paymentData.splitPayments && paymentData.splitPayments.length > 0) {
            console.log('[Frontend] === DETALLE DE SPLIT PAYMENTS ===');
            paymentData.splitPayments.forEach((split, index) => {
                console.log(`[Frontend] Split ${index + 1}:`, {
                    personName: split.personName,
                    amount: split.amount,
                    type: typeof split.amount
                });
            });
            
            const totalSplits = paymentData.splitPayments.reduce((sum, split) => sum + split.amount, 0);
            console.log('[Frontend] Total suma de splits:', totalSplits);
            console.log('[Frontend] Diferencia con monto total:', Math.abs(totalSplits - paymentData.amount));
        }
        console.log('[Frontend] === FIN DATOS DE PAGO ===');
        
        // Mostrar loading
        Swal.fire({
            title: 'Procesando pago...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
        
        // Enviar pago al servidor
        console.log('[Frontend] Enviando pago al servidor...');
        const response = await fetch('/api/payment/partial', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(paymentData)
        });
        
        console.log('[Frontend] Respuesta del servidor recibida:', response.status, response.statusText);
        
        if (!response.ok) {
            console.log('[Frontend] ❌ Error en la respuesta del servidor');
            let errorMessage = 'Error al procesar el pago';
            try {
                const errorData = await response.json();
                console.log('[Frontend] Datos del error:', errorData);
                errorMessage = errorData.message || errorData.error || errorMessage;
            } catch (e) {
                console.log('[Frontend] No se pudo parsear JSON del error:', e);
                // Si no se puede parsear JSON, usar el status y statusText
                errorMessage = `Error ${response.status}: ${response.statusText}`;
            }
            console.log('[Frontend] Error final:', errorMessage);
            throw new Error(errorMessage);
        }
        
        console.log('[Frontend] ✅ Respuesta exitosa del servidor');
        const paymentResult = await response.json();
        console.log('[Frontend] Resultado del pago:', paymentResult);
        
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
        
        console.log('[Frontend] ✅ Pago procesado exitosamente');
        
    } catch (error) {
        console.error('[Frontend] ❌ ERROR CRÍTICO procesando pago:', error);
        console.error('[Frontend] Error type:', error.constructor.name);
        console.error('[Frontend] Error message:', error.message);
        console.error('[Frontend] Error stack:', error.stack);
        
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
    document.getElementById('payerName').value = '';
    document.getElementById('paymentTypeIndividual').checked = true;
    isSharedPayment = false;
    splitPayments = [];
    document.getElementById('splitPaymentsContainer').innerHTML = '';
    
    // Resetear la visualización (esto configurará el método de pago correctamente)
    togglePaymentType();
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
                            <div class="mt-1">
                                ${payment.isShared ? 
                                    '<span class="badge bg-info me-1">Compartido</span>' : 
                                    (payment.payerName ? `<span class="badge bg-success me-1">${payment.payerName}</span>` : '<span class="badge bg-secondary me-1">Individual</span>')
                                }
                            </div>
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
                                <div class="ms-2 small">${split.personName}: $${split.amount.toFixed(2)} (${split.method})</div>
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
        console.log('[Frontend] Distribuyendo automáticamente los montos...');
        console.log('[Frontend] Total amount:', totalAmount);
        console.log('[Frontend] Número de personas:', splitPayments.length);
        
        // Distribuir el monto automáticamente con redondeo adecuado
        const amountPerPerson = totalAmount / splitPayments.length;
        const roundedAmountPerPerson = Math.round(amountPerPerson * 100) / 100;
        
        console.log('[Frontend] Monto por persona (sin redondear):', amountPerPerson);
        console.log('[Frontend] Monto por persona (redondeado):', roundedAmountPerPerson);
        
        // Asignar el monto redondeado a cada persona
        splitPayments.forEach((split, index) => {
            split.amount = roundedAmountPerPerson;
        });
        
        // Calcular diferencia por redondeo y ajustar en la primera persona
        const totalAssigned = roundedAmountPerPerson * splitPayments.length;
        const roundedTotalAssigned = Math.round(totalAssigned * 100) / 100;
        const roundedTotalAmount = Math.round(totalAmount * 100) / 100;
        const difference = Math.round((roundedTotalAmount - roundedTotalAssigned) * 100) / 100;
        
        console.log('[Frontend] Total asignado:', roundedTotalAssigned);
        console.log('[Frontend] Total esperado:', roundedTotalAmount);
        console.log('[Frontend] Diferencia por redondeo:', difference);
        
        // Ajustar la diferencia en la primera persona
        if (Math.abs(difference) > 0.001 && splitPayments.length > 0) {
            splitPayments[0].amount = Math.round((splitPayments[0].amount + difference) * 100) / 100;
            console.log('[Frontend] Ajustado monto de primera persona a:', splitPayments[0].amount);
        }
        
        // Recrear la lista visual con los nuevos montos
        const container = document.getElementById('splitPaymentsContainer');
        container.innerHTML = '';
        
        splitPayments.forEach((split, idx) => {
            // Mostrar placeholder con nombre por defecto
            const displayName = split.personName && split.personName.trim().length > 0 
                ? split.personName 
                : '';
            const placeholder = `Nombre de la persona (opcional: se usará "Persona ${idx + 1}")`;
            
            const splitDiv = document.createElement('div');
            splitDiv.className = 'split-payment-item mb-2 p-2 border rounded';
            splitDiv.innerHTML = `
                <div class="row">
                    <div class="col-md-4">
                        <input type="text" class="form-control" placeholder="${placeholder}" 
                               value="${displayName}" oninput="updateSplitPayment(${idx}, 'personName', this.value)">
                    </div>
                    <div class="col-md-3">
                        <input type="number" class="form-control" placeholder="Monto" step="0.01" min="0.01"
                               value="${(Math.round(split.amount * 100) / 100).toFixed(2)}" oninput="updateSplitPayment(${idx}, 'amount', parseFloat(this.value) || 0)">
                    </div>
                    <div class="col-md-3">
                        <select class="form-control" onchange="updateSplitPayment(${idx}, 'method', this.value)">
                            <option value="">Método de pago</option>
                            <option value="Efectivo" ${split.method === 'Efectivo' ? 'selected' : ''}>Efectivo</option>
                            <option value="Tarjeta" ${split.method === 'Tarjeta' ? 'selected' : ''}>Tarjeta</option>
                            <option value="Transferencia" ${split.method === 'Transferencia' ? 'selected' : ''}>Transferencia</option>
                            <option value="Otro" ${split.method === 'Otro' ? 'selected' : ''}>Otro</option>
                        </select>
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