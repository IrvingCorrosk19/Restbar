// Variables globales
let suppliersData = [];
let currentPage = 1;
let itemsPerPage = 10;
let currentSupplierId = null;

// Función para cargar proveedores
async function loadSuppliers() {
    try {
        
        const response = await fetch('/Supplier/GetSuppliers');
        
        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                suppliersData = data.suppliers || [];
                displaySuppliers();
                updateSuppliersCount();
                loadCityFilter();
            } else {
                
                showAlert('Error al cargar proveedores', 'error');
            }
        } else {
            
            showAlert('Error al cargar proveedores', 'error');
        }
    } catch (error) {
        
        showAlert('Error al cargar proveedores', 'error');
    }
}

// Función para mostrar proveedores en la tabla
function displaySuppliers() {
    const tbody = document.getElementById('suppliersTableBody');
    if (!tbody) return;

    // Aplicar filtros
    let filteredSuppliers = applyFilters(suppliersData);
    
    // Aplicar paginación
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    const paginatedSuppliers = filteredSuppliers.slice(startIndex, endIndex);

    tbody.innerHTML = '';

    if (paginatedSuppliers.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted py-4">
                    <i class="fas fa-inbox fa-2x mb-2"></i>
                    <p>No se encontraron proveedores</p>
                </td>
            </tr>
        `;
        return;
    }

    paginatedSuppliers.forEach(supplier => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>
                <div class="d-flex align-items-center">
                    <div class="avatar-sm bg-primary rounded-circle d-flex align-items-center justify-content-center me-2">
                        <i class="fas fa-truck text-white"></i>
                    </div>
                    <div>
                        <strong>${supplier.name || 'Sin nombre'}</strong>
                        ${supplier.description ? `<br><small class="text-muted">${supplier.description}</small>` : ''}
                    </div>
                </div>
            </td>
            <td>${supplier.contactPerson || 'No especificado'}</td>
            <td>
                ${supplier.email ? `<a href="mailto:${supplier.email}" class="text-decoration-none">${supplier.email}</a>` : 'No especificado'}
            </td>
            <td>
                ${supplier.phone ? `<a href="tel:${supplier.phone}" class="text-decoration-none">${supplier.phone}</a>` : 'No especificado'}
            </td>
            <td>${supplier.city || 'No especificado'}</td>
            <td>
                <span class="badge ${supplier.isActive ? 'bg-success' : 'bg-danger'}">
                    ${supplier.isActive ? 'Activo' : 'Inactivo'}
                </span>
            </td>
            <td>
                <span class="badge bg-info">${supplier.products ? supplier.products.length : 0} productos</span>
            </td>
            <td>
                <div class="btn-group btn-group-sm" role="group">
                    <button type="button" class="btn btn-outline-info" onclick="viewSupplierDetails('${supplier.id}')" title="Ver detalles">
                        <i class="fas fa-eye"></i>
                    </button>
                    <button type="button" class="btn btn-outline-primary" onclick="editSupplier('${supplier.id}')" title="Editar">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button type="button" class="btn btn-outline-danger" onclick="deleteSupplier('${supplier.id}')" title="Eliminar">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </td>
        `;
        tbody.appendChild(row);
    });

    updatePagination(filteredSuppliers.length);
}

// Función para aplicar filtros
function applyFilters(suppliers) {
    let filtered = suppliers;

    // Filtro de búsqueda
    const searchTerm = document.getElementById('searchInput')?.value.toLowerCase();
    if (searchTerm) {
        filtered = filtered.filter(supplier => 
            supplier.name?.toLowerCase().includes(searchTerm) ||
            supplier.contactPerson?.toLowerCase().includes(searchTerm) ||
            supplier.email?.toLowerCase().includes(searchTerm) ||
            supplier.phone?.toLowerCase().includes(searchTerm) ||
            supplier.city?.toLowerCase().includes(searchTerm)
        );
    }

    // Filtro de estado
    const statusFilter = document.getElementById('statusFilter')?.value;
    if (statusFilter !== '') {
        const isActive = statusFilter === 'true';
        filtered = filtered.filter(supplier => supplier.isActive === isActive);
    }

    // Filtro de ciudad
    const cityFilter = document.getElementById('cityFilter')?.value;
    if (cityFilter) {
        filtered = filtered.filter(supplier => supplier.city === cityFilter);
    }

    return filtered;
}

