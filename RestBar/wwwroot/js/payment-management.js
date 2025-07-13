/**
 * Payment Management System
 * RestBar - Gestión de Pagos
 */

class PaymentManager {
    constructor() {
        this.currentFilters = {
            dateFilter: 'month',
            statusFilter: 'all',
            methodFilter: 'all'
        };
        this.charts = {};
        this.init();
    }

    init() {
        
        this.bindEvents();
        this.loadDashboardStats();
        this.loadPayments();
        this.loadPendingPayments();
        this.initCharts();
    }

    bindEvents() {
        // Filter events
        $('#dateFilter, #statusFilter, #methodFilter').on('change', (e) => {
            this.currentFilters[e.target.id] = e.target.value;
            this.loadPayments();
            this.loadPendingPayments();
            this.updateCharts();
        });

        // Refresh button
        $('#refreshBtn').on('click', () => {
            this.refreshAll();
        });

        // Export button
        $('#exportPayments').on('click', () => {
            this.exportPayments();
        });

        // Tab events
        $('#paymentTabs button[data-bs-toggle="tab"]').on('shown.bs.tab', (e) => {
            const target = $(e.target).attr('data-bs-target');
            if (target === '#analytics') {
                this.updateCharts();
            }
        });
    }

            async loadDashboardStats() {
            try {
                const response = await fetch('/PaymentView/DashboardStats', {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });

            if (response.ok) {
                const data = await response.json();
                this.updateDashboardStats(data);
            } else {
                
                this.updateDashboardStats({
                    totalRevenue: 0,
                    totalOrders: 0,
                    pendingPayments: 0
                });
            }
        } catch (error) {
            
            this.updateDashboardStats({
                totalRevenue: 0,
                totalOrders: 0,
                pendingPayments: 0
            });
        }
    }

    updateDashboardStats(data) {
        $('#totalRevenue').text(`$${data.totalRevenue?.toFixed(2) || '0.00'}`);
        $('#totalOrders').text(data.totalOrders || 0);
        $('#pendingPayments').text(data.pendingPayments || 0);
    }

    async loadPayments() {
        const loadingElement = $('#paymentsLoading');
        const tableBody = $('#paymentsTableBody');

        loadingElement.show();
        tableBody.empty();

        try {
            const params = new URLSearchParams({
                dateFilter: this.currentFilters.dateFilter,
                statusFilter: this.currentFilters.statusFilter,
                methodFilter: this.currentFilters.methodFilter
            });

            const response = await fetch(`/PaymentView/RecentPayments?${params}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const responseData = await response.json();
                const payments = responseData.data || responseData;
                this.renderPaymentsTable(payments);
            } else {
                
                this.showErrorMessage('Error al cargar los pagos');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al cargar los pagos');
        } finally {
            loadingElement.hide();
        }
    }

    renderPaymentsTable(payments) {
        const tableBody = $('#paymentsTableBody');
        tableBody.empty();

        if (!payments || payments.length === 0) {
            tableBody.append(`
                <tr>
                    <td colspan="7" class="text-center text-muted">
                        <i class="fas fa-inbox fa-2x mb-2"></i>
                        <p>No hay pagos para mostrar</p>
                    </td>
                </tr>
            `);
            return;
        }

        payments.forEach(payment => {
            const row = `
                <tr>
                    <td>
                        <span class="badge bg-secondary">${payment.id.substring(0, 8)}...</span>
                    </td>
                    <td>
                        <strong>Orden #${payment.orderNumber}</strong>
                        <br>
                        <small class="text-muted">Mesa ${payment.tableNumber}</small>
                    </td>
                    <td>
                        <span class="fw-bold text-success">$${payment.amount.toFixed(2)}</span>
                    </td>
                    <td>
                        <span class="badge bg-info">${payment.method}</span>
                    </td>
                    <td>
                        <span class="status-badge status-${payment.status.toLowerCase()}">
                            ${this.getStatusText(payment.status)}
                        </span>
                    </td>
                    <td>
                        <small>${new Date(payment.createdAt).toLocaleDateString()}</small>
                        <br>
                        <small class="text-muted">${new Date(payment.createdAt).toLocaleTimeString()}</small>
                    </td>
                    <td>
                        <div class="btn-group" role="group">
                            <button class="action-btn btn-view" onclick="paymentManager.viewPayment('${payment.id}')" title="Ver detalles">
                                <i class="fas fa-eye"></i>
                            </button>
                            ${payment.status === 'PENDING' ? `
                                <button class="action-btn btn-edit" onclick="paymentManager.editPayment('${payment.id}')" title="Editar">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="action-btn btn-delete" onclick="paymentManager.voidPayment('${payment.id}')" title="Anular">
                                    <i class="fas fa-times"></i>
                                </button>
                            ` : ''}
                            <button class="action-btn btn-print" onclick="paymentManager.printReceipt('${payment.id}')" title="Imprimir recibo">
                                <i class="fas fa-print"></i>
                            </button>
                            <button class="action-btn btn-share" onclick="paymentManager.sharePayment('${payment.id}')" title="Compartir">
                                <i class="fas fa-share-alt"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
            tableBody.append(row);
        });
    }

