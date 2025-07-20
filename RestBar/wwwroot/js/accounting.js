/**
 * Accounting Management System
 * Handles all accounting-related functionality
 */
class AccountingManager {
    constructor() {
        this.financialChart = null;
        this.currentPeriod = 'month';
        this.init();
    }

    init() {
        console.log('🔢 Sistema de contabilidad iniciado');
        this.bindEvents();
        this.loadDashboardData();
        this.initializeChart();
    }

    bindEvents() {
        // Header actions
        $('#generateReport').on('click', () => this.showReportModal());
        $('#exportData').on('click', () => this.exportAccountingData());

        // Quick actions
        $('#viewIncome').on('click', () => this.viewIncomeDetails());
        $('#viewExpenses').on('click', () => this.viewExpenseDetails());
        $('#viewProfit').on('click', () => this.viewProfitDetails());
        $('#viewTaxes').on('click', () => this.viewTaxDetails());
        $('#viewReports').on('click', () => this.viewReports());

        // Period filter
        $('#periodFilter').on('change', (e) => {
            this.currentPeriod = e.target.value;
            this.loadDashboardData();
        });

        // Report modal
        $('#reportPeriod').on('change', (e) => {
            if (e.target.value === 'custom') {
                $('#customDateRange').show();
            } else {
                $('#customDateRange').hide();
            }
        });

        $('#generateReportBtn').on('click', () => this.generateReport());
    }

    async loadDashboardData() {
        try {
            // Load financial summary
            await this.loadFinancialSummary();
            
            // Load income details
            await this.loadIncomeDetails();
            
            // Load expense details
            await this.loadExpenseDetails();
            
            // Load tax summary
            await this.loadTaxSummary();
            
            // Update chart
            this.updateChart();
            
        } catch (error) {
            console.error('Error loading dashboard data:', error);
            this.showErrorMessage('Error al cargar datos del dashboard');
        }
    }

    async loadFinancialSummary() {
        try {
            const response = await fetch(`/Accounting/FinancialSummary?period=${this.currentPeriod}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateFinancialStats(data);
            } else {
                // Use mock data for now
                this.updateFinancialStats(this.getMockFinancialData());
            }
        } catch (error) {
            console.error('Error loading financial summary:', error);
            // Use mock data as fallback
            this.updateFinancialStats(this.getMockFinancialData());
        }
    }

    updateFinancialStats(data) {
        $('#totalIncome').text(`$${data.totalIncome.toFixed(2)}`);
        $('#totalExpenses').text(`$${data.totalExpenses.toFixed(2)}`);
        $('#netProfit').text(`$${data.netProfit.toFixed(2)}`);
        $('#totalTaxes').text(`$${data.totalTaxes.toFixed(2)}`);
    }

    async loadIncomeDetails() {
        const loadingElement = $('#incomeLoading');
        const tableBody = $('#incomeTableBody');

        loadingElement.show();
        tableBody.empty();

        try {
            const response = await fetch(`/Accounting/IncomeDetails?period=${this.currentPeriod}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.renderIncomeTable(data);
            } else {
                // Use mock data
                this.renderIncomeTable(this.getMockIncomeData());
            }
        } catch (error) {
            console.error('Error loading income details:', error);
            this.renderIncomeTable(this.getMockIncomeData());
        } finally {
            loadingElement.hide();
        }
    }

    renderIncomeTable(incomeData) {
        const tableBody = $('#incomeTableBody');
        tableBody.empty();

        if (incomeData && incomeData.length > 0) {
            incomeData.forEach(item => {
                const row = `
                    <tr>
                        <td>${item.concept}</td>
                        <td class="text-success fw-bold">$${item.amount.toFixed(2)}</td>
                        <td>${new Date(item.date).toLocaleDateString()}</td>
                        <td><span class="badge bg-success">Completado</span></td>
                    </tr>
                `;
                tableBody.append(row);
            });
        } else {
            tableBody.append(`
                <tr>
                    <td colspan="4" class="text-center text-muted">
                        No hay datos de ingresos disponibles
                    </td>
                </tr>
            `);
        }
    }

    async loadExpenseDetails() {
        const loadingElement = $('#expenseLoading');
        const tableBody = $('#expenseTableBody');

        loadingElement.show();
        tableBody.empty();

        try {
            const response = await fetch(`/Accounting/ExpenseDetails?period=${this.currentPeriod}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.renderExpenseTable(data);
            } else {
                // Use mock data
                this.renderExpenseTable(this.getMockExpenseData());
            }
        } catch (error) {
            console.error('Error loading expense details:', error);
            this.renderExpenseTable(this.getMockExpenseData());
        } finally {
            loadingElement.hide();
        }
    }

    renderExpenseTable(expenseData) {
        const tableBody = $('#expenseTableBody');
        tableBody.empty();

        if (expenseData && expenseData.length > 0) {
            expenseData.forEach(item => {
                const row = `
                    <tr>
                        <td>${item.concept}</td>
                        <td class="text-danger fw-bold">$${item.amount.toFixed(2)}</td>
                        <td>${new Date(item.date).toLocaleDateString()}</td>
                        <td><span class="badge bg-warning">${item.category}</span></td>
                    </tr>
                `;
                tableBody.append(row);
            });
        } else {
            tableBody.append(`
                <tr>
                    <td colspan="4" class="text-center text-muted">
                        No hay datos de gastos disponibles
                    </td>
                </tr>
            `);
        }
    }

    async loadTaxSummary() {
        try {
            const response = await fetch(`/Accounting/TaxSummary?period=${this.currentPeriod}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateTaxSummary(data);
            } else {
                // Use mock data
                this.updateTaxSummary(this.getMockTaxData());
            }
        } catch (error) {
            console.error('Error loading tax summary:', error);
            this.updateTaxSummary(this.getMockTaxData());
        }
    }

