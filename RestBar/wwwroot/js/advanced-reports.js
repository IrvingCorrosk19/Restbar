/**
 * Advanced Reports Management
 * Maneja la carga y visualización de reportes avanzados
 */

// Variables globales
let currentFilters = {
    startDate: null,
    endDate: null,
    branchId: null
};

/**
 * Inicialización al cargar la página
 */
document.addEventListener('DOMContentLoaded', function () {
    try {
        console.log('🔍 [AdvancedReports] init() - Iniciando módulo de reportes avanzados...');
        
        // Establecer fechas por defecto
        const today = new Date();
        const thirtyDaysAgo = new Date();
        thirtyDaysAgo.setDate(today.getDate() - 30);
        
        const startDateEl = document.getElementById('startDate');
        const endDateEl = document.getElementById('endDate');
        
        if (startDateEl) {
            startDateEl.value = thirtyDaysAgo.toISOString().split('T')[0];
        }
        if (endDateEl) {
            endDateEl.value = today.toISOString().split('T')[0];
        }
        
        currentFilters.startDate = thirtyDaysAgo.toISOString().split('T')[0];
        currentFilters.endDate = today.toISOString().split('T')[0];
        
        // Cargar estadísticas iniciales
        loadStatistics();
        
        // Cargar resúmenes de reportes
        loadReport('inventory');
        loadReport('profitability');
        loadReport('sales');
        
        console.log('✅ [AdvancedReports] init() - Módulo inicializado correctamente');
    } catch (error) {
        console.error('❌ [AdvancedReports] init() - Error:', error);
    }
});

/**
 * Aplicar filtros a los reportes
 */
function applyFilters() {
    try {
        console.log('🔍 [AdvancedReports] applyFilters() - Aplicando filtros...');
        
        const startDateEl = document.getElementById('startDate');
        const endDateEl = document.getElementById('endDate');
        
        if (startDateEl) {
            currentFilters.startDate = startDateEl.value;
        }
        if (endDateEl) {
            currentFilters.endDate = endDateEl.value;
        }
        
        console.log('📋 [AdvancedReports] applyFilters() - Filtros:', currentFilters);
        
        // Recargar todas las estadísticas y reportes
        loadStatistics();
        
        const reportCards = document.querySelectorAll('.report-card');
        reportCards.forEach(card => {
            const reportType = card.getAttribute('data-report');
            if (reportType) {
                loadReport(reportType);
            }
        });
        
        console.log('✅ [AdvancedReports] applyFilters() - Filtros aplicados');
    } catch (error) {
        console.error('❌ [AdvancedReports] applyFilters() - Error:', error);
    }
}

/**
 * Cargar estadísticas generales
 */
async function loadStatistics() {
    try {
        console.log('🔍 [AdvancedReports] loadStatistics() - Cargando estadísticas...');
        
        // Cargar estadísticas de inventario
        await loadInventoryStats();
        
        // Cargar estadísticas de rentabilidad
        await loadProfitabilityStats();
        
        // Cargar estadísticas de proveedores
        await loadSupplierStats();
        
        // Cargar estadísticas de tendencias
        await loadTrendsStats();
        
        console.log('✅ [AdvancedReports] loadStatistics() - Estadísticas cargadas');
    } catch (error) {
        console.error('❌ [AdvancedReports] loadStatistics() - Error:', error);
    }
}

/**
 * Cargar estadísticas de inventario
 */
