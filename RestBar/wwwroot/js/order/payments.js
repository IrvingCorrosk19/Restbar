// Payments Management - Funciones para manejo de pagos

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE PAGO CON CUENTAS SEPARADAS
function showPaymentModal() {
    try {
        console.log('🔍 [Payments] showPaymentModal() - Iniciando modal de pago...');
        
        if (!currentOrder || !currentOrder.orderId) {
            console.warn('⚠️ [Payments] showPaymentModal() - No hay orden actual');
            Swal.fire('Error', 'No hay una orden activa para procesar el pago', 'error');
            return;
        }

        // Verificar si hay cuentas separadas
        checkForSeparateAccounts().then(hasSeparateAccounts => {
            if (hasSeparateAccounts) {
                showSeparateAccountsPaymentModal();
            } else {
                showSingleAccountPaymentModal();
            }
        });

        console.log('✅ [Payments] showPaymentModal() - Modal de pago iniciado');
    } catch (error) {
        console.error('❌ [Payments] showPaymentModal() - Error:', error);
        Swal.fire('Error', 'Error al mostrar modal de pago', 'error');
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: VERIFICAR SI HAY CUENTAS SEPARADAS
async function checkForSeparateAccounts() {
    try {
        console.log('🔍 [Payments] checkForSeparateAccounts() - Verificando cuentas separadas...');
        
        if (!currentOrder || !currentOrder.orderId) return false;
        
        const response = await fetch(`/Person/GetPersonsByOrder?orderId=${currentOrder.orderId}`);
        const result = await response.json();
        
        const hasSeparateAccounts = result.success && result.data && result.data.length > 0;
        console.log(`📊 [Payments] checkForSeparateAccounts() - Cuentas separadas: ${hasSeparateAccounts}`);
        
        return hasSeparateAccounts;
    } catch (error) {
        console.error('❌ [Payments] checkForSeparateAccounts() - Error:', error);
        return false;
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE PAGO PARA CUENTA ÚNICA
function showSingleAccountPaymentModal() {
    try {
        console.log('🔍 [Payments] showSingleAccountPaymentModal() - Mostrando modal de cuenta única...');
        
        const orderTotal = currentOrder.items ? 
            currentOrder.items.reduce((sum, item) => sum + (item.quantity * item.unitPrice) - item.discount, 0) : 0;

        Swal.fire({
            title: '💳 Pago de Cuenta',
            html: `
                <div class="payment-modal-container">
                    <div class="row mb-3">
                        <div class="col-12">
                            <h6>Total de la Orden: <strong>$${orderTotal.toFixed(2)}</strong></h6>
                        </div>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-6">
                            <label class="form-label">Monto</label>
                            <input type="number" id="paymentAmount" class="form-control" step="0.01" min="0.01" value="${orderTotal.toFixed(2)}">
                        </div>
                        <div class="col-6">
                            <label class="form-label">Método de Pago</label>
                            <select id="paymentMethod" class="form-select">
                                <option value="Efectivo">Efectivo</option>
                                <option value="Tarjeta">Tarjeta</option>
                                <option value="Transferencia">Transferencia</option>
                            </select>
                        </div>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-12">
                            <label class="form-label">Nombre del Pagador (Opcional)</label>
                            <input type="text" id="payerName" class="form-control" placeholder="Nombre de quien paga">
                        </div>
                    </div>
                </div>
            `,
            width: '500px',
            showCancelButton: true,
            confirmButtonText: 'Procesar Pago',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            preConfirm: () => {
                const amount = parseFloat(document.getElementById('paymentAmount').value);
                const method = document.getElementById('paymentMethod').value;
                const payerName = document.getElementById('payerName').value;
                
                if (!amount || amount <= 0) {
                    Swal.showValidationMessage('El monto debe ser mayor a 0');
                    return false;
                }
                
                if (amount > orderTotal) {
                    Swal.showValidationMessage('El monto no puede ser mayor al total de la orden');
                    return false;
                }
                
                return { amount, method, payerName };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                processPayment(result.value.amount, result.value.method, false, result.value.payerName);
            }
        });

        console.log('✅ [Payments] showSingleAccountPaymentModal() - Modal de cuenta única mostrado');
    } catch (error) {
        console.error('❌ [Payments] showSingleAccountPaymentModal() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE PAGO PARA CUENTAS SEPARADAS
async function showSeparateAccountsPaymentModal() {
    try {
        console.log('🔍 [Payments] showSeparateAccountsPaymentModal() - Mostrando modal de cuentas separadas...');
        
        // Obtener resumen de cuentas separadas
        const summary = await getSeparateAccountsSummary();
        if (!summary) {
            console.warn('⚠️ [Payments] showSeparateAccountsPaymentModal() - No se pudo obtener resumen');
            showSingleAccountPaymentModal();
            return;
        }

        // Construir HTML del modal
        let html = `
            <div class="separate-accounts-payment-container">
                <div class="row mb-3">
                    <div class="col-12">
                        <h6>💰 Resumen de Cuentas Separadas</h6>
                    </div>
                </div>
                
                <div class="accounts-summary" style="max-height: 300px; overflow-y: auto; border: 1px solid #e9ecef; border-radius: 8px; padding: 15px; background: #f8f9fa;">
        `;

        // Mostrar cuentas por persona
        summary.persons.forEach(person => {
            html += `
                <div class="person-account mb-3 p-3 border rounded bg-white">
                    <div class="row align-items-center">
                        <div class="col-8">
                            <h6 class="mb-1">👤 ${person.name}</h6>
                            <small class="text-muted">${person.items.length} items</small>
                        </div>
                        <div class="col-4 text-end">
                            <strong class="text-primary">$${person.total.toFixed(2)}</strong>
                        </div>
                    </div>
                    <div class="mt-2">
                        <button type="button" class="btn btn-sm btn-outline-primary" onclick="showPersonPaymentModal('${person.id}', '${person.name}', ${person.total})">
                            💳 Pagar Cuenta
                        </button>
                    </div>
                </div>
            `;
        });

        // Mostrar items compartidos si existen
        if (summary.sharedItems.length > 0) {
            html += `
                <div class="shared-account mb-3 p-3 border rounded bg-warning bg-opacity-10">
                    <div class="row align-items-center">
                        <div class="col-8">
                            <h6 class="mb-1">🤝 Items Compartidos</h6>
                            <small class="text-muted">${summary.sharedItems.length} items</small>
                        </div>
                        <div class="col-4 text-end">
                            <strong class="text-warning">$${summary.sharedTotal.toFixed(2)}</strong>
                        </div>
                    </div>
                    <div class="mt-2">
                        <button type="button" class="btn btn-sm btn-outline-warning" onclick="showSharedItemsPaymentModal(${summary.sharedTotal})">
                            💳 Pagar Compartido
                        </button>
                    </div>
                </div>
            `;
        }

        html += `
                </div>
                
                <div class="row mt-3">
                    <div class="col-12">
                        <div class="alert alert-info">
                            <strong>Total General: $${summary.totalOrder.toFixed(2)}</strong>
                        </div>
                    </div>
                </div>
                
                <div class="row">
                    <div class="col-12">
                        <button type="button" class="btn btn-success w-100" onclick="showFullPaymentModal(${summary.totalOrder})">
                            💳 Pagar Cuenta Completa
                        </button>
                    </div>
                </div>
            </div>
        `;

        Swal.fire({
            title: '💳 Pago de Cuentas Separadas',
            html: html,
            width: '600px',
            showCancelButton: true,
            confirmButtonText: 'Cerrar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d'
        });

        console.log('✅ [Payments] showSeparateAccountsPaymentModal() - Modal de cuentas separadas mostrado');
    } catch (error) {
        console.error('❌ [Payments] showSeparateAccountsPaymentModal() - Error:', error);
        showSingleAccountPaymentModal();
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE PAGO PARA PERSONA ESPECÍFICA
function showPersonPaymentModal(personId, personName, personTotal) {
    try {
        console.log('🔍 [Payments] showPersonPaymentModal() - Mostrando pago para persona...');
        
        Swal.fire({
            title: `💳 Pago de ${personName}`,
            html: `
                <div class="person-payment-container">
                    <div class="row mb-3">
                        <div class="col-12">
                            <h6>Total de ${personName}: <strong>$${personTotal.toFixed(2)}</strong></h6>
                        </div>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-6">
                            <label class="form-label">Monto</label>
                            <input type="number" id="personPaymentAmount" class="form-control" step="0.01" min="0.01" value="${personTotal.toFixed(2)}">
                        </div>
                        <div class="col-6">
                            <label class="form-label">Método de Pago</label>
                            <select id="personPaymentMethod" class="form-select">
                                <option value="Efectivo">Efectivo</option>
                                <option value="Tarjeta">Tarjeta</option>
                                <option value="Transferencia">Transferencia</option>
                            </select>
                        </div>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-12">
                            <label class="form-label">Nombre del Pagador</label>
                            <input type="text" id="personPayerName" class="form-control" value="${personName}" readonly>
                        </div>
                    </div>
                </div>
            `,
            width: '500px',
            showCancelButton: true,
            confirmButtonText: 'Procesar Pago',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            preConfirm: () => {
                const amount = parseFloat(document.getElementById('personPaymentAmount').value);
                const method = document.getElementById('personPaymentMethod').value;
                const payerName = document.getElementById('personPayerName').value;
                
                if (!amount || amount <= 0) {
                    Swal.showValidationMessage('El monto debe ser mayor a 0');
                    return false;
                }
                
                if (amount > personTotal) {
                    Swal.showValidationMessage('El monto no puede ser mayor al total de la persona');
                    return false;
                }
                
                return { amount, method, payerName };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                processPayment(result.value.amount, result.value.method, false, result.value.payerName);
            }
        });

        console.log('✅ [Payments] showPersonPaymentModal() - Modal de pago de persona mostrado');
    } catch (error) {
        console.error('❌ [Payments] showPersonPaymentModal() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE PAGO PARA ITEMS COMPARTIDOS
function showSharedItemsPaymentModal(sharedTotal) {
    try {
        console.log('🔍 [Payments] showSharedItemsPaymentModal() - Mostrando pago para items compartidos...');
        
        Swal.fire({
            title: '💳 Pago de Items Compartidos',
            html: `
                <div class="shared-payment-container">
                    <div class="row mb-3">
                        <div class="col-12">
                            <h6>Total de Items Compartidos: <strong>$${sharedTotal.toFixed(2)}</strong></h6>
                        </div>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-6">
                            <label class="form-label">Monto</label>
                            <input type="number" id="sharedPaymentAmount" class="form-control" step="0.01" min="0.01" value="${sharedTotal.toFixed(2)}">
                        </div>
                        <div class="col-6">
                            <label class="form-label">Método de Pago</label>
                            <select id="sharedPaymentMethod" class="form-select">
                                <option value="Efectivo">Efectivo</option>
                                <option value="Tarjeta">Tarjeta</option>
                                <option value="Transferencia">Transferencia</option>
                            </select>
                        </div>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-12">
                            <label class="form-label">Nombre del Pagador</label>
                            <input type="text" id="sharedPayerName" class="form-control" placeholder="Quién paga los items compartidos">
                        </div>
                    </div>
                </div>
            `,
            width: '500px',
            showCancelButton: true,
            confirmButtonText: 'Procesar Pago',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            preConfirm: () => {
                const amount = parseFloat(document.getElementById('sharedPaymentAmount').value);
                const method = document.getElementById('sharedPaymentMethod').value;
                const payerName = document.getElementById('sharedPayerName').value;
                
                if (!amount || amount <= 0) {
                    Swal.showValidationMessage('El monto debe ser mayor a 0');
                    return false;
                }
                
                if (amount > sharedTotal) {
                    Swal.showValidationMessage('El monto no puede ser mayor al total de items compartidos');
                    return false;
                }
                
                if (!payerName.trim()) {
                    Swal.showValidationMessage('Debe especificar quién paga');
                    return false;
                }
                
                return { amount, method, payerName };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                processPayment(result.value.amount, result.value.method, true, result.value.payerName);
            }
        });

        console.log('✅ [Payments] showSharedItemsPaymentModal() - Modal de pago de items compartidos mostrado');
    } catch (error) {
        console.error('❌ [Payments] showSharedItemsPaymentModal() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE PAGO COMPLETO
function showFullPaymentModal(totalAmount) {
    try {
        console.log('🔍 [Payments] showFullPaymentModal() - Mostrando pago completo...');
        
        Swal.fire({
            title: '💳 Pago de Cuenta Completa',
            html: `
                <div class="full-payment-container">
                    <div class="row mb-3">
                        <div class="col-12">
                            <h6>Total Completo: <strong>$${totalAmount.toFixed(2)}</strong></h6>
                        </div>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-6">
                            <label class="form-label">Monto</label>
                            <input type="number" id="fullPaymentAmount" class="form-control" step="0.01" min="0.01" value="${totalAmount.toFixed(2)}">
                        </div>
                        <div class="col-6">
                            <label class="form-label">Método de Pago</label>
                            <select id="fullPaymentMethod" class="form-select">
                                <option value="Efectivo">Efectivo</option>
                                <option value="Tarjeta">Tarjeta</option>
                                <option value="Transferencia">Transferencia</option>
                            </select>
                        </div>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-12">
                            <label class="form-label">Nombre del Pagador</label>
                            <input type="text" id="fullPayerName" class="form-control" placeholder="Quién paga la cuenta completa">
                        </div>
                    </div>
                </div>
            `,
            width: '500px',
            showCancelButton: true,
            confirmButtonText: 'Procesar Pago',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            preConfirm: () => {
                const amount = parseFloat(document.getElementById('fullPaymentAmount').value);
                const method = document.getElementById('fullPaymentMethod').value;
                const payerName = document.getElementById('fullPayerName').value;
                
                if (!amount || amount <= 0) {
                    Swal.showValidationMessage('El monto debe ser mayor a 0');
                    return false;
                }
                
                if (amount > totalAmount) {
                    Swal.showValidationMessage('El monto no puede ser mayor al total');
                    return false;
                }
                
                if (!payerName.trim()) {
                    Swal.showValidationMessage('Debe especificar quién paga');
                    return false;
                }
                
                return { amount, method, payerName };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                processPayment(result.value.amount, result.value.method, false, result.value.payerName);
            }
        });

        console.log('✅ [Payments] showFullPaymentModal() - Modal de pago completo mostrado');
    } catch (error) {
        console.error('❌ [Payments] showFullPaymentModal() - Error:', error);
    }
}

// Función para procesar pago
async function processPayment(amount, method, isShared = false, payerName = '', splitPayments = []) {
    if (!currentOrder || !currentOrder.orderId) {
        Swal.fire('Error', 'No hay una orden activa para procesar el pago', 'error');
        return;
    }

    try {
        // Validar split payments si existen
        if (splitPayments && splitPayments.length > 0) {
            const validSplitPayments = splitPayments.filter(split => split.amount > 0);
            
            if (validSplitPayments.length === 0) {
                Swal.fire('Error', 'No hay pagos divididos válidos', 'error');
                return;
            }

            // Validar que la suma coincida con el monto total
            const roundedAmount = Math.round(amount * 100) / 100;
            const splitTotal = validSplitPayments.reduce((sum, split) => sum + split.amount, 0);
            const roundedSplitTotal = Math.round(splitTotal * 100) / 100;
            
            if (Math.abs(roundedSplitTotal - roundedAmount) > 0.01) {
                Swal.fire('Error', 'La suma de los pagos divididos no coincide con el monto total', 'error');
                return;
            }
        }

        // Preparar datos del pago
        const paymentData = {
            orderId: currentOrder.orderId,
            amount: Math.round(amount * 100) / 100,
            method: method,
            isShared: isShared,
            payerName: payerName,
            splitPayments: splitPayments || []
        };

        // Enviar pago al servidor
        const response = await fetch('/api/Payment/partial', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(paymentData)
        });

        if (response.ok) {
            const result = await response.json();
            
            if (result.success) {
                Swal.fire({
                    title: 'Pago Procesado',
                    text: result.message || 'El pago ha sido procesado exitosamente',
                    icon: 'success',
                    timer: 3000,
                    showConfirmButton: false
                });

                // Actualizar información de pagos
                if (typeof updatePaymentInfo === 'function') {
                    await updatePaymentInfo();
                }

                // Si es pago completo, limpiar orden
                if (result.isFullyPaid) {
                    currentOrder = { items: [], total: 0, tableId: null };
                    updateOrderUI();
                    clearOrderUI();
                }
            } else {
                Swal.fire('Error', result.message || 'Error al procesar el pago', 'error');
            }
        } else {
            const errorData = await response.json();
            Swal.fire('Error', errorData.message || 'Error del servidor al procesar el pago', 'error');
        }
    } catch (error) {
        Swal.fire('Error', 'Error de conexión al procesar el pago', 'error');
    }
}

// Función para validar pagos divididos
function validateSplitPayments(splitPayments, totalAmount) {
    if (!splitPayments || splitPayments.length === 0) {
        return { isValid: false, message: 'No hay pagos divididos' };
    }

    const validPayments = splitPayments.filter(split => split.amount > 0);
    
    if (validPayments.length === 0) {
        return { isValid: false, message: 'No hay pagos divididos válidos' };
    }

    const total = validPayments.reduce((sum, split) => sum + split.amount, 0);
    const roundedTotal = Math.round(total * 100) / 100;
    const roundedAmount = Math.round(totalAmount * 100) / 100;

    if (Math.abs(roundedTotal - roundedAmount) > 0.01) {
        return { 
            isValid: false, 
            message: `La suma de los pagos ($${roundedTotal.toFixed(2)}) no coincide con el total ($${roundedAmount.toFixed(2)})` 
        };
    }

    return { isValid: true, message: 'Pagos divididos válidos' };
}

// Función para agregar pago dividido
function addSplitPayment() {
    const splitPaymentsContainer = document.getElementById('splitPaymentsContainer');
    const splitIndex = splitPaymentsContainer.children.length;

    const splitPaymentDiv = document.createElement('div');
    splitPaymentDiv.className = 'split-payment-item mb-2';
    splitPaymentDiv.innerHTML = `
        <div class="row">
            <div class="col-md-4">
                <input type="text" class="form-control" placeholder="Nombre del pagador" 
                       id="payerName_${splitIndex}">
            </div>
            <div class="col-md-4">
                <input type="number" class="form-control" placeholder="Monto" step="0.01" min="0"
                       id="splitAmount_${splitIndex}">
            </div>
            <div class="col-md-4">
                <button type="button" class="btn btn-outline-danger btn-sm" 
                        onclick="removeSplitPayment(this)">
                    <i class="fas fa-trash"></i> Eliminar
                </button>
            </div>
        </div>
    `;

    splitPaymentsContainer.appendChild(splitPaymentDiv);
}

// Función para eliminar pago dividido
function removeSplitPayment(button) {
    button.closest('.split-payment-item').remove();
    updateSplitPaymentsTotal();
}

// Función para actualizar total de pagos divididos
function updateSplitPaymentsTotal() {
    const splitAmounts = document.querySelectorAll('[id^="splitAmount_"]');
    let total = 0;

    splitAmounts.forEach(input => {
        const amount = parseFloat(input.value) || 0;
        total += amount;
    });

    const splitTotalElement = document.getElementById('splitPaymentsTotal');
    if (splitTotalElement) {
        splitTotalElement.textContent = `$${total.toFixed(2)}`;
    }

    return total;
}

// Función para calcular desglose de impuestos
function calculateTaxBreakdown() {
    if (!currentOrder || !currentOrder.items) {
        return { subtotal: 0, tax: 0, total: 0 };
    }

    let subtotal = 0;
    let totalTax = 0;

    currentOrder.items.forEach(item => {
        const itemSubtotal = item.price * item.quantity;
        const taxRate = item.taxRate || 0;
        const itemTax = itemSubtotal * (taxRate / 100);
        
        subtotal += itemSubtotal;
        totalTax += itemTax;
    });

    return {
        subtotal: subtotal,
        tax: totalTax,
        total: subtotal + totalTax
    };
}

// Función para alternar tipo de pago
function togglePaymentType() {
    const paymentType = document.getElementById('paymentType');
    const splitPaymentsSection = document.getElementById('splitPaymentsSection');
    
    if (paymentType.value === 'shared') {
        splitPaymentsSection.style.display = 'block';
    } else {
        splitPaymentsSection.style.display = 'none';
    }
}

// Función para anular pago
async function voidPayment(paymentId) {
    const result = await Swal.fire({
        title: '¿Anular pago?',
        text: '¿Estás seguro de que deseas anular este pago?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, anular',
        cancelButtonText: 'Cancelar'
    });

    if (result.isConfirmed) {
        try {
            const response = await fetch(`/Order/VoidPayment/${paymentId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const result = await response.json();
                
                if (result.success) {
                    Swal.fire({
                        title: 'Pago Anulado',
                        text: 'El pago ha sido anulado exitosamente',
                        icon: 'success',
                        timer: 2000,
                        showConfirmButton: false
                    });

                    // Actualizar información de pagos
                    if (typeof updatePaymentInfo === 'function') {
                        await updatePaymentInfo();
                    }
                } else {
                    Swal.fire('Error', result.message || 'Error al anular el pago', 'error');
                }
            } else {
                Swal.fire('Error', 'Error del servidor al anular el pago', 'error');
            }
        } catch (error) {
            Swal.fire('Error', 'Error de conexión al anular el pago', 'error');
        }
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE PAGO PARCIAL
function showPaymentModal() {
    try {
        console.log('🚀 [Payments] showPaymentModal() - MODAL DE PAGO MOSTRADO');
        
        if (!currentOrder || !currentOrder.orderId) {
            Swal.fire('Error', 'No hay una orden activa para procesar el pago', 'error');
            return;
        }

        // Calcular total de la orden
        const taxBreakdown = calculateTaxBreakdown();
        const totalAmount = taxBreakdown.total;

        // Crear modal de pago con SweetAlert2
        Swal.fire({
            title: '💳 Pago Parcial',
            html: `
                <div class="payment-modal">
                    <div class="mb-3">
                        <label class="form-label">Total de la Orden:</label>
                        <input type="text" class="form-control" value="$${totalAmount.toFixed(2)}" readonly>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Monto a Pagar:</label>
                        <input type="number" id="paymentAmount" class="form-control" step="0.01" min="0.01" max="${totalAmount}" value="${totalAmount}" required>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Método de Pago:</label>
                        <select id="paymentMethod" class="form-select" required>
                            <option value="">Seleccionar método</option>
                            <option value="Cash">💵 Efectivo</option>
                            <option value="CreditCard">💳 Tarjeta de Crédito</option>
                            <option value="DebitCard">💳 Tarjeta de Débito</option>
                            <option value="MobilePayment">📱 Pago Móvil</option>
                        </select>
                    </div>
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: '💳 Procesar Pago',
            cancelButtonText: '❌ Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#dc3545',
            preConfirm: () => {
                const amount = parseFloat(document.getElementById('paymentAmount').value);
                const method = document.getElementById('paymentMethod').value;

                if (!amount || amount <= 0) {
                    Swal.showValidationMessage('Ingresa un monto válido');
                    return false;
                }

                if (!method) {
                    Swal.showValidationMessage('Selecciona un método de pago');
                    return false;
                }

                if (amount > totalAmount) {
                    Swal.showValidationMessage('El monto no puede ser mayor al total de la orden');
                    return false;
                }

                return { amount, method };
            }
        }).then(async (result) => {
            if (result.isConfirmed) {
                const { amount, method } = result.value;
                
                // 🎯 LOG ESTRATÉGICO: PROCESANDO PAGO PARCIAL
                console.log('🚀 [Payments] showPaymentModal() - PROCESANDO PAGO PARCIAL - Monto:', amount, 'Método:', method);
                
                await processPayment(amount, method);
            }
        });

    } catch (error) {
        console.error('❌ [Payments] showPaymentModal() - Error:', error);
        Swal.fire('Error', 'Error al mostrar el modal de pago', 'error');
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE HISTORIAL DE PAGOS
function showPaymentHistoryModal() {
    try {
        console.log('🚀 [Payments] showPaymentHistoryModal() - MODAL DE HISTORIAL MOSTRADO');
        
        if (!currentOrder || !currentOrder.orderId) {
            Swal.fire('Error', 'No hay una orden activa para ver el historial', 'error');
            return;
        }

        // Obtener historial de pagos de la orden
        fetch(`/Order/GetPaymentHistory/${currentOrder.orderId}`)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    displayPaymentHistory(data.payments);
                } else {
                    Swal.fire('Error', data.message || 'Error al obtener el historial de pagos', 'error');
                }
            })
            .catch(error => {
                console.error('❌ [Payments] showPaymentHistoryModal() - Error:', error);
                Swal.fire('Error', 'Error de conexión al obtener el historial', 'error');
            });

    } catch (error) {
        console.error('❌ [Payments] showPaymentHistoryModal() - Error:', error);
        Swal.fire('Error', 'Error al mostrar el modal de historial', 'error');
    }
}

// 🎯 FUNCIÓN AUXILIAR: MOSTRAR HISTORIAL DE PAGOS
function displayPaymentHistory(payments) {
    if (!payments || payments.length === 0) {
        Swal.fire({
            title: '📋 Historial de Pagos',
            text: 'No hay pagos registrados para esta orden',
            icon: 'info'
        });
        return;
    }

    // Crear tabla de pagos
    let paymentsTable = `
        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>Fecha</th>
                        <th>Monto</th>
                        <th>Método</th>
                        <th>Estado</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
    `;

    payments.forEach(payment => {
        const formattedDate = new Date(payment.createdAt).toLocaleString();
        const statusBadge = payment.isVoided ? 
            '<span class="badge bg-danger">Anulado</span>' : 
            '<span class="badge bg-success">Activo</span>';
        
        const voidButton = !payment.isVoided ? 
            `<button class="btn btn-sm btn-outline-danger" onclick="voidPayment('${payment.id}')">
                <i class="fas fa-times"></i> Anular
            </button>` : 
            '<span class="text-muted">-</span>';

        paymentsTable += `
            <tr>
                <td>${formattedDate}</td>
                <td>$${payment.amount.toFixed(2)}</td>
                <td>${payment.method}</td>
                <td>${statusBadge}</td>
                <td>${voidButton}</td>
            </tr>
        `;
    });

    paymentsTable += `
                </tbody>
            </table>
        </div>
    `;

    // Calcular totales
    const totalPaid = payments.filter(p => !p.isVoided).reduce((sum, p) => sum + p.amount, 0);
    const taxBreakdown = calculateTaxBreakdown();
    const remaining = taxBreakdown.total - totalPaid;

    paymentsTable += `
        <div class="mt-3 p-3 bg-light rounded">
            <div class="row">
                <div class="col-md-4">
                    <strong>Total Orden: $${taxBreakdown.total.toFixed(2)}</strong>
                </div>
                <div class="col-md-4">
                    <strong class="text-success">Pagado: $${totalPaid.toFixed(2)}</strong>
                </div>
                <div class="col-md-4">
                    <strong class="text-warning">Restante: $${remaining.toFixed(2)}</strong>
                </div>
            </div>
        </div>
    `;

    Swal.fire({
        title: '📋 Historial de Pagos',
        html: paymentsTable,
        width: '800px',
        showConfirmButton: true,
        confirmButtonText: '✅ Cerrar',
        confirmButtonColor: '#28a745'
    });
}

// Exportar funciones para uso global
window.processPayment = processPayment;
window.validateSplitPayments = validateSplitPayments;
window.addSplitPayment = addSplitPayment;
window.removeSplitPayment = removeSplitPayment;
window.updateSplitPaymentsTotal = updateSplitPaymentsTotal;
window.calculateTaxBreakdown = calculateTaxBreakdown;
window.togglePaymentType = togglePaymentType;
window.voidPayment = voidPayment;
window.showPaymentModal = showPaymentModal;
window.showPaymentHistoryModal = showPaymentHistoryModal; 