    updateTaxSummary(taxData) {
        $('#ivaCollected').text(`$${taxData.ivaCollected.toFixed(2)}`);
        $('#ivaPaid').text(`$${taxData.ivaPaid.toFixed(2)}`);
        $('#ivaToPay').text(`$${taxData.ivaToPay.toFixed(2)}`);
        $('#isr').text(`$${taxData.isr.toFixed(2)}`);
    }

    initializeChart() {
        const ctx = document.getElementById('financialChart').getContext('2d');
        
        this.financialChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Ingresos',
                    data: [],
                    borderColor: '#2ecc71',
                    backgroundColor: 'rgba(46, 204, 113, 0.1)',
                    tension: 0.4
                }, {
                    label: 'Gastos',
                    data: [],
                    borderColor: '#e74c3c',
                    backgroundColor: 'rgba(231, 76, 60, 0.1)',
                    tension: 0.4
                }, {
                    label: 'Beneficio',
                    data: [],
                    borderColor: '#f39c12',
                    backgroundColor: 'rgba(243, 156, 18, 0.1)',
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: 'Resumen Financiero'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return '$' + value.toFixed(2);
                            }
                        }
                    }
                }
            }
        });
    }

    updateChart() {
        // Mock data for chart
        const labels = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'];
        const incomeData = [12000, 15000, 14000, 16000, 18000, 17000];
        const expenseData = [8000, 9000, 8500, 10000, 11000, 10500];
        const profitData = [4000, 6000, 5500, 6000, 7000, 6500];

        this.financialChart.data.labels = labels;
        this.financialChart.data.datasets[0].data = incomeData;
        this.financialChart.data.datasets[1].data = expenseData;
        this.financialChart.data.datasets[2].data = profitData;
        this.financialChart.update();
    }

    showReportModal() {
        $('#reportModal').modal('show');
    }

    async generateReport() {
        const reportType = $('#reportType').val();
        const reportPeriod = $('#reportPeriod').val();
        const startDate = $('#startDate').val();
        const endDate = $('#endDate').val();

        try {
            const response = await fetch('/Accounting/GenerateReport', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    reportType,
                    reportPeriod,
                    startDate,
                    endDate
                })
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `reporte_contable_${reportType}_${new Date().toISOString().split('T')[0]}.pdf`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                
                $('#reportModal').modal('hide');
                this.showSuccessMessage('Reporte generado exitosamente');
            } else {
                this.showErrorMessage('Error al generar el reporte');
            }
        } catch (error) {
            console.error('Error generating report:', error);
            this.showErrorMessage('Error al generar el reporte');
        }
    }

    async exportAccountingData() {
        try {
            const response = await fetch('/Accounting/ExportData', {
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
                a.download = `datos_contables_${new Date().toISOString().split('T')[0]}.xlsx`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                
                this.showSuccessMessage('Datos exportados exitosamente');
            } else {
                this.showErrorMessage('Error al exportar datos');
            }
        } catch (error) {
            console.error('Error exporting data:', error);
            this.showErrorMessage('Error al exportar datos');
        }
    }

    // View methods
    viewIncomeDetails() {
        this.showInfoMessage('Funcionalidad de detalles de ingresos en desarrollo');
    }

    viewExpenseDetails() {
        this.showInfoMessage('Funcionalidad de detalles de gastos en desarrollo');
    }

    viewProfitDetails() {
        this.showInfoMessage('Funcionalidad de detalles de beneficios en desarrollo');
    }

    viewTaxDetails() {
        this.showInfoMessage('Funcionalidad de detalles de impuestos en desarrollo');
    }

    viewReports() {
        this.showInfoMessage('Funcionalidad de reportes en desarrollo');
    }

    // Mock data methods
    getMockFinancialData() {
        return {
            totalIncome: 125000.00,
            totalExpenses: 85000.00,
            netProfit: 40000.00,
            totalTaxes: 15000.00
        };
    }

    getMockIncomeData() {
        return [
            { concept: 'Ventas de Comida', amount: 45000.00, date: '2024-01-15', status: 'completed' },
            { concept: 'Ventas de Bebidas', amount: 18000.00, date: '2024-01-15', status: 'completed' },
            { concept: 'Servicios de Catering', amount: 12000.00, date: '2024-01-14', status: 'completed' },
            { concept: 'Ventas de Postres', amount: 8000.00, date: '2024-01-14', status: 'completed' }
        ];
    }

    getMockExpenseData() {
        return [
            { concept: 'Compra de Ingredientes', amount: 25000.00, date: '2024-01-15', category: 'Insumos' },
            { concept: 'Servicios Públicos', amount: 5000.00, date: '2024-01-15', category: 'Servicios' },
            { concept: 'Salarios Personal', amount: 35000.00, date: '2024-01-14', category: 'Personal' },
            { concept: 'Mantenimiento Equipos', amount: 3000.00, date: '2024-01-14', category: 'Mantenimiento' }
        ];
    }

    getMockTaxData() {
        return {
            ivaCollected: 20000.00,
            ivaPaid: 12000.00,
            ivaToPay: 8000.00,
            isr: 7000.00
        };
    }

    // Utility methods
    showSuccessMessage(message) {
        // You can implement a toast notification system here
        alert('✅ ' + message);
    }

    showErrorMessage(message) {
        // You can implement a toast notification system here
        alert('❌ ' + message);
    }

    showInfoMessage(message) {
        // You can implement a toast notification system here
        alert('ℹ️ ' + message);
    }
}

// Initialize when document is ready
$(document).ready(function() {
    window.accountingManager = new AccountingManager();
}); 