async function loadInventoryStats() {
    try {
        console.log('🔍 [AdvancedReports] loadInventoryStats() - Cargando estadísticas de inventario...');
        
        const url = '/AdvancedReports/GetInventoryAnalysis?' + 
            `startDate=${currentFilters.startDate}&endDate=${currentFilters.endDate}`;
        
        const response = await fetch(url);
        const result = await response.json();
        
        console.log('📡 [AdvancedReports] loadInventoryStats() - Respuesta recibida:', result);
        
        if (result.success && result.data) {
            const data = result.data;
            // Manejar tanto camelCase como PascalCase
            const totalProducts = data.totalProducts || data.TotalProducts || 0;
            const lowStock = data.lowStockProducts || data.LowStockProducts || 0;
            const outOfStock = data.outOfStockProducts || data.OutOfStockProducts || 0;
            
            const statsText = `${totalProducts} productos | ` +
                            `${lowStock} bajo stock | ` +
                            `${outOfStock} sin stock`;
            const statsEl = document.getElementById('inventoryStats');
            if (statsEl) {
                statsEl.textContent = statsText;
            }
            console.log('✅ [AdvancedReports] loadInventoryStats() - Estadísticas actualizadas');
        } else {
            const statsEl = document.getElementById('inventoryStats');
            if (statsEl) {
                statsEl.textContent = 'Sin datos disponibles';
            }
            console.warn('⚠️ [AdvancedReports] loadInventoryStats() - No hay datos');
        }
    } catch (error) {
        console.error('❌ [AdvancedReports] loadInventoryStats() - Error:', error);
        const statsEl = document.getElementById('inventoryStats');
        if (statsEl) {
            statsEl.textContent = 'Error al cargar';
        }
    }
}

/**
 * Cargar estadísticas de rentabilidad
 */
async function loadProfitabilityStats() {
    try {
        console.log('🔍 [AdvancedReports] loadProfitabilityStats() - Cargando estadísticas de rentabilidad...');
        
        const url = '/AdvancedReports/GetProductProfitability?' + 
            `startDate=${currentFilters.startDate}&endDate=${currentFilters.endDate}`;
        
        const response = await fetch(url);
        const result = await response.json();
        
        console.log('📡 [AdvancedReports] loadProfitabilityStats() - Respuesta recibida:', result);
        
        if (result.success && result.data) {
            const dataArray = result.data.$values || result.data || [];
            const totalRevenue = dataArray.reduce((sum, item) => sum + (item.revenue || 0), 0);
            const totalProfit = dataArray.reduce((sum, item) => sum + (item.profit || 0), 0);
            const margin = totalRevenue > 0 ? ((totalProfit / totalRevenue) * 100).toFixed(1) : 0;
            
            const statsText = `$${totalRevenue.toLocaleString()} | Margen: ${margin}%`;
            const statsEl = document.getElementById('profitabilityStats');
            if (statsEl) {
                statsEl.textContent = statsText;
            }
            console.log('✅ [AdvancedReports] loadProfitabilityStats() - Estadísticas actualizadas');
        } else {
            const statsEl = document.getElementById('profitabilityStats');
            if (statsEl) {
                statsEl.textContent = 'Sin datos disponibles';
            }
            console.warn('⚠️ [AdvancedReports] loadProfitabilityStats() - No hay datos');
        }
    } catch (error) {
        console.error('❌ [AdvancedReports] loadProfitabilityStats() - Error:', error);
        const statsEl = document.getElementById('profitabilityStats');
        if (statsEl) {
            statsEl.textContent = 'Error al cargar';
        }
    }
}

/**
 * Cargar estadísticas de proveedores
 */
async function loadSupplierStats() {
    try {
        console.log('🔍 [AdvancedReports] loadSupplierStats() - Cargando estadísticas de proveedores...');
        
        const url = '/AdvancedReports/GetSupplierAnalysis?' + 
            `startDate=${currentFilters.startDate}&endDate=${currentFilters.endDate}`;
        
        const response = await fetch(url);
        const result = await response.json();
        
        console.log('📡 [AdvancedReports] loadSupplierStats() - Respuesta recibida:', result);
        
        if (result.success && result.data) {
            const data = result.data;
            const statsText = `${data.totalSuppliers || 0} proveedores | ` +
                            `$${data.totalPurchases?.toLocaleString() || 0} compras`;
            const statsEl = document.getElementById('supplierStats');
            if (statsEl) {
                statsEl.textContent = statsText;
            }
            console.log('✅ [AdvancedReports] loadSupplierStats() - Estadísticas actualizadas');
        } else {
            const statsEl = document.getElementById('supplierStats');
            if (statsEl) {
                statsEl.textContent = 'Sin datos disponibles';
            }
            console.warn('⚠️ [AdvancedReports] loadSupplierStats() - No hay datos');
        }
    } catch (error) {
        console.error('❌ [AdvancedReports] loadSupplierStats() - Error:', error);
        const statsEl = document.getElementById('supplierStats');
        if (statsEl) {
            statsEl.textContent = 'Error al cargar';
        }
    }
}

