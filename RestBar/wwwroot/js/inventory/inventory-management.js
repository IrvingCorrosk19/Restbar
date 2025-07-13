// Variables globales para inventario
let inventoryData = [];
let currentFilters = {
    branch: '',
    category: '',
    stockStatus: '',
    search: ''
};

// Variables globales para datos de modales
let productsData = [];
let categoriesData = [];
let branchesData = [];

// Función para cargar datos de inventario
async function loadInventoryData() {
    try {
        
        
        const response = await fetch('/Inventory/GetInventoryData');
        if (!response.ok) {
            throw new Error('Error al cargar datos de inventario');
        }
        
        const data = await response.json();
        
        // Verificar si la respuesta es exitosa
        if (!data.success) {
            throw new Error(data.message || 'Error en la respuesta del servidor');
        }
        
        // Asegurar que inventoryData sea siempre un array
        inventoryData = Array.isArray(data.inventory) ? data.inventory : [];
        
        
        
        renderInventoryTable();
        updatePagination();
        
    } catch (error) {
        
        // Inicializar como array vacío en caso de error
        inventoryData = [];
        renderInventoryTable();
        
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudieron cargar los datos de inventario'
        });
    }
}

// Función para cargar datos para los modales
async function loadModalData() {
    try {
        
        
        // Cargar productos
        const productsResponse = await fetch('/Inventory/GetProducts');
        if (productsResponse.ok) {
            const productsData = await productsResponse.json();
            window.productsData = productsData.products || [];
        }
        
        // Cargar categorías
        const categoriesResponse = await fetch('/Inventory/GetCategories');
        if (categoriesResponse.ok) {
            const categoriesData = await categoriesResponse.json();
            window.categoriesData = categoriesData.categories || [];
        }
        
        // Cargar sucursales
        const branchesResponse = await fetch('/Inventory/GetBranches');
        if (branchesResponse.ok) {
            const branchesData = await branchesResponse.json();
            window.branchesData = branchesData.branches || [];
        }
        
        
        
    } catch (error) {
        
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
                        onclick="openUpdateStockModal('${item.productId}', '${item.branchId}', '${item.productName}', '${item.branchName}', ${item.quantity})"
                        title="Actualizar Stock">
                    <i class="fas fa-edit"></i>
                </button>
                <button type="button" class="btn btn-outline-info" 
                        onclick="viewStockHistory('${item.productId}', '${item.branchId}')"
                        title="Historial de Stock">
                    <i class="fas fa-history"></i>
                </button>
                <button type="button" class="btn btn-outline-success" 
                        onclick="viewMovementHistory('${item.productId}', '${item.branchId}')"
                        title="Historial de Movimientos">
                    <i class="fas fa-exchange-alt"></i>
                </button>
            </div>
        </td>
    `;
    
    return row;
}

// Función para filtrar datos de inventario
function filterInventoryData() {
    // Asegurar que inventoryData sea un array
    if (!Array.isArray(inventoryData)) {
        
        return [];
    }
    
    return inventoryData.filter(item => {
        // Verificar que el item sea válido
        if (!item || typeof item !== 'object') {
            return false;
        }
        
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
            const productName = item.productName || '';
            if (!productName.toLowerCase().includes(searchTerm)) {
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
        
        if (result.success && result.items && result.items.length > 0) {
            const alertsDiv = document.getElementById('lowStockAlerts');
            const itemsList = document.getElementById('lowStockItemsList');
            
            if (alertsDiv && itemsList) {
                const itemsHtml = result.items.map(item => `
                    <div class="mb-2">
                        <strong>${item.productName || 'Producto'}</strong> - 
                        <span class="badge bg-warning">${item.currentStock || 0} unidades</span>
                        <small class="text-muted">(${item.branchName || 'Sucursal'})</small>
                    </div>
                `).join('');
                
                itemsList.innerHTML = itemsHtml;
                alertsDiv.style.display = 'block';
            }
        } else {
            // Ocultar alertas si no hay items
            const alertsDiv = document.getElementById('lowStockAlerts');
            if (alertsDiv) {
                alertsDiv.style.display = 'none';
            }
        }
        
    } catch (error) {
        
        // Ocultar alertas en caso de error
        const alertsDiv = document.getElementById('lowStockAlerts');
        if (alertsDiv) {
            alertsDiv.style.display = 'none';
        }
    }
}

// Función para actualizar paginación
function updatePagination() {
    // Implementar paginación si es necesario
    
}

// Funciones para abrir modales de creación
function openCreateProductModal() {
    // Limpiar formulario
    document.getElementById('createProductForm').reset();
    
    // Cargar categorías en el select
    const categorySelect = document.getElementById('productCategory');
    categorySelect.innerHTML = '<option value="">Seleccionar categoría</option>';
    
    if (window.categoriesData) {
        window.categoriesData.forEach(category => {
            const option = document.createElement('option');
            option.value = category.id;
            option.textContent = category.name;
            categorySelect.appendChild(option);
        });
    }
    
    const modal = new bootstrap.Modal(document.getElementById('createProductModal'));
    modal.show();
}

function openCreateCategoryModal() {
    // Limpiar formulario
    document.getElementById('createCategoryForm').reset();
    
    const modal = new bootstrap.Modal(document.getElementById('createCategoryModal'));
    modal.show();
}

function openCreateBranchModal() {
    // Limpiar formulario
    document.getElementById('createBranchForm').reset();
    
    const modal = new bootstrap.Modal(document.getElementById('createBranchModal'));
    modal.show();
}

function openMovementModal() {
    // Limpiar formulario
    document.getElementById('movementForm').reset();
    
    // Cargar productos en el select
    const productSelect = document.getElementById('movementProduct');
    productSelect.innerHTML = '<option value="">Seleccionar producto</option>';
    
    if (window.productsData) {
        window.productsData.forEach(product => {
            const option = document.createElement('option');
            option.value = product.id;
            option.textContent = product.name;
            productSelect.appendChild(option);
        });
    }
    
    // Cargar sucursales en el select
    const branchSelect = document.getElementById('movementBranch');
    branchSelect.innerHTML = '<option value="">Seleccionar sucursal</option>';
    
    if (window.branchesData) {
        window.branchesData.forEach(branch => {
            const option = document.createElement('option');
            option.value = branch.id;
            option.textContent = branch.name;
            branchSelect.appendChild(option);
        });
    }
    
    const modal = new bootstrap.Modal(document.getElementById('movementModal'));
    modal.show();
}

// Funciones para guardar datos de los modales
async function saveProduct() {
    const formData = {
        name: document.getElementById('productName').value,
        description: document.getElementById('productDescription').value,
        categoryId: document.getElementById('productCategory').value,
        price: parseFloat(document.getElementById('productPrice').value),
        unitCost: parseFloat(document.getElementById('productUnitCost').value)
    };
    
    if (!formData.name || !formData.categoryId || isNaN(formData.price) || isNaN(formData.unitCost)) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Por favor complete todos los campos requeridos'
        });
        return;
    }
    
    try {
        const response = await fetch('/Inventory/CreateProduct', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Éxito',
                text: 'Producto creado correctamente'
            });
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('createProductModal'));
            modal.hide();
            
            // Recargar datos
            await loadModalData();
            await loadInventoryData();
            
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message || 'Error al crear producto'
            });
        }
        
    } catch (error) {
        
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error de conexión al crear producto'
        });
    }
}

async function saveCategory() {
    const formData = {
        name: document.getElementById('categoryName').value,
        description: document.getElementById('categoryDescription').value
    };
    
    if (!formData.name) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Por favor ingrese el nombre de la categoría'
        });
        return;
    }
    
    try {
        const response = await fetch('/Inventory/CreateCategory', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Éxito',
                text: 'Categoría creada correctamente'
            });
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('createCategoryModal'));
            modal.hide();
            
            // Recargar datos
            await loadModalData();
            
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message || 'Error al crear categoría'
            });
        }
        
    } catch (error) {
        
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error de conexión al crear categoría'
        });
    }
}

async function saveBranch() {
    const formData = {
        name: document.getElementById('branchName').value,
        address: document.getElementById('branchAddress').value,
        phone: document.getElementById('branchPhone').value
    };
    
    if (!formData.name) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Por favor ingrese el nombre de la sucursal'
        });
        return;
    }
    
    try {
        const response = await fetch('/Inventory/CreateBranch', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Éxito',
                text: 'Sucursal creada correctamente'
            });
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('createBranchModal'));
            modal.hide();
            
            // Recargar datos
            await loadModalData();
            
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message || 'Error al crear sucursal'
            });
        }
        
    } catch (error) {
        
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error de conexión al crear sucursal'
        });
    }
}

async function saveMovement() {
    const formData = {
        productId: document.getElementById('movementProduct').value,
        branchId: document.getElementById('movementBranch').value,
        movementType: document.getElementById('movementType').value,
        quantity: parseFloat(document.getElementById('movementQuantity').value),
        unitCost: parseFloat(document.getElementById('movementUnitCost').value) || 0,
        reason: document.getElementById('movementReason').value,
        reference: document.getElementById('movementReference').value
    };
    
    // Validar campos requeridos
    const validationErrors = validateMovement(formData);
    if (validationErrors.length > 0) {
        Swal.fire({
            icon: 'error',
            title: 'Error de Validación',
            html: validationErrors.map(error => `<li>${error}</li>`).join(''),
            confirmButtonText: 'Entendido'
        });
        return;
    }
    
    // Mostrar loading
    Swal.fire({
        title: 'Registrando Movimiento',
        text: 'Por favor espere...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
    
    try {
        const response = await fetch('/Inventory/CreateMovement', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: '¡Movimiento Registrado!',
                text: 'El movimiento se ha registrado correctamente en el sistema',
                confirmButtonText: 'Entendido'
            });
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('movementModal'));
            if (modal) {
                modal.hide();
            }
            
            // Limpiar formulario
            document.getElementById('movementForm').reset();
            
            // Recargar datos
            await loadInventoryData();
            
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error al Registrar',
                text: result.message || 'Error al registrar movimiento en el sistema',
                confirmButtonText: 'Entendido'
            });
        }
        
    } catch (error) {
        
        Swal.fire({
            icon: 'error',
            title: 'Error de Conexión',
            text: 'No se pudo conectar con el servidor. Verifique su conexión e intente nuevamente.',
            confirmButtonText: 'Entendido'
        });
    }
}

// Función para ver historial de movimientos
async function viewMovementHistory(productId, branchId) {
    try {
        const response = await fetch(`/Inventory/GetMovementHistory?productId=${productId}&branchId=${branchId}`);
        const result = await response.json();
        
        if (result.success) {
            displayMovementHistory(result.movements);
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message || 'Error al cargar historial de movimientos'
            });
        }
    } catch (error) {
        
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error de conexión al cargar historial'
        });
    }
}

// Función para mostrar historial de movimientos
function displayMovementHistory(movements) {
    if (!movements || movements.length === 0) {
        Swal.fire({
            icon: 'info',
            title: 'Sin Movimientos',
            text: 'No hay movimientos registrados para este producto en esta sucursal'
        });
        return;
    }
    
    let historyHtml = `
        <div class="table-responsive">
            <table class="table table-striped table-sm">
                <thead class="table-dark">
                    <tr>
                        <th>Fecha</th>
                        <th>Tipo</th>
                        <th>Cantidad</th>
                        <th>Stock Anterior</th>
                        <th>Stock Nuevo</th>
                        <th>Motivo</th>
                        <th>Usuario</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    movements.forEach(movement => {
        const quantityClass = movement.quantity > 0 ? 'text-success' : 'text-danger';
        const quantitySign = movement.quantity > 0 ? '+' : '';
        
        historyHtml += `
            <tr>
                <td>${movement.createdAt}</td>
                <td><span class="badge bg-primary">${movement.type}</span></td>
                <td class="${quantityClass}">${quantitySign}${movement.quantity}</td>
                <td>${movement.previousStock || 'N/A'}</td>
                <td>${movement.newStock || 'N/A'}</td>
                <td>${movement.reason || 'N/A'}</td>
                <td>${movement.userName}</td>
            </tr>
        `;
    });
    
    historyHtml += `
                </tbody>
            </table>
        </div>
    `;
    
    Swal.fire({
        title: 'Historial de Movimientos',
        html: historyHtml,
        width: '800px',
        confirmButtonText: 'Cerrar',
        confirmButtonColor: '#3085d6'
    });
}

// Función para obtener reporte de movimientos por tipo
async function getMovementsByType(movementType) {
    try {
        const response = await fetch(`/Inventory/GetMovementsByType?movementType=${movementType}`);
        const result = await response.json();
        
        if (result.success) {
            displayMovementsReport(result.movements, movementType);
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: result.message || 'Error al obtener movimientos'
            });
        }
    } catch (error) {
        
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error de conexión al obtener movimientos'
        });
    }
}

