// ✅ NUEVO: Script para análisis de inventario
try {
    console.log('🔍 [AdvancedReports/InventoryAnalysis] Cargando script...');

    $(document).ready(function() {
        console.log('🔍 [AdvancedReports/InventoryAnalysis] Documento listo');
        
        // Inicializar fechas
        var today = new Date();
        var lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, today.getDate());
        
        $('#startDate').val(lastMonth.toISOString().split('T')[0]);
        $('#endDate').val(today.toISOString().split('T')[0]);
        
        // Cargar datos iniciales
        loadInventoryAnalysis();
    });

    function loadInventoryAnalysis() {
        try {
            console.log('🔍 [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - Iniciando...');
            
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();
            var branchId = $('#branchId').val() || '';

            console.log('📋 [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - Filtros:', {
                startDate: startDate,
                endDate: endDate,
                branchId: branchId
            });

            // Mostrar loading
            showLoading();

            $.ajax({
                url: '/AdvancedReports/GetInventoryAnalysis',
                type: 'GET',
                data: {
                    startDate: startDate,
                    endDate: endDate,
                    branchId: branchId
                },
                success: function(response) {
                    console.log('📡 [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - Respuesta recibida:', response);
                    
                    if (response.success && response.data) {
                        const data = response.data.$values || response.data;
                        console.log('📊 [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - Datos procesados:', data);
                        
                        updateStatistics(data);
                        updateLowStockTable(data.lowStockAlerts || []);
                        updateTurnoverTable(data.turnoverData || []);
                        updateValueTable(data.valueReport || []);
                        
                        console.log('✅ [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - Datos actualizados');
                    } else {
                        console.warn('⚠️ [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - Respuesta sin datos válidos');
                        showError('No se pudieron cargar los datos del análisis de inventario');
                    }
                    
                    hideLoading();
                },
                error: function(xhr, status, error) {
                    console.error('❌ [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - Error:', error);
                    console.error('📡 [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - XHR:', xhr);
                    hideLoading();
                    showError('Error al cargar el análisis de inventario: ' + error);
                }
            });
        } catch (error) {
            console.error('❌ [AdvancedReports/InventoryAnalysis] loadInventoryAnalysis() - Excepción:', error);
            hideLoading();
            showError('Error al cargar el análisis de inventario');
        }
    }

    function updateStatistics(data) {
        try {
            console.log('🔍 [AdvancedReports/InventoryAnalysis] updateStatistics() - Actualizando estadísticas');
            
            $('#totalProducts').text(data.totalProducts || 0);
            $('#lowStockProducts').text(data.lowStockProducts || 0);
            $('#outOfStockProducts').text(data.outOfStockProducts || 0);
            
            const totalValue = data.totalInventoryValue || 0;
            $('#totalInventoryValue').text('$' + totalValue.toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
            
            console.log('✅ [AdvancedReports/InventoryAnalysis] updateStatistics() - Estadísticas actualizadas');
        } catch (error) {
            console.error('❌ [AdvancedReports/InventoryAnalysis] updateStatistics() - Error:', error);
        }
    }

    function updateLowStockTable(alerts) {
        try {
            console.log('🔍 [AdvancedReports/InventoryAnalysis] updateLowStockTable() - Actualizando tabla de alertas');
            
            const dataArray = alerts.$values || alerts || [];
            const tbody = $('#lowStockTableBody');
            tbody.empty();

            if (dataArray.length === 0) {
                tbody.append('<tr><td colspan="8" class="text-center text-muted">No hay alertas de bajo stock</td></tr>');
                console.log('⚠️ [AdvancedReports/InventoryAnalysis] updateLowStockTable() - No hay alertas');
                return;
            }

            dataArray.forEach(function(alert) {
                const alertLevelClass = alert.alertLevel === 'Critical' ? 'danger' : 
                                      alert.alertLevel === 'Warning' ? 'warning' : 'info';
                const alertLevelText = alert.alertLevel === 'Critical' ? 'Crítico' : 
                                     alert.alertLevel === 'Warning' ? 'Advertencia' : 'Normal';
                
                const lastUpdated = alert.lastUpdated ? new Date(alert.lastUpdated).toLocaleDateString('es-MX') : 'N/A';
                
                const row = `
                    <tr>
                        <td>${escapeHtml(alert.productName || 'N/A')}</td>
                        <td>${escapeHtml(alert.categoryName || 'N/A')}</td>
                        <td>${escapeHtml(alert.branchName || 'N/A')}</td>
                        <td>${alert.currentStock || 0}</td>
                        <td>${alert.minStock || 0}</td>
                        <td>${alert.reorderPoint || 0}</td>
                        <td><span class="badge badge-${alertLevelClass}">${alertLevelText}</span></td>
                        <td>${lastUpdated}</td>
                    </tr>
                `;
                tbody.append(row);
            });
            
            console.log(`✅ [AdvancedReports/InventoryAnalysis] updateLowStockTable() - ${dataArray.length} alertas mostradas`);
        } catch (error) {
            console.error('❌ [AdvancedReports/InventoryAnalysis] updateLowStockTable() - Error:', error);
        }
    }

    function updateTurnoverTable(turnoverData) {
        try {
            console.log('🔍 [AdvancedReports/InventoryAnalysis] updateTurnoverTable() - Actualizando tabla de rotación');
            
            const dataArray = turnoverData.$values || turnoverData || [];
            const tbody = $('#turnoverTableBody');
            tbody.empty();

            if (dataArray.length === 0) {
                tbody.append('<tr><td colspan="7" class="text-center text-muted">No hay datos de rotación disponibles</td></tr>');
                console.log('⚠️ [AdvancedReports/InventoryAnalysis] updateTurnoverTable() - No hay datos');
                return;
            }

            dataArray.forEach(function(item) {
                const efficiencyClass = item.efficiency === 'High' ? 'success' : 
                                      item.efficiency === 'Medium' ? 'warning' : 'danger';
                const efficiencyText = item.efficiency === 'High' ? 'Alta' : 
                                      item.efficiency === 'Medium' ? 'Media' : 'Baja';
                
                const row = `
                    <tr>
                        <td>${escapeHtml(item.productName || 'N/A')}</td>
                        <td>${escapeHtml(item.categoryName || 'N/A')}</td>
                        <td>${item.averageStock || 0}</td>
                        <td>${item.totalSold || 0}</td>
                        <td>${(item.turnoverRate || 0).toFixed(2)}</td>
                        <td>${item.daysToSell || 0}</td>
                        <td><span class="badge badge-${efficiencyClass}">${efficiencyText}</span></td>
                    </tr>
                `;
                tbody.append(row);
            });
            
            console.log(`✅ [AdvancedReports/InventoryAnalysis] updateTurnoverTable() - ${dataArray.length} items mostrados`);
        } catch (error) {
            console.error('❌ [AdvancedReports/InventoryAnalysis] updateTurnoverTable() - Error:', error);
        }
    }

    function updateValueTable(valueReport) {
        try {
            console.log('🔍 [AdvancedReports/InventoryAnalysis] updateValueTable() - Actualizando tabla de valores');
            
            const dataArray = valueReport.$values || valueReport || [];
            const tbody = $('#valueTableBody');
            tbody.empty();

            if (dataArray.length === 0) {
                tbody.append('<tr><td colspan="8" class="text-center text-muted">No hay datos de valor disponibles</td></tr>');
                console.log('⚠️ [AdvancedReports/InventoryAnalysis] updateValueTable() - No hay datos');
                return;
            }

            dataArray.forEach(function(item) {
                const valueChangeClass = item.valueChange >= 0 ? 'text-success' : 'text-danger';
                const valueChangeSign = item.valueChange >= 0 ? '+' : '';
                
                const row = `
                    <tr>
                        <td>${escapeHtml(item.productName || 'N/A')}</td>
                        <td>${escapeHtml(item.categoryName || 'N/A')}</td>
                        <td>${item.currentStock || 0}</td>
                        <td>$${(_formatDecimal(item.unitCost || 0))}</td>
                        <td>$${(_formatDecimal(item.totalValue || 0))}</td>
                        <td>$${(_formatDecimal(item.lastMonthValue || 0))}</td>
                        <td class="${valueChangeClass}">${valueChangeSign}$${(_formatDecimal(item.valueChange || 0))}</td>
                        <td>${escapeHtml(item.branchName || 'N/A')}</td>
                    </tr>
                `;
                tbody.append(row);
            });
            
            console.log(`✅ [AdvancedReports/InventoryAnalysis] updateValueTable() - ${dataArray.length} items mostrados`);
        } catch (error) {
            console.error('❌ [AdvancedReports/InventoryAnalysis] updateValueTable() - Error:', error);
        }
    }

    function exportToExcel() {
        try {
            console.log('🔍 [AdvancedReports/InventoryAnalysis] exportToExcel() - Iniciando...');
            
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();
            var branchId = $('#branchId').val() || '';

            window.location.href = `/AdvancedReports/ExportToExcel?reportType=inventory&startDate=${startDate}&endDate=${endDate}&branchId=${branchId}`;
        } catch (error) {
            console.error('❌ [AdvancedReports/InventoryAnalysis] exportToExcel() - Error:', error);
            showError('Error al exportar a Excel');
        }
    }

    function exportToPdf() {
        try {
            console.log('🔍 [AdvancedReports/InventoryAnalysis] exportToPdf() - Iniciando...');
            
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();
            var branchId = $('#branchId').val() || '';

            window.location.href = `/AdvancedReports/ExportToPdf?reportType=inventory&startDate=${startDate}&endDate=${endDate}&branchId=${branchId}`;
        } catch (error) {
            console.error('❌ [AdvancedReports/InventoryAnalysis] exportToPdf() - Error:', error);
            showError('Error al exportar a PDF');
        }
    }

    function showLoading() {
        $('#lowStockTableBody, #turnoverTableBody, #valueTableBody').html('<tr><td colspan="10" class="text-center"><i class="fas fa-spinner fa-spin"></i> Cargando...</td></tr>');
    }

    function hideLoading() {
        // El loading se reemplaza automáticamente por los datos
    }

    function showError(message) {
        alert(message);
    }

    function escapeHtml(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return String(text).replace(/[&<>"']/g, m => map[m]);
    }

    function _formatDecimal(value) {
        return Number(value).toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    console.log('✅ [AdvancedReports/InventoryAnalysis] Script cargado exitosamente');
} catch (error) {
    console.error('❌ [AdvancedReports/InventoryAnalysis] Error al cargar script:', error);
}