    async loadPendingPayments() {
        const loadingElement = $('#pendingLoading');
        const tableBody = $('#pendingTableBody');

        loadingElement.show();
        tableBody.empty();

        try {
            const response = await fetch('/PaymentView/PendingPayments', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const responseData = await response.json();
                const orders = responseData.data || responseData;
                this.renderPendingTable(orders);
            } else {
                
                this.showErrorMessage('Error al cargar pagos pendientes');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al cargar pagos pendientes');
        } finally {
            loadingElement.hide();
        }
    }

    renderPendingTable(orders) {
        const tableBody = $('#pendingTableBody');
        tableBody.empty();

        if (!orders || orders.length === 0) {
            tableBody.append(`
                <tr>
                    <td colspan="7" class="text-center text-muted">
                        <i class="fas fa-check-circle fa-2x mb-2"></i>
                        <p>No hay pagos pendientes</p>
                    </td>
                </tr>
            `);
            return;
        }

        orders.forEach(order => {
            const row = `
                <tr>
                    <td>
                        <strong>Orden #${order.orderNumber}</strong>
                        <br>
                        <small class="text-muted">${order.itemsCount} items</small>
                    </td>
                    <td>
                        <span class="badge bg-primary">Mesa ${order.tableNumber}</span>
                    </td>
                    <td>
                        <span class="fw-bold">$${order.total.toFixed(2)}</span>
                    </td>
                    <td>
                        <span class="text-success">$${order.paidAmount.toFixed(2)}</span>
                    </td>
                    <td>
                        <span class="text-warning fw-bold">$${order.pendingAmount.toFixed(2)}</span>
                    </td>
                    <td>
                        <span class="status-badge status-pending">Pendiente</span>
                    </td>
                    <td>
                        <div class="btn-group" role="group">
                            <button class="action-btn btn-view" onclick="paymentManager.viewOrder('${order.id}')" title="Ver orden">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="action-btn btn-edit" onclick="paymentManager.processPayment('${order.id}')" title="Procesar pago">
                                <i class="fas fa-credit-card"></i>
                            </button>
                            <button class="action-btn btn-print" onclick="paymentManager.printOrder('${order.id}')" title="Imprimir orden">
                                <i class="fas fa-print"></i>
                            </button>
                            <button class="action-btn btn-share" onclick="paymentManager.shareOrder('${order.id}')" title="Compartir orden">
                                <i class="fas fa-share-alt"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
            tableBody.append(row);
        });
    }

    initCharts() {
        // Payment Methods Chart
        const methodsCtx = document.getElementById('paymentMethodsChart');
        if (methodsCtx) {
            this.charts.methods = new Chart(methodsCtx, {
                type: 'doughnut',
                data: {
                    labels: ['Efectivo', 'Tarjeta', 'Transferencia', 'Compartido'],
                    datasets: [{
                        data: [30, 40, 20, 10],
                        backgroundColor: [
                            '#27ae60',
                            '#3498db',
                            '#f39c12',
                            '#9b59b6'
                        ],
                        borderWidth: 2,
                        borderColor: '#fff'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        }

        // Daily Sales Chart
        const salesCtx = document.getElementById('dailySalesChart');
        if (salesCtx) {
            this.charts.sales = new Chart(salesCtx, {
                type: 'line',
                data: {
                    labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
                    datasets: [{
                        label: 'Ventas ($)',
                        data: [1200, 1900, 1500, 2100, 2800, 3200, 2500],
                        borderColor: '#3498db',
                        backgroundColor: 'rgba(52, 152, 219, 0.1)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function(value) {
                                    return '$' + value.toLocaleString();
                                }
                            }
                        }
                    }
                }
            });
        }

        // Monthly Performance Chart
        const performanceCtx = document.getElementById('monthlyPerformanceChart');
        if (performanceCtx) {
            this.charts.performance = new Chart(performanceCtx, {
                type: 'bar',
                data: {
                    labels: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'],
                    datasets: [{
                        label: 'Ingresos ($)',
                        data: [45000, 52000, 48000, 61000, 58000, 67000],
                        backgroundColor: '#3498db',
                        borderRadius: 4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function(value) {
                                    return '$' + (value / 1000) + 'k';
                                }
                            }
                        }
                    }
                }
            });
        }
    }

    async updateCharts() {
        try {
            const response = await fetch('/PaymentView/Analytics', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateChartsData(data);
            }
        } catch (error) {
            
        }
    }

    updateChartsData(data) {
        // Update payment methods chart
        if (this.charts.methods && data.paymentMethods) {
            this.charts.methods.data.datasets[0].data = data.paymentMethods.map(m => m.amount);
            this.charts.methods.data.labels = data.paymentMethods.map(m => m.method);
            this.charts.methods.update();
        }

        // Update daily sales chart
        if (this.charts.sales && data.dailySales) {
            this.charts.sales.data.datasets[0].data = data.dailySales.map(d => d.amount);
            this.charts.sales.data.labels = data.dailySales.map(d => d.date);
            this.charts.sales.update();
        }

        // Update monthly performance chart
        if (this.charts.performance && data.monthlyPerformance) {
            this.charts.performance.data.datasets[0].data = data.monthlyPerformance.map(m => m.amount);
            this.charts.performance.data.labels = data.monthlyPerformance.map(m => m.month);
            this.charts.performance.update();
        }
    }

    async viewPayment(paymentId) {
        try {
            const response = await fetch(`/PaymentView/PaymentDetails/${paymentId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const payment = await response.json();
                this.showPaymentDetails(payment);
            } else {
                this.showErrorMessage('Error al cargar los detalles del pago');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión');
        }
    }

    showPaymentDetails(payment) {
        const modal = $('#paymentDetailsModal');
        const content = $('#paymentDetailsContent');

        content.html(`
            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-info-circle"></i> Información del Pago</h6>
                    <table class="table table-sm">
                        <tr>
                            <td><strong>ID:</strong></td>
                            <td>${payment.id}</td>
                        </tr>
                        <tr>
                            <td><strong>Orden:</strong></td>
                            <td>#${payment.orderNumber}</td>
                        </tr>
                        <tr>
                            <td><strong>Monto:</strong></td>
                            <td class="text-success fw-bold">$${payment.amount.toFixed(2)}</td>
                        </tr>
                        <tr>
                            <td><strong>Método:</strong></td>
                            <td><span class="badge bg-info">${payment.method}</span></td>
                        </tr>
                        <tr>
                            <td><strong>Estado:</strong></td>
                            <td><span class="status-badge status-${payment.status.toLowerCase()}">${this.getStatusText(payment.status)}</span></td>
                        </tr>
                        <tr>
                            <td><strong>Fecha:</strong></td>
                            <td>${new Date(payment.createdAt).toLocaleString()}</td>
                        </tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h6><i class="fas fa-list"></i> Detalles de la Orden</h6>
                    <div class="order-items">
                        ${payment.orderItems ? payment.orderItems.map(item => `
                            <div class="order-item">
                                <span class="item-name">${item.productName}</span>
                                <span class="item-quantity">x${item.quantity}</span>
                                <span class="item-price">$${item.unitPrice.toFixed(2)}</span>
                                <span class="item-total">$${(item.quantity * item.unitPrice).toFixed(2)}</span>
                            </div>
                        `).join('') : '<p class="text-muted">No hay items disponibles</p>'}
                    </div>
                </div>
            </div>
        `);

        modal.modal('show');
    }

    async processPayment(orderId) {
        try {
            const response = await fetch(`/api/Payment/order/${orderId}/summary`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const orderSummary = await response.json();
                this.showPaymentModal(orderSummary);
            } else {
                this.showErrorMessage('Error al cargar el resumen de la orden');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión');
        }
    }

    showPaymentModal(orderSummary) {
        const modal = $('#paymentModal');
        const content = $('#paymentModalContent');

        content.html(`
            <div class="payment-form">
                <div class="row">
                    <div class="col-md-6">
                        <h6><i class="fas fa-receipt"></i> Resumen de la Orden</h6>
                        <div class="order-summary">
                            <p><strong>Orden #${orderSummary.orderNumber}</strong></p>
                            <p><strong>Mesa:</strong> ${orderSummary.tableNumber}</p>
                            <p><strong>Total:</strong> <span class="text-success fw-bold">$${orderSummary.total.toFixed(2)}</span></p>
                            <p><strong>Pagado:</strong> <span class="text-info">$${orderSummary.paidAmount.toFixed(2)}</span></p>
                            <p><strong>Pendiente:</strong> <span class="text-warning fw-bold">$${orderSummary.pendingAmount.toFixed(2)}</span></p>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <h6><i class="fas fa-credit-card"></i> Procesar Pago</h6>
                        <form id="paymentForm">
                            <div class="mb-3">
                                <label for="paymentAmount" class="form-label">Monto a Pagar</label>
                                <input type="number" class="form-control" id="paymentAmount" 
                                       value="${orderSummary.pendingAmount}" 
                                       max="${orderSummary.pendingAmount}" 
                                       step="0.01" required>
                            </div>
                            <div class="mb-3">
                                <label for="paymentMethod" class="form-label">Método de Pago</label>
                                <select class="form-control" id="paymentMethod" required>
                                    <option value="">Seleccionar método</option>
                                    <option value="Efectivo">Efectivo</option>
                                    <option value="Tarjeta">Tarjeta</option>
                                    <option value="Transferencia">Transferencia</option>
                                    <option value="Compartido">Compartido</option>
                                </select>
                            </div>
                            <div class="mb-3">
                                <label for="payerName" class="form-label">Nombre del Pagador</label>
                                <input type="text" class="form-control" id="payerName" placeholder="Opcional">
                            </div>
                            <div class="d-grid">
                                <button type="submit" class="btn btn-primary">
                                    <i class="fas fa-check"></i> Procesar Pago
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        `);

        // Bind form submission
        $('#paymentForm').off('submit').on('submit', (e) => {
            e.preventDefault();
            this.submitPayment(orderSummary.orderId);
        });

        modal.modal('show');
    }

    async submitPayment(orderId) {
        const amount = parseFloat($('#paymentAmount').val());
        const method = $('#paymentMethod').val();
        const payerName = $('#payerName').val();

        if (!amount || !method) {
            this.showErrorMessage('Por favor complete todos los campos requeridos');
            return;
        }

        try {
            const response = await fetch('/api/Payment/partial', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    orderId: orderId,
                    amount: amount,
                    method: method,
                    payerName: payerName,
                    isShared: method === 'Compartido'
                })
            });

            if (response.ok) {
                const result = await response.json();
                $('#paymentModal').modal('hide');
                this.showSuccessMessage('Pago procesado exitosamente');
                this.refreshAll();
            } else {
                const error = await response.json();
                this.showErrorMessage(error.message || 'Error al procesar el pago');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al procesar el pago');
        }
    }

    async voidPayment(paymentId) {
        if (!confirm('¿Está seguro de que desea anular este pago?')) {
            return;
        }

        try {
            const response = await fetch(`/api/Payment/${paymentId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.showSuccessMessage('Pago anulado exitosamente');
                this.refreshAll();
            } else {
                const error = await response.json();
                this.showErrorMessage(error.message || 'Error al anular el pago');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al anular el pago');
        }
    }

    async editPayment(paymentId) {
        try {
            const response = await fetch(`/PaymentView/PaymentDetails/${paymentId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.showEditPaymentModal(data.data);
                } else {
                    this.showErrorMessage('Error al cargar los detalles del pago');
                }
            } else {
                this.showErrorMessage('Error al cargar los detalles del pago');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al cargar detalles');
        }
    }

    showEditPaymentModal(payment) {
        const modal = $('#paymentModal');
        const content = $('#paymentModalContent');

        content.html(`
            <div class="payment-form">
                <div class="row">
                    <div class="col-md-6">
                        <h6><i class="fas fa-edit"></i> Editar Pago</h6>
                        <div class="payment-info">
                            <p><strong>Orden #${payment.orderNumber}</strong></p>
                            <p><strong>Mesa:</strong> ${payment.tableNumber}</p>
                            <p><strong>Monto Original:</strong> <span class="text-success fw-bold">$${payment.amount.toFixed(2)}</span></p>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <h6><i class="fas fa-credit-card"></i> Modificar Pago</h6>
                        <form id="editPaymentForm">
                            <div class="mb-3">
                                <label for="editPaymentAmount" class="form-label">Nuevo Monto</label>
                                <input type="number" class="form-control" id="editPaymentAmount" 
                                       value="${payment.amount}" step="0.01" required>
                            </div>
                            <div class="mb-3">
                                <label for="editPaymentMethod" class="form-label">Método de Pago</label>
                                <select class="form-control" id="editPaymentMethod" required>
                                    <option value="Efectivo" ${payment.method === 'Efectivo' ? 'selected' : ''}>Efectivo</option>
                                    <option value="Tarjeta" ${payment.method === 'Tarjeta' ? 'selected' : ''}>Tarjeta</option>
                                    <option value="Transferencia" ${payment.method === 'Transferencia' ? 'selected' : ''}>Transferencia</option>
                                    <option value="Compartido" ${payment.method === 'Compartido' ? 'selected' : ''}>Compartido</option>
                                </select>
                            </div>
                            <div class="mb-3">
                                <label for="editPayerName" class="form-label">Nombre del Pagador</label>
                                <input type="text" class="form-control" id="editPayerName" 
                                       value="${payment.payerName || ''}" placeholder="Opcional">
                            </div>
                            <div class="d-grid gap-2">
                                <button type="submit" class="btn btn-primary">
                                    <i class="fas fa-save"></i> Guardar Cambios
                                </button>
                                <button type="button" class="btn btn-secondary" onclick="$('#paymentModal').modal('hide')">
                                    <i class="fas fa-times"></i> Cancelar
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        `);

        // Bind form submission
        $('#editPaymentForm').off('submit').on('submit', (e) => {
            e.preventDefault();
            this.submitEditPayment(payment.id);
        });

        modal.modal('show');
    }

    async submitEditPayment(paymentId) {
        const amount = parseFloat($('#editPaymentAmount').val());
        const method = $('#editPaymentMethod').val();
        const payerName = $('#editPayerName').val();

        if (!amount || !method) {
            this.showErrorMessage('Por favor complete todos los campos requeridos');
            return;
        }

        try {
            const response = await fetch(`/PaymentView/UpdatePayment/${paymentId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    amount: amount,
                    method: method,
                    payerName: payerName
                })
            });

            if (response.ok) {
                $('#paymentModal').modal('hide');
                this.showSuccessMessage('Pago actualizado exitosamente');
                this.refreshAll();
            } else {
                const error = await response.json();
                this.showErrorMessage(error.message || 'Error al actualizar el pago');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al actualizar el pago');
        }
    }

    async printReceipt(paymentId) {
        try {
            const response = await fetch(`/PaymentView/PrintReceipt/${paymentId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const printWindow = window.open(url, '_blank');
                if (printWindow) {
                    printWindow.print();
                }
                this.showSuccessMessage('Recibo enviado a impresión');
            } else {
                this.showErrorMessage('Error al generar el recibo');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al imprimir');
        }
    }

    async sharePayment(paymentId) {
        try {
            const response = await fetch(`/PaymentView/PaymentDetails/${paymentId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.showShareModal(data.data);
                } else {
                    this.showErrorMessage('Error al cargar los detalles del pago');
                }
            } else {
                this.showErrorMessage('Error al cargar los detalles del pago');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al cargar detalles');
        }
    }

    showShareModal(payment) {
        const shareText = `Pago procesado - Orden #${payment.orderNumber} - Mesa ${payment.tableNumber} - Monto: $${payment.amount.toFixed(2)} - Método: ${payment.method}`;
        
        if (navigator.share) {
            navigator.share({
                title: 'Detalles del Pago',
                text: shareText,
                url: window.location.href
            }).then(() => {
                this.showSuccessMessage('Información compartida exitosamente');
            }).catch((error) => {
                
                this.showErrorMessage('Error al compartir');
            });
        } else {
            // Fallback: copy to clipboard
            navigator.clipboard.writeText(shareText).then(() => {
                this.showSuccessMessage('Información copiada al portapapeles');
            }).catch(() => {
                this.showErrorMessage('No se pudo copiar al portapapeles');
            });
        }
    }

    async viewOrder(orderId) {
        try {
            const response = await fetch(`/PaymentView/OrderDetails/${orderId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.showOrderDetailsModal(data.data);
                } else {
                    this.showErrorMessage('Error al cargar los detalles de la orden');
                }
            } else {
                this.showErrorMessage('Error al cargar los detalles de la orden');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al cargar detalles');
        }
    }

    showOrderDetailsModal(order) {
        const modal = $('#paymentDetailsModal');
        const content = $('#paymentDetailsContent');

        content.html(`
            <div class="order-details">
                <div class="row">
                    <div class="col-md-6">
                        <h6><i class="fas fa-receipt"></i> Detalles de la Orden</h6>
                        <div class="order-info">
                            <p><strong>Orden #${order.orderNumber}</strong></p>
                            <p><strong>Mesa:</strong> ${order.tableNumber}</p>
                            <p><strong>Estado:</strong> <span class="status-badge status-${order.status.toLowerCase()}">${this.getStatusText(order.status)}</span></p>
                            <p><strong>Fecha:</strong> ${new Date(order.createdAt).toLocaleString()}</p>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <h6><i class="fas fa-calculator"></i> Resumen Financiero</h6>
                        <div class="financial-summary">
                            <p><strong>Total Orden:</strong> <span class="text-primary fw-bold">$${order.total.toFixed(2)}</span></p>
                            <p><strong>Pagado:</strong> <span class="text-success">$${order.paidAmount.toFixed(2)}</span></p>
                            <p><strong>Pendiente:</strong> <span class="text-warning fw-bold">$${order.pendingAmount.toFixed(2)}</span></p>
                        </div>
                    </div>
                </div>
                <div class="row mt-3">
                    <div class="col-12">
                        <h6><i class="fas fa-list"></i> Items de la Orden</h6>
                        <div class="table-responsive">
                            <table class="table table-sm">
                                <thead>
                                    <tr>
                                        <th>Producto</th>
                                        <th>Cantidad</th>
                                        <th>Precio Unit.</th>
                                        <th>Total</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${order.orderItems ? order.orderItems.map(item => `
                                        <tr>
                                            <td>${item.productName}</td>
                                            <td>${item.quantity}</td>
                                            <td>$${item.unitPrice.toFixed(2)}</td>
                                            <td>$${item.total.toFixed(2)}</td>
                                        </tr>
                                    `).join('') : '<tr><td colspan="4" class="text-center">No hay items disponibles</td></tr>'}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        `);

        modal.modal('show');
    }

    async printOrder(orderId) {
        try {
            const response = await fetch(`/PaymentView/PrintOrder/${orderId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const printWindow = window.open(url, '_blank');
                if (printWindow) {
                    printWindow.print();
                }
                this.showSuccessMessage('Orden enviada a impresión');
            } else {
                this.showErrorMessage('Error al generar la orden para imprimir');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al imprimir');
        }
    }

    async shareOrder(orderId) {
        try {
            const response = await fetch(`/PaymentView/OrderDetails/${orderId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.showShareOrderModal(data.data);
                } else {
                    this.showErrorMessage('Error al cargar los detalles de la orden');
                }
            } else {
                this.showErrorMessage('Error al cargar los detalles de la orden');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al cargar detalles');
        }
    }

    showShareOrderModal(order) {
        const shareText = `Orden #${order.orderNumber} - Mesa ${order.tableNumber} - Total: $${order.total.toFixed(2)} - Pendiente: $${order.pendingAmount.toFixed(2)}`;
        
        if (navigator.share) {
            navigator.share({
                title: 'Detalles de la Orden',
                text: shareText,
                url: window.location.href
            }).then(() => {
                this.showSuccessMessage('Información de la orden compartida exitosamente');
            }).catch((error) => {
                
                this.showErrorMessage('Error al compartir');
            });
        } else {
            // Fallback: copy to clipboard
            navigator.clipboard.writeText(shareText).then(() => {
                this.showSuccessMessage('Información de la orden copiada al portapapeles');
            }).catch(() => {
                this.showErrorMessage('No se pudo copiar al portapapeles');
            });
        }
    }

    refreshAll() {
        this.loadDashboardStats();
        this.loadPayments();
        this.loadPendingPayments();
        this.updateCharts();
    }

    async exportPayments() {
        try {
            const params = new URLSearchParams({
                dateFilter: this.currentFilters.dateFilter,
                statusFilter: this.currentFilters.statusFilter,
                methodFilter: this.currentFilters.methodFilter
            });

            const response = await fetch(`/PaymentView/ExportPayments?${params}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `pagos_${new Date().toISOString().split('T')[0]}.xlsx`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                this.showSuccessMessage('Exportación completada exitosamente');
            } else {
                this.showErrorMessage('Error al exportar los pagos');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al exportar');
        }
    }

    async generateDailyReport() {
        try {
            const response = await fetch('/PaymentView/GenerateReport?type=daily', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `reporte_diario_${new Date().toISOString().split('T')[0]}.pdf`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                this.showSuccessMessage('Reporte diario generado exitosamente');
            } else {
                this.showErrorMessage('Error al generar el reporte diario');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al generar reporte');
        }
    }

    async generateWeeklyReport() {
        try {
            const response = await fetch('/PaymentView/GenerateReport?type=weekly', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `reporte_semanal_${new Date().toISOString().split('T')[0]}.pdf`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                this.showSuccessMessage('Reporte semanal generado exitosamente');
            } else {
                this.showErrorMessage('Error al generar el reporte semanal');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al generar reporte');
        }
    }

    async generateMonthlyReport() {
        try {
            const response = await fetch('/PaymentView/GenerateReport?type=monthly', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `reporte_mensual_${new Date().toISOString().split('T')[0]}.pdf`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                this.showSuccessMessage('Reporte mensual generado exitosamente');
            } else {
                this.showErrorMessage('Error al generar el reporte mensual');
            }
        } catch (error) {
            
            this.showErrorMessage('Error de conexión al generar reporte');
        }
    }

    getStatusText(status) {
        const statusMap = {
            'COMPLETED': 'Completado',
            'PENDING': 'Pendiente',
            'CANCELLED': 'Cancelado'
        };
        return statusMap[status] || status;
    }

    showSuccessMessage(message) {
        this.showMessage(message, 'success');
    }

    showErrorMessage(message) {
        this.showMessage(message, 'danger');
    }

    showInfoMessage(message) {
        this.showMessage(message, 'info');
    }

    showMessage(message, type) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'danger' ? 'exclamation-triangle' : 'info-circle'}"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        // Remove existing alerts
        $('.alert').remove();

        // Add new alert
        $('.payment-container').prepend(alertHtml);

        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }
}

// Initialize Payment Manager when document is ready
$(document).ready(function() {
    window.paymentManager = new PaymentManager();
}); 