// Función para configurar filtros
function setupFilters() {
    const searchInput = document.getElementById('searchInput');
    const statusFilter = document.getElementById('statusFilter');
    const cityFilter = document.getElementById('cityFilter');

    if (searchInput) {
        searchInput.addEventListener('input', () => {
            currentPage = 1;
            displaySuppliers();
        });
    }

    if (statusFilter) {
        statusFilter.addEventListener('change', () => {
            currentPage = 1;
            displaySuppliers();
        });
    }

    if (cityFilter) {
        cityFilter.addEventListener('change', () => {
            currentPage = 1;
            displaySuppliers();
        });
    }
}

// Función para cargar filtro de ciudades
function loadCityFilter() {
    const cityFilter = document.getElementById('cityFilter');
    if (!cityFilter) return;

    const cities = [...new Set(suppliersData.map(s => s.city).filter(city => city))];
    cities.sort();

    cityFilter.innerHTML = '<option value="">Todas las ciudades</option>';
    cities.forEach(city => {
        const option = document.createElement('option');
        option.value = city;
        option.textContent = city;
        cityFilter.appendChild(option);
    });
}

// Función para limpiar filtros
function clearFilters() {
    const searchInput = document.getElementById('searchInput');
    const statusFilter = document.getElementById('statusFilter');
    const cityFilter = document.getElementById('cityFilter');

    if (searchInput) searchInput.value = '';
    if (statusFilter) statusFilter.value = '';
    if (cityFilter) cityFilter.value = '';

    currentPage = 1;
    displaySuppliers();
}

// Función para actualizar contador de proveedores
function updateSuppliersCount() {
    const countElement = document.getElementById('suppliersCount');
    if (countElement) {
        const filteredSuppliers = applyFilters(suppliersData);
        countElement.textContent = filteredSuppliers.length;
    }
}

// Función para actualizar paginación
function updatePagination(totalItems) {
    const pagination = document.getElementById('pagination');
    if (!pagination) return;

    const totalPages = Math.ceil(totalItems / itemsPerPage);
    pagination.innerHTML = '';

    if (totalPages <= 1) return;

    // Botón anterior
    const prevLi = document.createElement('li');
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `
        <button class="page-link" onclick="changePage(${currentPage - 1})" ${currentPage === 1 ? 'disabled' : ''}>
            <i class="fas fa-chevron-left"></i>
        </button>
    `;
    pagination.appendChild(prevLi);

    // Páginas
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
            const li = document.createElement('li');
            li.className = `page-item ${i === currentPage ? 'active' : ''}`;
            li.innerHTML = `
                <button class="page-link" onclick="changePage(${i})">${i}</button>
            `;
            pagination.appendChild(li);
        } else if (i === currentPage - 3 || i === currentPage + 3) {
            const li = document.createElement('li');
            li.className = 'page-item disabled';
            li.innerHTML = '<span class="page-link">...</span>';
            pagination.appendChild(li);
        }
    }

    // Botón siguiente
    const nextLi = document.createElement('li');
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `
        <button class="page-link" onclick="changePage(${currentPage + 1})" ${currentPage === totalPages ? 'disabled' : ''}>
            <i class="fas fa-chevron-right"></i>
        </button>
    `;
    pagination.appendChild(nextLi);
}

// Función para cambiar página
function changePage(page) {
    const totalItems = applyFilters(suppliersData).length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    
    if (page >= 1 && page <= totalPages) {
        currentPage = page;
        displaySuppliers();
    }
}

