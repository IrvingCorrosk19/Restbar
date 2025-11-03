// ProductStockAssignment Management
console.log('🔍 [ProductStockAssignment] product-stock-assignment.js cargado');

let assignments = [];
let products = [];
let stations = [];

// Cargar asignaciones al inicio
document.addEventListener('DOMContentLoaded', function() {
    console.log('🔍 [ProductStockAssignment] DOMContentLoaded - Iniciando carga...');
    loadAssignments();
    loadProducts();
    loadStations();
    
    // Event listeners para filtros
    document.getElementById('productSearchInput')?.addEventListener('input', filterAssignments);
    document.getElementById('stationFilter')?.addEventListener('change', filterAssignments);
    document.getElementById('statusFilter')?.addEventListener('change', filterAssignments);
});

// Cargar asignaciones
async function loadAssignments() {
    try {
        console.log('🔍 [ProductStockAssignment] loadAssignments() - Iniciando...');
        
        const response = await fetch('/ProductStockAssignment/GetAssignments');
        const result = await response.json();
        
        console.log('📡 [ProductStockAssignment] loadAssignments() - Respuesta recibida:', result);
        
        if (result.success) {
            const dataArray = result.data.$values || result.data;
            assignments = dataArray || [];
            console.log(`✅ [ProductStockAssignment] loadAssignments() - Total asignaciones: ${assignments.length}`);
            renderAssignments();
        } else {
            console.warn('⚠️ [ProductStockAssignment] loadAssignments() - No hay datos o hay error:', result.message);
            showError(result.message || 'Error al cargar asignaciones');
        }
    } catch (error) {
        console.error('❌ [ProductStockAssignment] loadAssignments() - Error:', error);
        showError('Error al cargar asignaciones: ' + error.message);
    }
}

// Cargar productos
async function loadProducts() {
    try {
        console.log('🔍 [ProductStockAssignment] loadProducts() - Iniciando...');
        
        const response = await fetch('/Product/GetProducts');
        const result = await response.json();
        
        if (result.success) {
            const dataArray = result.data.$values || result.data;
            products = dataArray || [];
            
            const createSelect = document.getElementById('createProductId');
            if (createSelect) {
                createSelect.innerHTML = '<option value="">Seleccionar producto...</option>';
                products.forEach(product => {
                    const option = document.createElement('option');
                    option.value = product.id;
                    option.textContent = product.name;
                    createSelect.appendChild(option);
                });
            }
            
            console.log(`✅ [ProductStockAssignment] loadProducts() - Total productos: ${products.length}`);
        }
    } catch (error) {
        console.error('❌ [ProductStockAssignment] loadProducts() - Error:', error);
    }
}

// Cargar estaciones
async function loadStations() {
    try {
        console.log('🔍 [ProductStockAssignment] loadStations() - Iniciando...');
        
        const response = await fetch('/Station/GetStations');
        const result = await response.json();
        
        if (result.success) {
            const dataArray = result.data.$values || result.data;
            stations = dataArray || [];
            
            const createSelect = document.getElementById('createStationId');
            const filterSelect = document.getElementById('stationFilter');
            
            if (createSelect) {
                createSelect.innerHTML = '<option value="">Seleccionar estación...</option>';
                stations.forEach(station => {
                    const option = document.createElement('option');
                    option.value = station.id;
                    option.textContent = `${station.name} (${station.type})`;
                    createSelect.appendChild(option);
                });
            }
            
            if (filterSelect) {
                filterSelect.innerHTML = '<option value="">Todas las estaciones</option>';
                stations.forEach(station => {
                    const option = document.createElement('option');
                    option.value = station.id;
                    option.textContent = `${station.name} (${station.type})`;
                    filterSelect.appendChild(option);
                });
            }
            
            console.log(`✅ [ProductStockAssignment] loadStations() - Total estaciones: ${stations.length}`);
        }
    } catch (error) {
        console.error('❌ [ProductStockAssignment] loadStations() - Error:', error);
    }
}

