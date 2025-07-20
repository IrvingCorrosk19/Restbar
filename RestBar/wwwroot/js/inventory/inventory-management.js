// Variables globales para inventario
let inventoryData = [];
let currentFilters = {
    branch: '',
    category: '',
    stockStatus: '',
    search: ''
};

// Función para cargar datos de inventario
async function loadInventoryData() {
    try {
        console.log('[Frontend] Cargando datos de inventario...');
        
        const response = await fetch('/Inventory/GetInventoryData');
        if (!response.ok) {
            throw new Error('Error al cargar datos de inventario');
        }
        
        const data = await response.json();
        inventoryData = data.inventory || [];
        
        console.log('[Frontend] Datos de inventario cargados:', inventoryData.length, 'items');
        
        renderInventoryTable();
        updatePagination();
        
    } catch (error) {
        console.error('[Frontend] Error cargando inventario:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudieron cargar los datos de inventario'
        });
    }
}

// Función para renderizar la tabla de inventario
function renderInventoryTable() {
    const tbody = document.getElementById('inventoryTableBody');
    if (!tbody) return;
    
    const filteredData = filterInventoryData();
    
    tbody.innerHTML = '';
    
    if (filteredData.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted">
                    <i class="fas fa-box-open me-2"></i>No se encontraron productos en inventario
                </td>
            </tr>
        `;
        return;
    }
    
    filteredData.forEach(item => {
        const row = createInventoryRow(item);
        tbody.appendChild(row);
    });
}

// Función para crear una fila de inventario
function createInventoryRow(item) {
    const row = document.createElement('tr');
    
    const stockStatus = getStockStatus(item.quantity, item.minStock);
    const statusClass = getStockStatusClass(stockStatus);
    
    row.innerHTML = `
        <td>
            <div class="d-flex align-items-center">
                <div class="me-3">
                    <i class="fas fa-box text-primary"></i>
                </div>
                <div>
                    <strong>${item.productName}</strong>
                    <br>
                    <small class="text-muted">${item.productDescription || ''}</small>
                </div>
            </div>
        </td>
        <td>
            <span class="badge bg-secondary">${item.categoryName || 'Sin categoría'}</span>
        </td>
        <td>
            <span class="badge bg-info">${item.branchName}</span>
        </td>
        <td>
            <span class="fw-bold ${stockStatus === 'low' ? 'text-warning' : stockStatus === 'out' ? 'text-danger' : 'text-success'}">
                ${item.quantity.toFixed(2)}
            </span>
        </td>
        <td>
            <span class="text-muted">${item.minStock || 0}</span>
        </td>
        <td>
            <span class="badge ${statusClass}">${getStockStatusText(stockStatus)}</span>
        </td>
        <td>
            <small class="text-muted">
                ${item.lastUpdated ? new Date(item.lastUpdated).toLocaleString() : 'N/A'}
            </small>
        </td>
        <td>
            <div class="btn-group btn-group-sm" role="group">
                <button type="button" class="btn btn-outline-primary" 
                        onclick="openUpdateStockModal('${item.productId}', '${item.branchId}', '${item.productName}', '${item.branchName}', ${item.quantity})">
                    <i class="fas fa-edit"></i>
                </button>
                <button type="button" class="btn btn-outline-info" 
                        onclick="viewStockHistory('${item.productId}', '${item.branchId}')">
                    <i class="fas fa-history"></i>
                </button>
            </div>
        </td>
    `;
    
    return row;
}

// Función para filtrar datos de inventario
function filterInventoryData() {
    return inventoryData.filter(item => {
        // Filtro por sucursal
        if (currentFilters.branch && item.branchId !== currentFilters.branch) {
            return false;
        }
        
        // Filtro por categoría
        if (currentFilters.category && item.categoryId !== currentFilters.category) {
            return false;
        }
        
        // Filtro por estado de stock
        if (currentFilters.stockStatus) {
            const status = getStockStatus(item.quantity, item.minStock);
            if (status !== currentFilters.stockStatus) {
                return false;
            }
        }
        
        // Filtro por búsqueda
        if (currentFilters.search) {
            const searchTerm = currentFilters.search.toLowerCase();
            if (!item.productName.toLowerCase().includes(searchTerm)) {
                return false;
            }
        }
        
        return true;
    });
}

// Función para obtener estado de stock
function getStockStatus(quantity, minStock) {
    if (quantity <= 0) return 'out';
    if (quantity <= (minStock || 5)) return 'low';
    return 'normal';
}

// Función para obtener clase CSS del estado de stock
function getStockStatusClass(status) {
    switch (status) {
        case 'out': return 'bg-danger';
        case 'low': return 'bg-warning';
        case 'normal': return 'bg-success';
        default: return 'bg-secondary';
    }
}

// Función para obtener texto del estado de stock
function getStockStatusText(status) {
    switch (status) {
        case 'out': return 'Sin Stock';
        case 'low': return 'Bajo Stock';
        case 'normal': return 'Stock Normal';
        default: return 'Desconocido';
    }
}

// Función para configurar filtros
function setupFilters() {
    // Filtro por sucursal
    const branchFilter = document.getElementById('branchFilter');
    if (branchFilter) {
        branchFilter.addEventListener('change', function() {
            currentFilters.branch = this.value;
            renderInventoryTable();
        });
    }
    
    // Filtro por categoría
    const categoryFilter = document.getElementById('categoryFilter');
    if (categoryFilter) {
        categoryFilter.addEventListener('change', function() {
            currentFilters.category = this.value;
            renderInventoryTable();
        });
    }
    
    // Filtro por estado de stock
    const stockFilter = document.getElementById('stockFilter');
    if (stockFilter) {
        stockFilter.addEventListener('change', function() {
            currentFilters.stockStatus = this.value;
            renderInventoryTable();
        });
    }
    
    // Filtro por búsqueda
    const searchFilter = document.getElementById('searchProduct');
    if (searchFilter) {
        searchFilter.addEventListener('input', function() {
            currentFilters.search = this.value;
            renderInventoryTable();
        });
    }
}

// Función para abrir modal de actualización de stock
function openUpdateStockModal(productId, branchId, productName, branchName, currentQuantity) {
    document.getElementById('updateProductId').value = productId;
    document.getElementById('updateBranchId').value = branchId;
    document.getElementById('productNameDisplay').value = productName;
    document.getElementById('branchNameDisplay').value = branchName;
    document.getElementById('currentStockDisplay').value = currentQuantity.toFixed(2);
    document.getElementById('newStockQuantity').value = currentQuantity.toFixed(2);
    document.getElementById('stockReason').value = '';
    
    const modal = new bootstrap.Modal(document.getElementById('updateStockModal'));
    modal.show();
}

// Función para guardar actualización de stock
async function saveStockUpdate() {
    const productId = document.getElementById('updateProductId').value;
    const branchId = document.getElementById('updateBranchId').value;
    const newQuantity = parseFloat(document.getElementById('newStockQuantity').value);
    const reason = document.getElementById('stockReason').value;
    
    if (isNaN(newQuantity) || newQuantity < 0) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Por favor ingrese una cantidad válida'
        });
        return;
    }
    
    try {
        const response = await fetch('/Inventory/UpdateStock', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                productId: productId,
                branchId: branchId,
                newQuantity: newQuantity,
                reason: reason
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Éxito',
                text: 'Stock actualizado correctamente'
            });
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('updateStockModal'));
            modal.hide();
            
            // Recargar datos
            await loadInventoryData();
            
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message || 'Error al actualizar stock'
            });
        }
        
    } catch (error) {
        console.error('[Frontend] Error actualizando stock:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error de conexión al actualizar stock'
        });
    }
}

// Función para ver historial de stock
async function viewStockHistory(productId, branchId) {
    try {
        const response = await fetch(`/Inventory/GetStockHistory?productId=${productId}&branchId=${branchId}`);
        const result = await response.json();
        
        if (result.success) {
            displayStockHistory(result.history);
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message || 'Error al cargar historial'
            });
        }
        
    } catch (error) {
        console.error('[Frontend] Error cargando historial:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error de conexión al cargar historial'
        });
    }
}

// Función para mostrar historial de stock
function displayStockHistory(history) {
    const content = document.getElementById('stockHistoryContent');
    
    if (!history || history.length === 0) {
        content.innerHTML = `
            <div class="text-center text-muted">
                <i class="fas fa-info-circle me-2"></i>No hay historial disponible
            </div>
        `;
    } else {
        const historyHtml = history.map(item => `
            <div class="border-bottom pb-2 mb-2">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <strong>${item.changeType}</strong>
                        <br>
                        <small class="text-muted">${item.reason || 'Sin motivo especificado'}</small>
                    </div>
                    <div class="text-end">
                        <span class="badge ${item.quantityChange > 0 ? 'bg-success' : 'bg-danger'}">
                            ${item.quantityChange > 0 ? '+' : ''}${item.quantityChange}
                        </span>
                        <br>
                        <small class="text-muted">${new Date(item.timestamp).toLocaleString()}</small>
                    </div>
                </div>
            </div>
        `).join('');
        
        content.innerHTML = historyHtml;
    }
    
    const modal = new bootstrap.Modal(document.getElementById('stockHistoryModal'));
    modal.show();
}

// Función para cargar alertas de bajo stock
async function loadLowStockAlerts() {
    try {
        const response = await fetch('/Inventory/GetLowStockReport');
        const result = await response.json();
        
        if (result.success && result.items.length > 0) {
            const alertsDiv = document.getElementById('lowStockAlerts');
            const itemsList = document.getElementById('lowStockItemsList');
            
            const itemsHtml = result.items.map(item => `
                <div class="mb-2">
                    <strong>${item.productName}</strong> - 
                    <span class="badge bg-warning">${item.currentStock} unidades</span>
                    <small class="text-muted">(${item.branchName})</small>
                </div>
            `).join('');
            
            itemsList.innerHTML = itemsHtml;
            alertsDiv.style.display = 'block';
        }
        
    } catch (error) {
        console.error('[Frontend] Error cargando alertas de bajo stock:', error);
    }
}

// Función para actualizar paginación
function updatePagination() {
    // Implementar paginación si es necesario
    console.log('[Frontend] Paginación actualizada');
}

// Event listeners
document.addEventListener('DOMContentLoaded', function() {
    // Configurar botón de guardar stock
    const saveButton = document.getElementById('saveStockUpdate');
    if (saveButton) {
        saveButton.addEventListener('click', saveStockUpdate);
    }
    
    // Configurar formulario de actualización
    const updateForm = document.getElementById('updateStockForm');
    if (updateForm) {
        updateForm.addEventListener('submit', function(e) {
            e.preventDefault();
            saveStockUpdate();
        });
    }
});

// Función para sincronizar stock de todos los productos
async function syncAllStock() {
    try {
        console.log('[Frontend] 🔄 syncAllStock iniciado');
        console.log('[Frontend] Llamando al endpoint /Inventory/SyncAllStock...');
        
        // Mostrar loading
        Swal.fire({
            title: 'Sincronizando Stock',
            text: 'Actualizando stock de todos los productos...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        console.log('[Frontend] Enviando petición al servidor...');
        const response = await fetch('/Inventory/SyncAllStock');
        console.log('[Frontend] Respuesta recibida:', response.status, response.statusText);
        
        const data = await response.json();
        console.log('[Frontend] Datos de respuesta:', data);

        if (data.success) {
            console.log('[Frontend] ✅ Sincronización exitosa');
            Swal.fire({
                icon: 'success',
                title: 'Sincronización Completada',
                text: data.message,
                timer: 2000,
                showConfirmButton: false
            });

            // Recargar datos de inventario
            console.log('[Frontend] Recargando datos de inventario...');
            await loadInventoryData();
            console.log('[Frontend] ✅ Datos de inventario recargados');
        } else {
            console.log('[Frontend] ❌ Error en sincronización:', data.message);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: data.message
            });
        }
    } catch (error) {
        console.error('[Frontend] ❌ Error sincronizando stock:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudo sincronizar el stock'
        });
    }
}

async function checkDatabaseData() {
    try {
        console.log('[Frontend] Verificando datos de la base de datos...');
        
        const response = await fetch('/Inventory/CheckDatabaseData');
        if (!response.ok) {
            throw new Error('Error al verificar datos');
        }
        
        const result = await response.json();
        
        if (result.success) {
            const data = result.data;
            console.log('[Frontend] Datos de la base de datos:', data);
            
            Swal.fire({
                icon: 'info',
                title: 'Estado de la Base de Datos',
                html: `
                    <div class="text-start">
                        <p><strong>Productos:</strong> ${data.productsCount}</p>
                        <p><strong>Categorías:</strong> ${data.categoriesCount}</p>
                        <p><strong>Sucursales:</strong> ${data.branchesCount}</p>
                        <p><strong>Inventarios:</strong> ${data.inventoriesCount}</p>
                    </div>
                `,
                confirmButtonText: 'Entendido'
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message || 'Error al verificar datos'
            });
        }
        
    } catch (error) {
        console.error('[Frontend] Error verificando datos:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error de conexión al verificar datos'
        });
    }
}

// Exportar funciones para uso global
window.openUpdateStockModal = openUpdateStockModal;
window.viewStockHistory = viewStockHistory;
window.saveStockUpdate = saveStockUpdate;
window.checkDatabaseData = checkDatabaseData;
window.syncAllStock = syncAllStock; 