// Función para mostrar reporte de movimientos
function displayMovementsReport(movements, movementType) {
    if (!movements || movements.length === 0) {
        Swal.fire({
            icon: 'info',
            title: 'Sin Movimientos',
            text: `No hay movimientos de tipo "${movementType}" registrados`
        });
        return;
    }
    
    let reportHtml = `
        <div class="table-responsive">
            <table class="table table-striped table-sm">
                <thead class="table-dark">
                    <tr>
                        <th>Producto</th>
                        <th>Sucursal</th>
                        <th>Cantidad</th>
                        <th>Motivo</th>
                        <th>Referencia</th>
                        <th>Fecha</th>
                        <th>Usuario</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    movements.forEach(movement => {
        const quantityClass = movement.quantity > 0 ? 'text-success' : 'text-danger';
        const quantitySign = movement.quantity > 0 ? '+' : '';
        
        reportHtml += `
            <tr>
                <td>${movement.productName}</td>
                <td>${movement.branchName}</td>
                <td class="${quantityClass}">${quantitySign}${movement.quantity}</td>
                <td>${movement.reason || 'N/A'}</td>
                <td>${movement.reference || 'N/A'}</td>
                <td>${movement.createdAt}</td>
                <td>${movement.userName}</td>
            </tr>
        `;
    });
    
    reportHtml += `
                </tbody>
            </table>
        </div>
    `;
    
    Swal.fire({
        title: `Reporte de Movimientos - ${movementType}`,
        html: reportHtml,
        width: '900px',
        confirmButtonText: 'Cerrar',
        confirmButtonColor: '#3085d6'
    });
}

// Función para validar movimiento antes de guardar
function validateMovement(formData) {
    const errors = [];
    
    if (!formData.productId) {
        errors.push('Debe seleccionar un producto');
    }
    
    if (!formData.branchId) {
        errors.push('Debe seleccionar una sucursal');
    }
    
    if (!formData.movementType) {
        errors.push('Debe seleccionar un tipo de movimiento');
    }
    
    if (isNaN(formData.quantity) || formData.quantity <= 0) {
        errors.push('La cantidad debe ser un número mayor a 0');
    }
    
    // Validaciones específicas por tipo de movimiento
    if (formData.movementType === 'purchase' && formData.unitCost <= 0) {
        errors.push('Para compras debe especificar el costo unitario');
    }
    
    if (formData.movementType === 'sale' && formData.quantity > 0) {
        errors.push('Las ventas deben tener cantidad negativa');
    }
    
    return errors;
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
    
    // Configurar botones de guardar para los nuevos modales
    const saveProductBtn = document.getElementById('saveProduct');
    if (saveProductBtn) {
        saveProductBtn.addEventListener('click', saveProduct);
    }
    
    const saveCategoryBtn = document.getElementById('saveCategory');
    if (saveCategoryBtn) {
        saveCategoryBtn.addEventListener('click', saveCategory);
    }
    
    const saveBranchBtn = document.getElementById('saveBranch');
    if (saveBranchBtn) {
        saveBranchBtn.addEventListener('click', saveBranch);
    }
    
    const saveMovementBtn = document.getElementById('saveMovement');
    if (saveMovementBtn) {
        saveMovementBtn.addEventListener('click', saveMovement);
    }
});

// Función para verificar datos de la base de datos
async function checkDatabaseData() {
    try {
        
        
        const response = await fetch('/Inventory/CheckDatabaseData');
        if (!response.ok) {
            throw new Error('Error al verificar datos');
        }
        
        const result = await response.json();
        
        if (result.success) {
            const data = result.data;
            
            
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
window.openCreateProductModal = openCreateProductModal;
window.openCreateCategoryModal = openCreateCategoryModal;
window.openCreateBranchModal = openCreateBranchModal;
window.openMovementModal = openMovementModal;
window.saveProduct = saveProduct;
window.saveCategory = saveCategory;
window.saveBranch = saveBranch;
window.saveMovement = saveMovement;
window.loadModalData = loadModalData;
window.viewMovementHistory = viewMovementHistory;
window.getMovementsByType = getMovementsByType;
window.displayMovementHistory = displayMovementHistory;
window.displayMovementsReport = displayMovementsReport;
window.validateMovement = validateMovement; 