// Función para abrir modal de crear proveedor
function openCreateSupplierModal() {
    currentSupplierId = null;
    resetSupplierForm();
    document.getElementById('supplierModalLabel').innerHTML = '<i class="fas fa-truck me-2"></i>Nuevo Proveedor';
    const modal = new bootstrap.Modal(document.getElementById('supplierModal'));
    modal.show();
}

// Función para editar proveedor
async function editSupplier(supplierId) {
    try {
        const supplier = suppliersData.find(s => s.id === supplierId);
        if (!supplier) {
            showAlert('Proveedor no encontrado', 'error');
            return;
        }

        currentSupplierId = supplierId;
        fillSupplierForm(supplier);
        document.getElementById('supplierModalLabel').innerHTML = '<i class="fas fa-edit me-2"></i>Editar Proveedor';
        const modal = new bootstrap.Modal(document.getElementById('supplierModal'));
        modal.show();
    } catch (error) {
        
        showAlert('Error al editar proveedor', 'error');
    }
}

// Función para llenar formulario con datos del proveedor
function fillSupplierForm(supplier) {
    document.getElementById('supplierName').value = supplier.name || '';
    document.getElementById('supplierDescription').value = supplier.description || '';
    document.getElementById('supplierContactPerson').value = supplier.contactPerson || '';
    document.getElementById('supplierEmail').value = supplier.email || '';
    document.getElementById('supplierPhone').value = supplier.phone || '';
    document.getElementById('supplierAddress').value = supplier.address || '';
    document.getElementById('supplierCity').value = supplier.city || '';
    document.getElementById('supplierState').value = supplier.state || '';
    document.getElementById('supplierPostalCode').value = supplier.postalCode || '';
    document.getElementById('supplierCountry').value = supplier.country || '';
    document.getElementById('supplierTaxId').value = supplier.taxId || '';
    document.getElementById('supplierWebsite').value = supplier.website || '';
    document.getElementById('supplierNotes').value = supplier.notes || '';
    document.getElementById('supplierIsActive').checked = supplier.isActive !== false;
}

// Función para resetear formulario
function resetSupplierForm() {
    document.getElementById('supplierForm').reset();
    document.getElementById('supplierIsActive').checked = true;
}

// Función para guardar proveedor
async function saveSupplier() {
    try {
        const formData = {
            name: document.getElementById('supplierName').value,
            description: document.getElementById('supplierDescription').value,
            contactPerson: document.getElementById('supplierContactPerson').value,
            email: document.getElementById('supplierEmail').value,
            phone: document.getElementById('supplierPhone').value,
            address: document.getElementById('supplierAddress').value,
            city: document.getElementById('supplierCity').value,
            state: document.getElementById('supplierState').value,
            postalCode: document.getElementById('supplierPostalCode').value,
            country: document.getElementById('supplierCountry').value,
            taxId: document.getElementById('supplierTaxId').value,
            website: document.getElementById('supplierWebsite').value,
            notes: document.getElementById('supplierNotes').value,
            isActive: document.getElementById('supplierIsActive').checked
        };

        // Validación básica
        if (!formData.name.trim()) {
            showAlert('El nombre del proveedor es obligatorio', 'warning');
            return;
        }

        const url = currentSupplierId ? `/Supplier/Edit/${currentSupplierId}` : '/Supplier/CreateSupplier';
        const method = currentSupplierId ? 'PUT' : 'POST';

        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(formData)
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                showAlert(result.message, 'success');
                bootstrap.Modal.getInstance(document.getElementById('supplierModal')).hide();
                loadSuppliers();
            } else {
                showAlert(result.message || 'Error al guardar proveedor', 'error');
            }
        } else {
            showAlert('Error al guardar proveedor', 'error');
        }
    } catch (error) {
        
        showAlert('Error al guardar proveedor', 'error');
    }
}