// Renderizar asignaciones
function renderAssignments(filteredAssignments = null) {
    const tbody = document.getElementById('assignmentsTableBody');
    if (!tbody) return;
    
    const dataToRender = filteredAssignments || assignments;
    tbody.innerHTML = '';
    
    if (dataToRender.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center">No hay asignaciones registradas</td></tr>';
        return;
    }
    
    dataToRender.forEach(assignment => {
        const row = document.createElement('tr');
        
        const stockClass = getStockClass(assignment.stock, assignment.minStock);
        const stockBadge = `<span class="stock-badge ${stockClass}">${formatStock(assignment.stock)}</span>`;
        
        row.innerHTML = `
            <td>${assignment.productName || 'N/A'}</td>
            <td>${assignment.stationName || 'N/A'}</td>
            <td>${stockBadge}</td>
            <td>${assignment.minStock != null ? assignment.minStock : '-'}</td>
            <td><span class="badge bg-secondary">${assignment.priority || 0}</span></td>
            <td>
                <span class="badge ${assignment.isActive ? 'bg-success' : 'bg-danger'}">
                    ${assignment.isActive ? 'Activa' : 'Inactiva'}
                </span>
            </td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="editAssignment('${assignment.id}')">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteAssignment('${assignment.id}')">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;
        
        tbody.appendChild(row);
    });
    
    console.log(`✅ [ProductStockAssignment] renderAssignments() - Renderizadas ${dataToRender.length} asignaciones`);
}

// Filtrar asignaciones
function filterAssignments() {
    const productSearch = document.getElementById('productSearchInput')?.value.toLowerCase() || '';
    const stationFilter = document.getElementById('stationFilter')?.value || '';
    const statusFilter = document.getElementById('statusFilter')?.value || '';
    
    let filtered = [...assignments];
    
    if (productSearch) {
        filtered = filtered.filter(a => 
            (a.productName || '').toLowerCase().includes(productSearch)
        );
    }
    
    if (stationFilter) {
        filtered = filtered.filter(a => a.stationId === stationFilter);
    }
    
    if (statusFilter) {
        filtered = filtered.filter(a => a.isActive.toString() === statusFilter);
    }
    
    renderAssignments(filtered);
}

// Obtener clase de stock
function getStockClass(stock, minStock) {
    if (stock == null || stock < 0) return 'stock-unlimited';
    if (minStock != null && stock <= minStock) return 'stock-low';
    if (stock < minStock * 1.5) return 'stock-medium';
    return 'stock-high';
}

// Formatear stock
function formatStock(stock) {
    if (stock == null) return 'Ilimitado';
    return stock.toFixed(2);
}

// Crear asignación
async function createAssignment() {
    try {
        console.log('🔍 [ProductStockAssignment] createAssignment() - Iniciando...');
        
        const productId = document.getElementById('createProductId').value;
        const stationId = document.getElementById('createStationId').value;
        const stock = parseFloat(document.getElementById('createStock').value);
        const minStock = document.getElementById('createMinStock').value ? parseFloat(document.getElementById('createMinStock').value) : null;
        const priority = parseInt(document.getElementById('createPriority').value) || 0;
        const isActive = document.getElementById('createIsActive').value === 'true';
        
        if (!productId || !stationId || isNaN(stock)) {
            showError('Por favor, complete todos los campos requeridos');
            return;
        }
        
        const assignment = {
            productId: productId,
            stationId: stationId,
            stock: stock,
            minStock: minStock,
            priority: priority,
            isActive: isActive
        };
        
        console.log('📤 [ProductStockAssignment] createAssignment() - Enviando:', assignment);
        
        const response = await fetch('/ProductStockAssignment/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(assignment)
        });
        
        const result = await response.json();
        console.log('📡 [ProductStockAssignment] createAssignment() - Respuesta:', result);
        
        if (result.success) {
            $('#createAssignmentModal').modal('hide');
            showSuccess('Asignación creada exitosamente');
            document.getElementById('createAssignmentForm').reset();
            loadAssignments();
        } else {
            showError(result.message || 'Error al crear asignación');
        }
    } catch (error) {
        console.error('❌ [ProductStockAssignment] createAssignment() - Error:', error);
        showError('Error al crear asignación: ' + error.message);
    }
}

// Editar asignación
async function editAssignment(id) {
    try {
        console.log(`🔍 [ProductStockAssignment] editAssignment() - ID: ${id}`);
        
        const assignment = assignments.find(a => a.id === id);
        if (!assignment) {
            showError('Asignación no encontrada');
            return;
        }
        
        document.getElementById('editAssignmentId').value = assignment.id;
        document.getElementById('editProductId').value = assignment.productId;
        document.getElementById('editProductName').value = assignment.productName || 'N/A';
        document.getElementById('editStationId').value = assignment.stationId;
        document.getElementById('editStationName').value = assignment.stationName || 'N/A';
        document.getElementById('editStock').value = assignment.stock || 0;
        document.getElementById('editMinStock').value = assignment.minStock || '';
        document.getElementById('editPriority').value = assignment.priority || 0;
        document.getElementById('editIsActive').value = assignment.isActive ? 'true' : 'false';
        
        $('#editAssignmentModal').modal('show');
    } catch (error) {
        console.error('❌ [ProductStockAssignment] editAssignment() - Error:', error);
        showError('Error al cargar asignación: ' + error.message);
    }
}

// Actualizar asignación
async function updateAssignment() {
    try {
        console.log('🔍 [ProductStockAssignment] updateAssignment() - Iniciando...');
        
        const id = document.getElementById('editAssignmentId').value;
        const productId = document.getElementById('editProductId').value;
        const stationId = document.getElementById('editStationId').value;
        const stock = parseFloat(document.getElementById('editStock').value);
        const minStock = document.getElementById('editMinStock').value ? parseFloat(document.getElementById('editMinStock').value) : null;
        const priority = parseInt(document.getElementById('editPriority').value) || 0;
        const isActive = document.getElementById('editIsActive').value === 'true';
        
        if (!id || !productId || !stationId || isNaN(stock)) {
            showError('Por favor, complete todos los campos requeridos');
            return;
        }
        
        const assignment = {
            productId: productId,
            stationId: stationId,
            stock: stock,
            minStock: minStock,
            priority: priority,
            isActive: isActive
        };
        
        console.log('📤 [ProductStockAssignment] updateAssignment() - Enviando:', assignment);
        
        const response = await fetch(`/ProductStockAssignment/Update/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(assignment)
        });
        
        const result = await response.json();
        console.log('📡 [ProductStockAssignment] updateAssignment() - Respuesta:', result);
        
        if (result.success) {
            $('#editAssignmentModal').modal('hide');
            showSuccess('Asignación actualizada exitosamente');
            loadAssignments();
        } else {
            showError(result.message || 'Error al actualizar asignación');
        }
    } catch (error) {
        console.error('❌ [ProductStockAssignment] updateAssignment() - Error:', error);
        showError('Error al actualizar asignación: ' + error.message);
    }
}

// Eliminar asignación
async function deleteAssignment(id) {
    try {
        console.log(`🔍 [ProductStockAssignment] deleteAssignment() - ID: ${id}`);
        
        if (!confirm('¿Estás seguro de eliminar esta asignación?')) {
            return;
        }
        
        const response = await fetch(`/ProductStockAssignment/Delete/${id}`, {
            method: 'DELETE'
        });
        
        const result = await response.json();
        console.log('📡 [ProductStockAssignment] deleteAssignment() - Respuesta:', result);
        
        if (result.success) {
            showSuccess('Asignación eliminada exitosamente');
            loadAssignments();
        } else {
            showError(result.message || 'Error al eliminar asignación');
        }
    } catch (error) {
        console.error('❌ [ProductStockAssignment] deleteAssignment() - Error:', error);
        showError('Error al eliminar asignación: ' + error.message);
    }
}

// Funciones de utilidad
function showSuccess(message) {
    Swal.fire({
        icon: 'success',
        title: 'Éxito',
        text: message,
        timer: 2000,
        showConfirmButton: false
    });
}

function showError(message) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message
    });
}