/**
 * Cargar estadísticas de tendencias
 */
async function loadTrendsStats() {
    try {
        console.log('🔍 [AdvancedReports] loadTrendsStats() - Cargando estadísticas de tendencias...');
        
        const url = '/AdvancedReports/GetTrendAnalysis?' + 
            `startDate=${currentFilters.startDate}&endDate=${currentFilters.endDate}`;
        
        const response = await fetch(url);
        const result = await response.json();
        
        console.log('📡 [AdvancedReports] loadTrendsStats() - Respuesta recibida:', result);
        
        if (result.success && result.data) {
            const data = result.data;
            const trends = data.salesTrends?.$values || data.salesTrends || [];
            const statsText = `${trends.length} puntos de datos`;
            const statsEl = document.getElementById('trendsStats');
            if (statsEl) {
                statsEl.textContent = statsText;
            }
            console.log('✅ [AdvancedReports] loadTrendsStats() - Estadísticas actualizadas');
        } else {
            const statsEl = document.getElementById('trendsStats');
            if (statsEl) {
                statsEl.textContent = 'Sin datos disponibles';
            }
            console.warn('⚠️ [AdvancedReports] loadTrendsStats() - No hay datos');
        }
    } catch (error) {
        console.error('❌ [AdvancedReports] loadTrendsStats() - Error:', error);
        const statsEl = document.getElementById('trendsStats');
        if (statsEl) {
            statsEl.textContent = 'Error al cargar';
        }
    }
}

/**
 * Cargar resumen de un reporte específico
 */
async function loadReport(reportType) {
    try {
        console.log(`🔍 [AdvancedReports] loadReport(${reportType}) - Cargando reporte...`);
        
        const contentId = `${reportType}ReportContent`;
        const contentEl = document.getElementById(contentId);
        
        if (!contentEl) {
            console.warn(`⚠️ [AdvancedReports] loadReport(${reportType}) - No se encontró elemento para mostrar contenido`);
            return;
        }
        
        contentEl.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Cargando...</div>';
        
        let url = '';
        let endpoint = '';
        
        switch (reportType) {
            case 'inventory':
                endpoint = '/AdvancedReports/GetInventoryAnalysis';
                break;
            case 'profitability':
                endpoint = '/AdvancedReports/GetProductProfitability';
                break;
            case 'sales':
                endpoint = '/AdvancedReports/GetTopSellingProducts';
                break;
            case 'customers':
                endpoint = '/AdvancedReports/GetTopCustomers';
                break;
            case 'operational':
                endpoint = '/AdvancedReports/GetStationPerformance';
                break;
            case 'trends':
                endpoint = '/AdvancedReports/GetTrendAnalysis';
                break;
            case 'audit':
                endpoint = '/AdvancedReports/GetAuditReport';
                break;
            case 'health':
                endpoint = '/AdvancedReports/GetSystemHealth';
                break;
            default:
                console.warn(`⚠️ [AdvancedReports] loadReport(${reportType}) - Tipo de reporte no reconocido`);
                contentEl.innerHTML = '<div class="text-muted">Tipo de reporte no reconocido</div>';
                return;
        }
        
        url = endpoint + '?' + 
            `startDate=${currentFilters.startDate}&endDate=${currentFilters.endDate}`;
        
        console.log(`📡 [AdvancedReports] loadReport(${reportType}) - Enviando petición a: ${url}`);
        
        const response = await fetch(url);
        const result = await response.json();
        
        console.log(`📡 [AdvancedReports] loadReport(${reportType}) - Respuesta recibida:`, result);
        
        if (result.success && result.data) {
            renderReportSummary(reportType, result.data, contentEl);
            console.log(`✅ [AdvancedReports] loadReport(${reportType}) - Reporte cargado`);
        } else {
            contentEl.innerHTML = '<div class="text-muted small">No hay datos disponibles para este período</div>';
            console.warn(`⚠️ [AdvancedReports] loadReport(${reportType}) - No hay datos`);
        }
    } catch (error) {
        console.error(`❌ [AdvancedReports] loadReport(${reportType}) - Error:`, error);
        const contentId = `${reportType}ReportContent`;
        const contentEl = document.getElementById(contentId);
        if (contentEl) {
            contentEl.innerHTML = '<div class="text-danger small">Error al cargar el reporte</div>';
        }
    }
}

