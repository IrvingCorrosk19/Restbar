// Variables globales para movimientos de inventario
let movementsData = [];
let currentMovementFilters = {
    type: '',
    productId: '',
    branchId: '',
    startDate: '',
    endDate: ''
};

// Función para cargar datos de movimientos
async function loadMovementsData() {
    try {
        
        
        const response = await fetch('/InventoryMovement/GetMovementsByDateRange?' + new URLSearchParams({
            startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString(),
            endDate: new Date().toISOString()
        }));
        
        if (!response.ok) {
            throw new Error('Error al cargar datos de movimientos');
        }
        
        const data = await response.json();
        movementsData = data.movements || [];
        
        
        
        renderMovementsTable();
        updateMovementStats();
        
    } catch (error) {
        
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudieron cargar los datos de movimientos'
        });
    }
}

// Función para renderizar la tabla de movimientos
function renderMovementsTable() {
    const tbody = document.getElementById('movementsTableBody');
    if (!tbody) return;
    
    const filteredData = filterMovementsData();
    
    tbody.innerHTML = '';
    
    if (filteredData.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="11" class="text-center text-muted">
                    <i class="fas fa-exchange-alt me-2"></i>No se encontraron movimientos
                </td>
            </tr>
        `;
        return;
    }
    
    filteredData.forEach(movement => {
        const row = createMovementRow(movement);
        tbody.appendChild(row);
    });
}

// Función para crear una fila de movimiento
function createMovementRow(movement) {
    const row = document.createElement('tr');
    
    const typeClass = getMovementTypeClass(movement.type);
    const quantityClass = movement.quantity >= 0 ? 'quantity-positive' : 'quantity-negative';
    const quantitySign = movement.quantity >= 0 ? '+' : '';
    
    row.innerHTML = `
        <td>
            <small>${formatDateTime(movement.createdAt)}</small>
        </td>
        <td>
            <span class="movement-type-badge ${typeClass}">${getMovementTypeText(movement.type)}</span>
        </td>
        <td>
            <strong>${movement.productName || 'N/A'}</strong>
        </td>
        <td>
            <span class="badge bg-info">${movement.branchName || 'N/A'}</span>
        </td>
        <td>
            <span class="${quantityClass}">${quantitySign}${movement.quantity}</span>
        </td>
        <td>
            <span class="text-muted">${movement.previousStock || 0}</span>
        </td>
        <td>
            <span class="fw-bold">${movement.newStock || 0}</span>
        </td>
        <td>
            <small>${movement.userName || 'Sistema'}</small>
        </td>
        <td>
            <small>${movement.reason || 'N/A'}</small>
        </td>
        <td>
            <code class="small">${movement.reference || 'N/A'}</code>
        </td>
        <td>
            <div class="btn-group btn-group-sm">
                <button type="button" class="btn btn-outline-primary btn-sm" onclick="viewMovementDetails('${movement.id}')" title="Ver detalles">
                    <i class="fas fa-eye"></i>
                </button>
                <button type="button" class="btn btn-outline-info btn-sm" onclick="printMovement('${movement.id}')" title="Imprimir">
                    <i class="fas fa-print"></i>
                </button>
            </div>
        </td>
    `;
    
    return row;
}

// Función para filtrar datos de movimientos
function filterMovementsData() {
    return movementsData.filter(movement => {
        // Filtro por tipo
        if (currentMovementFilters.type && movement.type !== currentMovementFilters.type) {
            return false;
        }
        
        // Filtro por producto
        if (currentMovementFilters.productId && movement.productId !== currentMovementFilters.productId) {
            return false;
        }
        
        // Filtro por sucursal
        if (currentMovementFilters.branchId && movement.branchId !== currentMovementFilters.branchId) {
            return false;
        }
        
        // Filtro por fecha
        if (currentMovementFilters.startDate || currentMovementFilters.endDate) {
            const movementDate = new Date(movement.createdAt);
            const startDate = currentMovementFilters.startDate ? new Date(currentMovementFilters.startDate) : null;
            const endDate = currentMovementFilters.endDate ? new Date(currentMovementFilters.endDate) : null;
            
            if (startDate && movementDate < startDate) return false;
            if (endDate && movementDate > endDate) return false;
        }
        
        return true;
    });
}

// Función para obtener la clase CSS del tipo de movimiento
function getMovementTypeClass(type) {
    return `movement-type-${type.toLowerCase()}`;
}

// Función para obtener el texto del tipo de movimiento
function getMovementTypeText(type) {
    const typeTexts = {
        'Purchase': 'Compra',
        'Sale': 'Venta',
        'Adjustment': 'Ajuste',
        'Transfer': 'Transferencia',
        'Waste': 'Pérdida',
        'Initial': 'Inicial',
        'Correction': 'Corrección'
    };
    
    return typeTexts[type] || type;
}

// Función para configurar filtros de movimientos
function setupMovementFilters() {
    // Configurar eventos de filtros
    document.getElementById('movementTypeFilter')?.addEventListener('change', function() {
        currentMovementFilters.type = this.value;
    });
    
    document.getElementById('productFilter')?.addEventListener('change', function() {
        currentMovementFilters.productId = this.value;
    });
    
    document.getElementById('branchFilter')?.addEventListener('change', function() {
        currentMovementFilters.branchId = this.value;
    });
    
    document.getElementById('startDateFilter')?.addEventListener('change', function() {
        currentMovementFilters.startDate = this.value;
    });
    
    document.getElementById('endDateFilter')?.addEventListener('change', function() {
        currentMovementFilters.endDate = this.value;
    });
}

// Función para aplicar filtros
function applyFilters() {
    renderMovementsTable();
    updateMovementStats();
}

// Función para limpiar filtros
function clearFilters() {
    currentMovementFilters = {
        type: '',
        productId: '',
        branchId: '',
        startDate: '',
        endDate: ''
    };
    
    // Limpiar campos de filtro
    document.getElementById('movementTypeFilter').value = '';
    document.getElementById('productFilter').value = '';
    document.getElementById('branchFilter').value = '';
    document.getElementById('startDateFilter').value = '';
    document.getElementById('endDateFilter').value = '';
    
    renderMovementsTable();
    updateMovementStats();
}

// Función para actualizar estadísticas
function updateMovementStats() {
    const filteredData = filterMovementsData();
    
    const totalMovements = filteredData.length;
    const totalPurchases = filteredData.filter(m => m.type === 'Purchase').length;
    const totalSales = filteredData.filter(m => m.type === 'Sale').length;
    const totalAdjustments = filteredData.filter(m => m.type === 'Adjustment').length;
    
    document.getElementById('totalMovements').textContent = totalMovements;
    document.getElementById('totalPurchases').textContent = totalPurchases;
    document.getElementById('totalSales').textContent = totalSales;
    document.getElementById('totalAdjustments').textContent = totalAdjustments;
}

// Función para abrir modal de crear movimiento
function openCreateMovementModal() {
    const modal = new bootstrap.Modal(document.getElementById('createMovementModal'));
    modal.show();
}

// Función para configurar el modal de movimientos
function setupMovementModal() {
    const movementTypeSelect = document.getElementById('movementType');
    const unitCostGroup = document.getElementById('unitCostGroup');
    
    if (movementTypeSelect) {
        movementTypeSelect.addEventListener('change', function() {
            if (this.value === 'Purchase') {
                unitCostGroup.style.display = 'block';
            } else {
                unitCostGroup.style.display = 'none';
            }
        });
    }
    
    // Configurar evento de guardar movimiento
    document.getElementById('saveMovement')?.addEventListener('click', saveMovement);
}

// Función para guardar movimiento
async function saveMovement() {
    try {
        const form = document.getElementById('createMovementForm');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const movementType = document.getElementById('movementType').value;
        const productId = document.getElementById('movementProduct').value;
        const branchId = document.getElementById('movementBranch').value;
        const quantity = parseFloat(document.getElementById('movementQuantity').value);
        const unitCost = parseFloat(document.getElementById('movementUnitCost').value) || 0;
        const reason = document.getElementById('movementReason').value;
        
        // Obtener el inventario correspondiente
        const inventoryId = await getInventoryId(productId, branchId);
        if (!inventoryId) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'No se encontró el inventario para el producto y sucursal seleccionados'
            });
            return;
        }
        
        let endpoint = '';
        let requestData = {};
        
        switch (movementType) {
            case 'Purchase':
                endpoint = '/InventoryMovement/CreatePurchase';
                requestData = {
                    inventoryId: inventoryId,
                    productId: productId,
                    branchId: branchId,
                    quantity: quantity,
                    unitCost: unitCost,
                    reason: reason
                };
                break;
                
            case 'Adjustment':
                endpoint = '/InventoryMovement/CreateAdjustment';
                requestData = {
                    inventoryId: inventoryId,
                    productId: productId,
                    branchId: branchId,
                    quantity: quantity,
                    reason: reason
                };
                break;
                
            case 'Waste':
                endpoint = '/InventoryMovement/CreateWaste';
                requestData = {
                    inventoryId: inventoryId,
                    productId: productId,
                    branchId: branchId,
                    quantity: quantity,
                    reason: reason
                };
                break;
                
            default:
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Tipo de movimiento no válido'
                });
                return;
        }
        
        const response = await fetch(endpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(requestData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Éxito',
                text: result.message
            });
            
            // Cerrar modal y recargar datos
            bootstrap.Modal.getInstance(document.getElementById('createMovementModal')).hide();
            form.reset();
            loadMovementsData();
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message
            });
        }
        
    } catch (error) {
        
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error al guardar el movimiento'
        });
    }
}

// Función para obtener el ID del inventario
async function getInventoryId(productId, branchId) {
    try {
        const response = await fetch(`/Inventory/GetByBranchAndProduct?productId=${productId}&branchId=${branchId}`);
        const data = await response.json();
        
        if (data.success && data.inventory) {
            return data.inventory.id;
        }
        
        return null;
    } catch (error) {
        
        return null;
    }
}

// Función para ver detalles de movimiento
function viewMovementDetails(movementId) {
    // Implementar vista de detalles
    
}

// Función para imprimir movimiento
function printMovement(movementId) {
    // Implementar impresión
    
}

// Función para exportar movimientos
function exportMovements() {
    const filteredData = filterMovementsData();
    
    // Crear CSV
    const headers = ['Fecha', 'Tipo', 'Producto', 'Sucursal', 'Cantidad', 'Stock Anterior', 'Stock Nuevo', 'Usuario', 'Motivo', 'Referencia'];
    const csvContent = [
        headers.join(','),
        ...filteredData.map(movement => [
            formatDateTime(movement.createdAt),
            getMovementTypeText(movement.type),
            movement.productName || 'N/A',
            movement.branchName || 'N/A',
            movement.quantity,
            movement.previousStock || 0,
            movement.newStock || 0,
            movement.userName || 'Sistema',
            movement.reason || 'N/A',
            movement.reference || 'N/A'
        ].join(','))
    ].join('\n');
    
    // Descargar archivo
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `movimientos_inventario_${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Función para formatear fecha y hora
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('es-ES', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
} 