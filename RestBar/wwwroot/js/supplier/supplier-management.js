// Variables globales
let suppliersData = [];
let currentPage = 1;
let itemsPerPage = 10;
let currentSupplierId = null;

// Función para cargar proveedores
async function loadSuppliers() {
    try {
        console.log('[DEBUG] loadSuppliers iniciado');
        const response = await fetch('/Supplier/GetSuppliers');
        const data = await response.json();
        
        console.log('[DEBUG] Respuesta del servidor:', data);
        
        if (data.success) {
            suppliersData = data.suppliers || [];
            console.log('[DEBUG] suppliersData actualizado:', suppliersData.length, 'proveedores');
            displaySuppliers();
            updateSuppliersCount();
        } else {
            console.log('[DEBUG] Error en respuesta del servidor:', data.message);
            showAlert('Error al cargar proveedores: ' + data.message, 'error');
        }
    } catch (error) {
        console.log('[DEBUG] Error en loadSuppliers:', error);
        showAlert('Error al cargar proveedores: ' + error.message, 'error');
    }
}

// Función para mostrar proveedores en la tabla
function displaySuppliers() {
    console.log('[DEBUG] displaySuppliers iniciado');
    const tbody = document.getElementById('suppliersTableBody');
    if (!tbody) {
        console.log('[DEBUG] No se encontró tbody');
        return;
    }

    // Aplicar filtros
    let filteredSuppliers = applyFilters(suppliersData);
    console.log('[DEBUG] Filtros aplicados:', filteredSuppliers.length, 'proveedores');
    
    // Aplicar paginación
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    const paginatedSuppliers = filteredSuppliers.slice(startIndex, endIndex);
    console.log('[DEBUG] Paginación aplicada:', paginatedSuppliers.length, 'proveedores en página', currentPage);

    tbody.innerHTML = '';

    if (paginatedSuppliers.length === 0) {
        console.log('[DEBUG] No hay proveedores para mostrar en la página actual');
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

    paginatedSuppliers.forEach((supplier) => {
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
                <span class="badge bg-info">${supplier.products || 0} productos</span>
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
    console.log('[DEBUG] displaySuppliers completado');
}

// Función para aplicar filtros
function applyFilters(suppliers) {
    if (!Array.isArray(suppliers)) return [];

    let filtered = suppliers;

    // Filtro de búsqueda
    const searchTerm = document.getElementById('searchInput')?.value.toLowerCase();
    if (searchTerm) {
        filtered = filtered.filter(supplier => 
            (supplier.name && supplier.name.toLowerCase().includes(searchTerm)) ||
            (supplier.contactPerson && supplier.contactPerson.toLowerCase().includes(searchTerm)) ||
            (supplier.email && supplier.email.toLowerCase().includes(searchTerm)) ||
            (supplier.phone && supplier.phone.toLowerCase().includes(searchTerm)) ||
            (supplier.city && supplier.city.toLowerCase().includes(searchTerm))
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

// Función para actualizar contador de proveedores
function updateSuppliersCount() {
    const countElement = document.getElementById('suppliersCount');
    if (countElement) {
        const filteredSuppliers = applyFilters(suppliersData);
        countElement.textContent = filteredSuppliers.length;
    }
}

// Función para actualizar solo un registro específico en la tabla
function updateSingleSupplierRow(supplierId, updatedData) {
    console.log('[DEBUG] Actualizando fila específica para supplierId:', supplierId);
    console.log('[DEBUG] Datos actualizados:', updatedData);
    
    const tbody = document.getElementById('suppliersTableBody');
    if (!tbody) {
        console.log('[DEBUG] No se encontró tbody');
        return;
    }

    // Buscar la fila específica
    const rows = tbody.querySelectorAll('tr');
    console.log('[DEBUG] Filas encontradas:', rows.length);
    
    for (let row of rows) {
        const editButton = row.querySelector('button[onclick*="editSupplier"]');
        if (editButton) {
            const rowSupplierId = editButton.getAttribute('onclick').match(/'([^']+)'/)?.[1];
            console.log('[DEBUG] Comparando rowSupplierId:', rowSupplierId, 'con supplierId:', supplierId);
            if (rowSupplierId === supplierId) {
                console.log('[DEBUG] ¡Fila encontrada! Actualizando...');
                // Actualizar el contenido de la fila
                const cells = row.querySelectorAll('td');
                if (cells.length >= 8) {
                    // Actualizar nombre y descripción
                    const nameCell = cells[0];
                    nameCell.innerHTML = `
                        <div class="d-flex align-items-center">
                            <div class="avatar-sm bg-primary rounded-circle d-flex align-items-center justify-content-center me-2">
                                <i class="fas fa-truck text-white"></i>
                            </div>
                            <div>
                                <strong>${updatedData.name || 'Sin nombre'}</strong>
                                ${updatedData.description ? `<br><small class="text-muted">${updatedData.description}</small>` : ''}
                            </div>
                        </div>
                    `;

                    // Actualizar contacto
                    cells[1].textContent = updatedData.contactPerson || 'No especificado';

                    // Actualizar email
                    cells[2].innerHTML = updatedData.email ? 
                        `<a href="mailto:${updatedData.email}" class="text-decoration-none">${updatedData.email}</a>` : 
                        'No especificado';

                    // Actualizar teléfono
                    cells[3].innerHTML = updatedData.phone ? 
                        `<a href="tel:${updatedData.phone}" class="text-decoration-none">${updatedData.phone}</a>` : 
                        'No especificado';

                    // Actualizar ciudad
                    cells[4].textContent = updatedData.city || 'No especificado';

                    // Actualizar estado
                    cells[5].innerHTML = `
                        <span class="badge ${updatedData.isActive ? 'bg-success' : 'bg-danger'}">
                            ${updatedData.isActive ? 'Activo' : 'Inactivo'}
                        </span>
                    `;

                    // Los productos se mantienen igual (no cambian al editar)
                    // cells[6] se mantiene igual

                    // Actualizar el array local
                    const index = suppliersData.findIndex(s => s.id == supplierId);
                    if (index !== -1) {
                        suppliersData[index] = { ...suppliersData[index], ...updatedData };
                        console.log('[DEBUG] Array local actualizado en índice:', index);
                    }

                    // Actualizar contador
                    updateSuppliersCount();
                    console.log('[DEBUG] Actualización completada exitosamente');
                    break;
                }
            }
        }
    }
    
    // Si llegamos aquí, no se encontró la fila
    console.log('[DEBUG] No se encontró la fila para actualizar');
    showAlert('No se pudo actualizar la fila en la tabla. Los datos se han guardado correctamente.', 'warning');
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
            li.innerHTML = `<button class="page-link" onclick="changePage(${i})">${i}</button>`;
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
function editSupplier(supplierId) {
    const supplier = suppliersData.find(s => s.id == supplierId);
    if (!supplier) {
        showAlert('Proveedor no encontrado', 'error');
        return;
    }

    currentSupplierId = supplierId;
    fillSupplierForm(supplier);
    document.getElementById('supplierModalLabel').innerHTML = '<i class="fas fa-edit me-2"></i>Editar Proveedor';
    const modal = new bootstrap.Modal(document.getElementById('supplierModal'));
    modal.show();
}

// Función para llenar formulario con datos del proveedor
function fillSupplierForm(supplier) {
    document.getElementById('supplierName').value = supplier.name || '';
    document.getElementById('supplierDescription').value = supplier.description || '';
    document.getElementById('supplierContactPerson').value = supplier.contactPerson || '';
    document.getElementById('supplierEmail').value = supplier.email || '';
    document.getElementById('supplierPhone').value = supplier.phone || '';
    document.getElementById('supplierFax').value = supplier.fax || '';
    document.getElementById('supplierAddress').value = supplier.address || '';
    document.getElementById('supplierCity').value = supplier.city || '';
    document.getElementById('supplierState').value = supplier.state || '';
    document.getElementById('supplierPostalCode').value = supplier.postalCode || '';
    document.getElementById('supplierCountry').value = supplier.country || '';
    document.getElementById('supplierTaxId').value = supplier.taxId || '';
    document.getElementById('supplierAccountNumber').value = supplier.accountNumber || '';
    document.getElementById('supplierWebsite').value = supplier.website || '';
    document.getElementById('supplierPaymentTerms').value = supplier.paymentTerms || '';
    document.getElementById('supplierLeadTimeDays').value = supplier.leadTimeDays || '';
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
            ...(currentSupplierId && { id: currentSupplierId }),
            name: document.getElementById('supplierName').value,
            description: document.getElementById('supplierDescription').value,
            contactPerson: document.getElementById('supplierContactPerson').value,
            email: document.getElementById('supplierEmail').value,
            phone: document.getElementById('supplierPhone').value,
            fax: document.getElementById('supplierFax').value,
            address: document.getElementById('supplierAddress').value,
            city: document.getElementById('supplierCity').value,
            state: document.getElementById('supplierState').value,
            postalCode: document.getElementById('supplierPostalCode').value,
            country: document.getElementById('supplierCountry').value,
            taxId: document.getElementById('supplierTaxId').value,
            accountNumber: document.getElementById('supplierAccountNumber').value,
            website: document.getElementById('supplierWebsite').value,
            paymentTerms: document.getElementById('supplierPaymentTerms').value,
            leadTimeDays: document.getElementById('supplierLeadTimeDays').value ? parseInt(document.getElementById('supplierLeadTimeDays').value) : null,
            notes: document.getElementById('supplierNotes').value,
            isActive: document.getElementById('supplierIsActive').checked
        };

        // Validaciones completas
        const validationErrors = [];
        
        // Validación de nombre (obligatorio)
        if (!formData.name.trim()) {
            validationErrors.push('El nombre del proveedor es obligatorio');
        } else if (formData.name.trim().length < 2) {
            validationErrors.push('El nombre debe tener al menos 2 caracteres');
        } else if (formData.name.trim().length > 100) {
            validationErrors.push('El nombre no puede exceder 100 caracteres');
        }
        
        // Validación de email (si se proporciona)
        if (formData.email && formData.email.trim()) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(formData.email.trim())) {
                validationErrors.push('El formato del email no es válido');
            }
        }
        
        // Validación de teléfono (si se proporciona)
        if (formData.phone && formData.phone.trim()) {
            const phoneRegex = /^[\+]?[0-9\s\-\(\)]{7,15}$/;
            if (!phoneRegex.test(formData.phone.trim())) {
                validationErrors.push('El formato del teléfono no es válido');
            }
        }
        
        // Validación de fax (si se proporciona)
        if (formData.fax && formData.fax.trim()) {
            const faxRegex = /^[\+]?[0-9\s\-\(\)]{7,15}$/;
            if (!faxRegex.test(formData.fax.trim())) {
                validationErrors.push('El formato del fax no es válido');
            }
        }
        

        
        // Validación de tiempo de entrega (si se proporciona)
        if (formData.leadTimeDays !== null && formData.leadTimeDays !== '') {
            const leadTime = parseInt(formData.leadTimeDays);
            if (isNaN(leadTime) || leadTime < 0 || leadTime > 365) {
                validationErrors.push('El tiempo de entrega debe ser un número entre 0 y 365 días');
            }
        }
        
        // Validación de descripción (si se proporciona)
        if (formData.description && formData.description.trim().length > 500) {
            validationErrors.push('La descripción no puede exceder 500 caracteres');
        }
        
        // Validación de notas (si se proporciona)
        if (formData.notes && formData.notes.trim().length > 1000) {
            validationErrors.push('Las notas no pueden exceder 1000 caracteres');
        }
        
        // Mostrar errores si los hay
        if (validationErrors.length > 0) {
            showAlert('Por favor corrija los siguientes errores:\n\n' + validationErrors.join('\n'), 'warning');
            return;
        }

        const url = currentSupplierId ? '/Supplier/EditSupplier' : '/Supplier/CreateSupplier';
        
        console.log('[DEBUG] URL:', url);
        console.log('[DEBUG] Datos a enviar:', formData);
        console.log('[DEBUG] JSON a enviar:', JSON.stringify(formData, null, 2));
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(formData)
        });
        
        console.log('[DEBUG] Status de respuesta:', response.status);
        console.log('[DEBUG] Headers de respuesta:', response.headers);

        if (response.ok) {
            const result = await response.json();
            console.log('[DEBUG] Respuesta del servidor:', result);
            
            if (result.success) {
                showAlert(result.message, 'success');
                bootstrap.Modal.getInstance(document.getElementById('supplierModal')).hide();
                
                // Actualizar solo el registro específico en lugar de recargar todo
                if (currentSupplierId) {
                    console.log('[DEBUG] Editando proveedor existente, ID:', currentSupplierId);
                    // Es una edición - actualizar solo esa fila
                    updateSingleSupplierRow(currentSupplierId, formData);
                } else {
                    console.log('[DEBUG] Creando nuevo proveedor');
                    // Es una creación - agregar el nuevo proveedor a la lista
                    if (result.supplier) {
                        const newSupplier = {
                            id: result.supplier.id,
                            name: result.supplier.name,
                            description: result.supplier.description,
                            contactPerson: result.supplier.contactPerson,
                            email: result.supplier.email,
                            phone: result.supplier.phone,
                            fax: result.supplier.fax,
                            address: result.supplier.address,
                            city: result.supplier.city,
                            state: result.supplier.state,
                            postalCode: result.supplier.postalCode,
                            country: result.supplier.country,
                            taxId: result.supplier.taxId,
                            accountNumber: result.supplier.accountNumber,
                            website: result.supplier.website,
                            paymentTerms: result.supplier.paymentTerms,
                            leadTimeDays: result.supplier.leadTimeDays,
                            notes: result.supplier.notes,
                            isActive: result.supplier.isActive,
                            products: 0
                        };
                        
                        // Agregar al array local
                        suppliersData.push(newSupplier);
                        
                        // Actualizar la tabla
                        displaySuppliers();
                        updateSuppliersCount();
                        
                        console.log('[DEBUG] Nuevo proveedor agregado a la lista:', newSupplier);
                    } else {
                        // Fallback: recargar toda la lista si no hay datos del proveedor
                        console.log('[DEBUG] No hay datos del proveedor, recargando lista completa');
                        loadSuppliers();
                    }
                }
            } else {
                console.log('[DEBUG] Error en la respuesta:', result);
                console.log('[DEBUG] ¿Hay errores específicos?', result.errors);
                console.log('[DEBUG] Tipo de errores:', typeof result.errors);
                console.log('[DEBUG] Es array?', Array.isArray(result.errors));
                
                // Si hay errores de validación específicos, mostrarlos
                if (result.errors && Array.isArray(result.errors) && result.errors.length > 0) {
                    console.log('[DEBUG] Mostrando errores específicos:', result.errors);
                    
                    let errorHtml = `
                        <div class="text-start">
                            <p class="mb-3">Por favor corrija los siguientes errores:</p>
                            <ul class="list-unstyled">
                                ${result.errors.map(error => `<li class="text-danger"><i class="fas fa-times-circle me-2"></i>${error}</li>`).join('')}
                            </ul>
                    `;
                    
                    // Si hay errores por campo, mostrarlos también
                    if (result.fieldErrors) {
                        console.log('[DEBUG] Errores por campo:', result.fieldErrors);
                        errorHtml += `
                            <hr>
                            <h6>Errores por campo:</h6>
                            <div class="mt-2">
                        `;
                        for (const [field, fieldErrors] of Object.entries(result.fieldErrors)) {
                            if (fieldErrors && fieldErrors.length > 0) {
                                errorHtml += `
                                    <div class="mb-2">
                                        <strong>${field}:</strong>
                                        <ul class="small">
                                            ${fieldErrors.map(err => `<li class="text-danger">${err}</li>`).join('')}
                                        </ul>
                                    </div>
                                `;
                            }
                        }
                        errorHtml += '</div>';
                    }
                    
                    errorHtml += '</div>';
                    
                    Swal.fire({
                        title: 'Errores de Validación',
                        html: errorHtml,
                        icon: 'warning',
                        confirmButtonText: 'Entendido',
                        confirmButtonColor: '#f39c12',
                        width: '600px'
                    });
                } else {
                    console.log('[DEBUG] Mostrando mensaje general:', result.message);
                    console.log('[DEBUG] Estructura completa de result:', JSON.stringify(result, null, 2));
                    showAlert(result.message || 'Error al guardar proveedor', 'error');
                }
            }
        } else {
            console.log('[DEBUG] Error HTTP:', response.status, response.statusText);
            const errorText = await response.text();
            console.log('[DEBUG] Respuesta de error:', errorText);
            showAlert('Error al guardar proveedor: ' + response.statusText, 'error');
        }
    } catch (error) {
        showAlert('Error al guardar proveedor: ' + error.message, 'error');
    }
}