/**
 * Renderizar resumen de reporte
 */
function renderReportSummary(reportType, data, contentEl) {
    try {
        console.log(`🔍 [AdvancedReports] renderReportSummary(${reportType}) - Renderizando resumen...`);
        
        let html = '';
        
        switch (reportType) {
            case 'inventory':
                html = renderInventorySummary(data);
                break;
            case 'profitability':
                html = renderProfitabilitySummary(data);
                break;
            case 'sales':
                html = renderSalesSummary(data);
                break;
            case 'customers':
                html = renderCustomersSummary(data);
                break;
            case 'operational':
                html = renderOperationalSummary(data);
                break;
            case 'trends':
                html = renderTrendsSummary(data);
                break;
            case 'audit':
                html = renderAuditSummary(data);
                break;
            case 'health':
                html = renderHealthSummary(data);
                break;
            default:
                html = '<div class="text-muted small">Resumen no disponible</div>';
        }
        
        if (contentEl) {
            contentEl.innerHTML = html;
        }
        console.log(`✅ [AdvancedReports] renderReportSummary(${reportType}) - Resumen renderizado`);
    } catch (error) {
        console.error(`❌ [AdvancedReports] renderReportSummary(${reportType}) - Error:`, error);
        if (contentEl) {
            contentEl.innerHTML = '<div class="text-danger small">Error al renderizar el resumen</div>';
        }
    }
}

/**
 * Renderizar resumen de inventario
 */
function renderInventorySummary(data) {
    // Manejar tanto camelCase como PascalCase
    const lowStock = data.lowStockProducts || data.LowStockProducts || 0;
    const outOfStock = data.outOfStockProducts || data.OutOfStockProducts || 0;
    const totalValue = data.totalInventoryValue || data.TotalInventoryValue || 0;
    const totalProducts = data.totalProducts || data.TotalProducts || 0;
    
    let html = '<div class="small">';
    html += `<div class="mb-2"><strong>Productos:</strong> ${totalProducts}</div>`;
    html += `<div class="mb-2"><span class="badge bg-warning">Bajo stock: ${lowStock}</span> `;
    html += `<span class="badge bg-danger">Sin stock: ${outOfStock}</span></div>`;
    html += `<div><strong>Valor total:</strong> $${totalValue.toLocaleString()}</div>`;
    html += '</div>';
    
    return html;
}

/**
 * Renderizar resumen de rentabilidad
 */
function renderProfitabilitySummary(data) {
    const dataArray = data.$values || data || [];
    const top5 = dataArray.slice(0, 5);
    
    let html = '<div class="small">';
    html += '<div class="mb-2"><strong>Top 5 Productos:</strong></div>';
    html += '<ul class="list-unstyled">';
    top5.forEach(item => {
        const margin = item.profitMargin?.toFixed(1) || 0;
        html += `<li class="mb-1">${item.productName || 'N/A'}: <strong>${margin}%</strong></li>`;
    });
    html += '</ul>';
    html += '</div>';
    
    return html;
}

/**
 * Renderizar resumen de ventas
 */
function renderSalesSummary(data) {
    const dataArray = data.$values || data || [];
    const top5 = dataArray.slice(0, 5);
    
    let html = '<div class="small">';
    html += '<div class="mb-2"><strong>Top 5 Productos:</strong></div>';
    html += '<ul class="list-unstyled">';
    top5.forEach(item => {
        html += `<li class="mb-1">${item.productName || 'N/A'}: <strong>${item.unitsSold || 0} u.</strong></li>`;
    });
    html += '</ul>';
    html += '</div>';
    
    return html;
}