// Función para eliminar proveedor
async function deleteSupplier(supplierId) {
    try {
        const result = await Swal.fire({
            title: '¿Estás seguro?',
            text: 'Esta acción no se puede deshacer',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        });

        if (result.isConfirmed) {
            const response = await fetch(`/Supplier/Delete/${supplierId}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            if (response.ok) {
                showAlert('Proveedor eliminado exitosamente', 'success');
                loadSuppliers();
            } else {
                showAlert('Error al eliminar proveedor', 'error');
            }
        }
    } catch (error) {
        
        showAlert('Error al eliminar proveedor', 'error');
    }
}

// Función para ver detalles del proveedor
async function viewSupplierDetails(supplierId) {
    try {
        const supplier = suppliersData.find(s => s.id === supplierId);
        if (!supplier) {
            showAlert('Proveedor no encontrado', 'error');
            return;
        }

        const content = document.getElementById('supplierDetailsContent');
        content.innerHTML = `
            <div class="row">
                <div class="col-md-6">
                    <h5 class="text-primary mb-3">Información General</h5>
                    <table class="table table-borderless">
                        <tr>
                            <td><strong>Nombre:</strong></td>
                            <td>${supplier.name || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Descripción:</strong></td>
                            <td>${supplier.description || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Estado:</strong></td>
                            <td>
                                <span class="badge ${supplier.isActive ? 'bg-success' : 'bg-danger'}">
                                    ${supplier.isActive ? 'Activo' : 'Inactivo'}
                                </span>
                            </td>
                        </tr>
                        <tr>
                            <td><strong>Fecha de Creación:</strong></td>
                            <td>${supplier.createdAt ? new Date(supplier.createdAt).toLocaleDateString() : 'No especificado'}</td>
                        </tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h5 class="text-primary mb-3">Información de Contacto</h5>
                    <table class="table table-borderless">
                        <tr>
                            <td><strong>Persona de Contacto:</strong></td>
                            <td>${supplier.contactPerson || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Email:</strong></td>
                            <td>${supplier.email ? `<a href="mailto:${supplier.email}">${supplier.email}</a>` : 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Teléfono:</strong></td>
                            <td>${supplier.phone ? `<a href="tel:${supplier.phone}">${supplier.phone}</a>` : 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Fax:</strong></td>
                            <td>${supplier.fax || 'No especificado'}</td>
                        </tr>
                    </table>
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-md-6">
                    <h5 class="text-primary mb-3">Dirección</h5>
                    <table class="table table-borderless">
                        <tr>
                            <td><strong>Dirección:</strong></td>
                            <td>${supplier.address || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Ciudad:</strong></td>
                            <td>${supplier.city || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Estado/Provincia:</strong></td>
                            <td>${supplier.state || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Código Postal:</strong></td>
                            <td>${supplier.postalCode || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>País:</strong></td>
                            <td>${supplier.country || 'No especificado'}</td>
                        </tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h5 class="text-primary mb-3">Información Adicional</h5>
                    <table class="table table-borderless">
                        <tr>
                            <td><strong>ID Fiscal:</strong></td>
                            <td>${supplier.taxId || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Número de Cuenta:</strong></td>
                            <td>${supplier.accountNumber || 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Sitio Web:</strong></td>
                            <td>${supplier.website ? `<a href="${supplier.website}" target="_blank">${supplier.website}</a>` : 'No especificado'}</td>
                        </tr>
                        <tr>
                            <td><strong>Notas:</strong></td>
                            <td>${supplier.notes || 'No especificado'}</td>
                        </tr>
                    </table>
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-12">
                    <h5 class="text-primary mb-3">Productos Asociados</h5>
                    <div id="supplierProducts">
                        <div class="text-center text-muted">
                            <i class="fas fa-spinner fa-spin"></i> Cargando productos...
                        </div>
                    </div>
                </div>
            </div>
        `;

        const modal = new bootstrap.Modal(document.getElementById('supplierDetailsModal'));
        modal.show();

        // Cargar productos del proveedor
        await loadSupplierProducts(supplierId);
    } catch (error) {
        
        showAlert('Error al cargar detalles del proveedor', 'error');
    }
}

// Función para cargar productos del proveedor
async function loadSupplierProducts(supplierId) {
    try {
        const response = await fetch(`/Supplier/GetSupplierProducts/${supplierId}`);
        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                displaySupplierProducts(data.products || []);
            } else {
                document.getElementById('supplierProducts').innerHTML = 
                    '<div class="text-center text-muted">Error al cargar productos</div>';
            }
        } else {
            document.getElementById('supplierProducts').innerHTML = 
                '<div class="text-center text-muted">Error al cargar productos</div>';
        }
    } catch (error) {
        
        document.getElementById('supplierProducts').innerHTML = 
            '<div class="text-center text-muted">Error al cargar productos</div>';
    }
}

// Función para mostrar productos del proveedor
function displaySupplierProducts(products) {
    const container = document.getElementById('supplierProducts');
    if (!container) return;

    if (products.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted">
                <i class="fas fa-box-open fa-2x mb-2"></i>
                <p>No hay productos asociados a este proveedor</p>
            </div>
        `;
        return;
    }

    const table = `
        <div class="table-responsive">
            <table class="table table-sm table-hover">
                <thead class="table-light">
                    <tr>
                        <th>Producto</th>
                        <th>Categoría</th>
                        <th>Precio</th>
                        <th>Estado</th>
                    </tr>
                </thead>
                <tbody>
                    ${products.map(product => `
                        <tr>
                            <td>
                                <div class="d-flex align-items-center">
                                    <div class="avatar-sm bg-info rounded-circle d-flex align-items-center justify-content-center me-2">
                                        <i class="fas fa-box text-white"></i>
                                    </div>
                                    <div>
                                        <strong>${product.name}</strong>
                                        ${product.description ? `<br><small class="text-muted">${product.description}</small>` : ''}
                                    </div>
                                </div>
                            </td>
                            <td>${product.category?.name || 'Sin categoría'}</td>
                            <td>$${product.price?.toFixed(2) || '0.00'}</td>
                            <td>
                                <span class="badge ${product.isActive ? 'bg-success' : 'bg-danger'}">
                                    ${product.isActive ? 'Activo' : 'Inactivo'}
                                </span>
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
    container.innerHTML = table;
}

// Función para refrescar proveedores
function refreshSuppliers() {
    loadSuppliers();
}

// Función para exportar proveedores
function exportSuppliers() {
    const filteredSuppliers = applyFilters(suppliersData);
    
    if (filteredSuppliers.length === 0) {
        showAlert('No hay proveedores para exportar', 'warning');
        return;
    }

    // Crear CSV
    const headers = ['Nombre', 'Contacto', 'Email', 'Teléfono', 'Ciudad', 'Estado', 'Dirección', 'País'];
    const csvContent = [
        headers.join(','),
        ...filteredSuppliers.map(supplier => [
            `"${supplier.name || ''}"`,
            `"${supplier.contactPerson || ''}"`,
            `"${supplier.email || ''}"`,
            `"${supplier.phone || ''}"`,
            `"${supplier.city || ''}"`,
            `"${supplier.isActive ? 'Activo' : 'Inactivo'}"`,
            `"${supplier.address || ''}"`,
            `"${supplier.country || ''}"`
        ].join(','))
    ].join('\n');

    // Descargar archivo
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `proveedores_${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Función para mostrar alertas
function showAlert(message, type = 'info') {
    Swal.fire({
        title: type === 'success' ? '¡Éxito!' : type === 'error' ? 'Error' : type === 'warning' ? 'Advertencia' : 'Información',
        text: message,
        icon: type,
        timer: type === 'success' ? 3000 : undefined,
        timerProgressBar: type === 'success'
    });
}

// Exportar funciones al scope global
window.openCreateSupplierModal = openCreateSupplierModal;
window.editSupplier = editSupplier;
window.deleteSupplier = deleteSupplier;
window.viewSupplierDetails = viewSupplierDetails;
window.saveSupplier = saveSupplier;
window.refreshSuppliers = refreshSuppliers;
window.exportSuppliers = exportSuppliers;
window.clearFilters = clearFilters;
window.changePage = changePage; 