// Función para eliminar proveedor
async function deleteSupplier(supplierId) {
    try {
        console.log('[DEBUG] deleteSupplier iniciado para ID:', supplierId);
        
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
            console.log('[DEBUG] Confirmación aceptada, enviando solicitud de eliminación...');
            
            const response = await fetch(`/Supplier/DeleteSupplier/${supplierId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            console.log('[DEBUG] Respuesta del servidor:', response.status);

            if (response.ok) {
                const result = await response.json();
                console.log('[DEBUG] Resultado de eliminación:', result);
                
                if (result.success) {
                    showAlert(result.message, 'success');
                    
                    // Eliminar solo la fila específica del DOM en lugar de recargar todo
                    removeSupplierRow(supplierId);
                    
                    // Actualizar el array local
                    suppliersData = suppliersData.filter(s => s.id != supplierId);
                    
                    // Actualizar contador
                    updateSuppliersCount();
                    
                    console.log('[DEBUG] Proveedor eliminado exitosamente del DOM y array local');
                } else {
                    // Si el error es por productos asociados, mostrar un mensaje más informativo
                    if (result.hasProducts) {
                        // Obtener y mostrar los productos asociados
                        showSupplierProducts(supplierId, result.message);
                    } else {
                        showAlert(result.message || 'Error al eliminar proveedor', 'error');
                    }
                }
            } else {
                const errorText = await response.text();
                console.log('[DEBUG] Error en respuesta:', errorText);
                showAlert('Error al eliminar proveedor: ' + response.statusText, 'error');
            }
        }
    } catch (error) {
        console.log('[DEBUG] Error en deleteSupplier:', error);
        showAlert('Error al eliminar proveedor: ' + error.message, 'error');
    }
}

// Función para eliminar una fila específica del DOM
function removeSupplierRow(supplierId) {
    console.log('[DEBUG] removeSupplierRow iniciado para ID:', supplierId);
    
    const tbody = document.getElementById('suppliersTableBody');
    if (!tbody) {
        console.log('[DEBUG] No se encontró tbody');
        return;
    }

    // Buscar la fila específica
    const rows = tbody.querySelectorAll('tr');
    console.log('[DEBUG] Filas encontradas:', rows.length);
    
    for (let row of rows) {
        const deleteButton = row.querySelector('button[onclick*="deleteSupplier"]');
        if (deleteButton) {
            const rowSupplierId = deleteButton.getAttribute('onclick').match(/'([^']+)'/)?.[1];
            console.log('[DEBUG] Comparando rowSupplierId:', rowSupplierId, 'con supplierId:', supplierId);
            
            if (rowSupplierId === supplierId) {
                console.log('[DEBUG] ¡Fila encontrada! Eliminando...');
                row.remove();
                console.log('[DEBUG] Fila eliminada exitosamente del DOM');
                break;
            }
        }
    }
    
    // Si no hay más filas, mostrar mensaje de "no hay proveedores"
    if (tbody.children.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted py-4">
                    <i class="fas fa-inbox fa-2x mb-2"></i>
                    <p>No se encontraron proveedores</p>
                </td>
            </tr>
        `;
        console.log('[DEBUG] Tabla vacía, mostrando mensaje de "no hay proveedores"');
    }
}

// Función para ver detalles del proveedor
function viewSupplierDetails(supplierId) {
    const supplier = suppliersData.find(s => s.id == supplierId);
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
                    <tr><td><strong>Nombre:</strong></td><td>${supplier.name || 'No especificado'}</td></tr>
                    <tr><td><strong>Descripción:</strong></td><td>${supplier.description || 'No especificado'}</td></tr>
                    <tr><td><strong>Estado:</strong></td><td><span class="badge ${supplier.isActive ? 'bg-success' : 'bg-danger'}">${supplier.isActive ? 'Activo' : 'Inactivo'}</span></td></tr>
                    <tr><td><strong>ID Fiscal:</strong></td><td>${supplier.taxId || 'No especificado'}</td></tr>
                    <tr><td><strong>Número de Cuenta:</strong></td><td>${supplier.accountNumber || 'No especificado'}</td></tr>
                    <tr><td><strong>Términos de Pago:</strong></td><td>${supplier.paymentTerms || 'No especificado'}</td></tr>
                    <tr><td><strong>Tiempo de Entrega:</strong></td><td>${supplier.leadTimeDays ? supplier.leadTimeDays + ' días' : 'No especificado'}</td></tr>
                </table>
            </div>
            <div class="col-md-6">
                <h5 class="text-primary mb-3">Información de Contacto</h5>
                <table class="table table-borderless">
                    <tr><td><strong>Persona de Contacto:</strong></td><td>${supplier.contactPerson || 'No especificado'}</td></tr>
                    <tr><td><strong>Email:</strong></td><td>${supplier.email ? `<a href="mailto:${supplier.email}">${supplier.email}</a>` : 'No especificado'}</td></tr>
                    <tr><td><strong>Teléfono:</strong></td><td>${supplier.phone ? `<a href="tel:${supplier.phone}">${supplier.phone}</a>` : 'No especificado'}</td></tr>
                    <tr><td><strong>Fax:</strong></td><td>${supplier.fax || 'No especificado'}</td></tr>
                    <tr><td><strong>Sitio Web:</strong></td><td>${supplier.website ? `<a href="${supplier.website}" target="_blank">${supplier.website}</a>` : 'No especificado'}</td></tr>
                </table>
            </div>
        </div>
        <div class="row mt-3">
            <div class="col-md-6">
                <h5 class="text-primary mb-3">Dirección</h5>
                <table class="table table-borderless">
                    <tr><td><strong>Dirección:</strong></td><td>${supplier.address || 'No especificado'}</td></tr>
                    <tr><td><strong>Ciudad:</strong></td><td>${supplier.city || 'No especificado'}</td></tr>
                    <tr><td><strong>Estado/Provincia:</strong></td><td>${supplier.state || 'No especificado'}</td></tr>
                    <tr><td><strong>Código Postal:</strong></td><td>${supplier.postalCode || 'No especificado'}</td></tr>
                    <tr><td><strong>País:</strong></td><td>${supplier.country || 'No especificado'}</td></tr>
                </table>
            </div>
            <div class="col-md-6">
                <h5 class="text-primary mb-3">Información Adicional</h5>
                <table class="table table-borderless">
                    <tr><td><strong>Notas:</strong></td><td>${supplier.notes || 'No especificado'}</td></tr>
                    <tr><td><strong>Productos:</strong></td><td><span class="badge bg-info">${supplier.products || 0} productos</span></td></tr>
                </table>
            </div>
        </div>
    `;

    const modal = new bootstrap.Modal(document.getElementById('supplierDetailsModal'));
    modal.show();
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

    const headers = ['Nombre', 'Contacto', 'Email', 'Teléfono', 'Ciudad', 'Estado'];
    const csvContent = [
        headers.join(','),
        ...filteredSuppliers.map(supplier => [
            `"${supplier.name || ''}"`,
            `"${supplier.contactPerson || ''}"`,
            `"${supplier.email || ''}"`,
            `"${supplier.phone || ''}"`,
            `"${supplier.city || ''}"`,
            `"${supplier.isActive ? 'Activo' : 'Inactivo'}"`
        ].join(','))
    ].join('\n');

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

// Función para validar email
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Función para validar teléfono
function validatePhone(phone) {
    const phoneRegex = /^[\+]?[0-9\s\-\(\)]{7,15}$/;
    return phoneRegex.test(phone);
}



// Función para validar campo en tiempo real
function validateField(fieldId, validationType) {
    const field = document.getElementById(fieldId);
    const value = field.value.trim();
    const feedbackDiv = document.getElementById(fieldId + 'Feedback');
    
    if (!feedbackDiv) {
        const div = document.createElement('div');
        div.id = fieldId + 'Feedback';
        div.className = 'invalid-feedback';
        field.parentNode.appendChild(div);
    }
    
    let isValid = true;
    let errorMessage = '';
    
    switch (validationType) {
        case 'email':
            if (value && !validateEmail(value)) {
                isValid = false;
                errorMessage = 'Formato de email inválido';
            }
            break;
        case 'phone':
            if (value && !validatePhone(value)) {
                isValid = false;
                errorMessage = 'Formato de teléfono inválido';
            }
            break;

        case 'required':
            if (!value) {
                isValid = false;
                errorMessage = 'Este campo es obligatorio';
            }
            break;
    }
    
    if (isValid) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
        feedbackDiv.textContent = '';
    } else {
        field.classList.remove('is-valid');
        field.classList.add('is-invalid');
        feedbackDiv.textContent = errorMessage;
    }
    
    return isValid;
}

// Función para mostrar productos asociados a un proveedor
async function showSupplierProducts(supplierId, errorMessage) {
    try {
        console.log('[DEBUG] showSupplierProducts iniciado para supplierId:', supplierId);
        
        const response = await fetch(`/Supplier/GetSupplierProducts/${supplierId}`);
        const data = await response.json();
        
        if (data.success && data.products) {
            const supplier = suppliersData.find(s => s.id == supplierId);
            const supplierName = supplier ? supplier.name : 'Proveedor';
            
            let productsHtml = '';
            if (data.products.length > 0) {
                productsHtml = `
                    <div class="table-responsive mt-3">
                        <table class="table table-sm table-bordered">
                            <thead class="table-light">
                                <tr>
                                    <th>Producto</th>
                                    <th>Descripción</th>
                                    <th>Precio</th>
                                    <th>Estado</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${data.products.map(product => `
                                    <tr>
                                        <td><strong>${product.name || 'Sin nombre'}</strong></td>
                                        <td>${product.description || 'Sin descripción'}</td>
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
            }
            
            Swal.fire({
                title: 'No se puede eliminar',
                html: `
                    <div class="text-start">
                        <p class="mb-3">${errorMessage}</p>
                        <p class="text-muted small">Para eliminar este proveedor, primero debe eliminar o reasignar los productos asociados.</p>
                        ${productsHtml}
                    </div>
                `,
                icon: 'warning',
                confirmButtonText: 'Entendido',
                confirmButtonColor: '#3085d6',
                width: '600px'
            });
        } else {
            showAlert('Error al obtener productos asociados: ' + (data.message || 'Error desconocido'), 'error');
        }
    } catch (error) {
        console.log('[DEBUG] Error en showSupplierProducts:', error);
        showAlert('Error al obtener productos asociados: ' + error.message, 'error');
    }
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

// Inicialización cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    setupFilters();
    setupFormValidations();
    
    // Solo cargar proveedores si la tabla está vacía o solo tiene el mensaje de "No se encontraron"
    const tbody = document.getElementById('suppliersTableBody');
    const hasData = tbody && tbody.children.length > 0 && 
                   !tbody.querySelector('td[colspan="8"]')?.textContent.includes('No se encontraron');
    
    if (!hasData) {
        loadSuppliers();
    } else {
        // Si ya hay datos, solo actualizar el contador y configurar paginación
        const suppliersCount = tbody.children.length;
        suppliersData = Array.from(tbody.querySelectorAll('tr')).map(row => {
            const cells = row.querySelectorAll('td');
            if (cells.length >= 8) {
                return {
                    id: row.querySelector('button[onclick*="editSupplier"]')?.getAttribute('onclick')?.match(/'([^']+)'/)?.[1] || '',
                    name: cells[0]?.querySelector('strong')?.textContent || '',
                    contactPerson: cells[1]?.textContent || '',
                    email: cells[2]?.querySelector('a')?.textContent || '',
                    phone: cells[3]?.querySelector('a')?.textContent || '',
                    city: cells[4]?.textContent || '',
                    isActive: cells[5]?.querySelector('.badge')?.textContent?.includes('Activo') || false,
                    products: parseInt(cells[6]?.querySelector('.badge')?.textContent?.match(/\d+/)?.[0] || '0')
                };
            }
            return null;
        }).filter(s => s && s.id);
        
        updateSuppliersCount();
        displaySuppliers();
    }
});

// Función para configurar validaciones del formulario
function setupFormValidations() {
    // Validación del nombre (obligatorio)
    const nameField = document.getElementById('supplierName');
    if (nameField) {
        nameField.addEventListener('blur', () => validateField('supplierName', 'required'));
        nameField.addEventListener('input', () => {
            if (nameField.value.trim().length >= 2) {
                validateField('supplierName', 'required');
            }
        });
    }
    
    // Validación del email
    const emailField = document.getElementById('supplierEmail');
    if (emailField) {
        emailField.addEventListener('blur', () => validateField('supplierEmail', 'email'));
        emailField.addEventListener('input', () => {
            if (emailField.value.trim()) {
                validateField('supplierEmail', 'email');
            }
        });
    }
    
    // Validación del teléfono
    const phoneField = document.getElementById('supplierPhone');
    if (phoneField) {
        phoneField.addEventListener('blur', () => validateField('supplierPhone', 'phone'));
        phoneField.addEventListener('input', () => {
            if (phoneField.value.trim()) {
                validateField('supplierPhone', 'phone');
            }
        });
    }
    
    // Validación del fax
    const faxField = document.getElementById('supplierFax');
    if (faxField) {
        faxField.addEventListener('blur', () => validateField('supplierFax', 'phone'));
        faxField.addEventListener('input', () => {
            if (faxField.value.trim()) {
                validateField('supplierFax', 'phone');
            }
        });
    }
    

}