/**
 * Renderizar resumen de clientes
 */
function renderCustomersSummary(data) {
    const dataArray = data.$values || data || [];
    const top5 = dataArray.slice(0, 5);
    
    let html = '<div class="small">';
    html += '<div class="mb-2"><strong>Top 5 Clientes:</strong></div>';
    html += '<ul class="list-unstyled">';
    top5.forEach(item => {
        html += `<li class="mb-1">${item.customerName || 'N/A'}: <strong>$${item.totalSpent?.toLocaleString() || 0}</strong></li>`;
    });
    html += '</ul>';
    html += '</div>';
    
    return html;
}

/**
 * Renderizar resumen operacional
 */
function renderOperationalSummary(data) {
    const dataArray = data.$values || data || [];
    
    let html = '<div class="small">';
    html += '<div class="mb-2"><strong>Rendimiento de Estaciones:</strong></div>';
    html += '<ul class="list-unstyled">';
    dataArray.slice(0, 3).forEach(item => {
        html += `<li class="mb-1">${item.stationName || 'N/A'}: <strong>${item.ordersProcessed || 0} órdenes</strong></li>`;
    });
    html += '</ul>';
    html += '</div>';
    
    return html;
}

/**
 * Renderizar resumen de tendencias
 */
function renderTrendsSummary(data) {
    const trends = data.salesTrends?.$values || data.salesTrends || [];
    
    let html = '<div class="small">';
    html += `<div class="mb-2"><strong>Puntos de datos:</strong> ${trends.length}</div>`;
    html += '<div><strong>Estado:</strong> Datos disponibles</div>';
    html += '</div>';
    
    return html;
}

/**
 * Renderizar resumen de auditoría
 */
function renderAuditSummary(data) {
    let html = '<div class="small">';
    html += `<div class="mb-2"><strong>Total logs:</strong> ${data.totalLogs || 0}</div>`;
    html += `<div class="mb-2"><span class="badge bg-danger">Errores: ${data.errorLogs || 0}</span> `;
    html += `<span class="badge bg-warning">Advertencias: ${data.warningLogs || 0}</span></div>`;
    html += '</div>';
    
    return html;
}

/**
 * Renderizar resumen de salud del sistema
 */
function renderHealthSummary(data) {
    const status = data.overallStatus || 'Unknown';
    const statusClass = status === 'Healthy' ? 'success' : status === 'Warning' ? 'warning' : 'danger';
    
    let html = '<div class="small">';
    html += `<div class="mb-2"><strong>Estado:</strong> <span class="badge bg-${statusClass}">${status}</span></div>`;
    html += `<div class="mb-2"><strong>Uptime:</strong> ${data.systemUptime?.toFixed(1) || 0}%</div>`;
    html += `<div><strong>Usuarios activos:</strong> ${data.activeUsers || 0}</div>`;
    html += '</div>';
    
    return html;
}

/**
 * Exportar reporte
 */
function exportReport(reportType, format) {
    try {
        console.log(`🔍 [AdvancedReports] exportReport(${reportType}, ${format}) - Iniciando exportación...`);
        
        const startDate = currentFilters.startDate;
        const endDate = currentFilters.endDate;
        
        let url = '';
        if (format === 'pdf') {
            url = `/AdvancedReports/ExportToPdf?reportType=${reportType}&startDate=${startDate}&endDate=${endDate}`;
        } else if (format === 'excel') {
            url = `/AdvancedReports/ExportToExcel?reportType=${reportType}&startDate=${startDate}&endDate=${endDate}`;
        }
        
        console.log(`📤 [AdvancedReports] exportReport(${reportType}, ${format}) - URL: ${url}`);
        
        window.open(url, '_blank');
        
        console.log(`✅ [AdvancedReports] exportReport(${reportType}, ${format}) - Exportación iniciada`);
    } catch (error) {
        console.error(`❌ [AdvancedReports] exportReport(${reportType}, ${format}) - Error:`, error);
        alert('Error al exportar el reporte');
    }
}

