// 🚀 SISTEMA DE CUENTAS SEPARADAS - RestBar
// Funcionalidad completa para gestionar personas y asignar items

let currentPersons = [];
let currentOrderId = null;

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE GESTIÓN DE PERSONAS
function showPersonsManagementModal() {
    try {
        console.log('🔍 [SeparateAccounts] showPersonsManagementModal() - Iniciando modal de gestión de personas...');
        
        if (!currentOrderId) {
            console.warn('⚠️ [SeparateAccounts] showPersonsManagementModal() - No hay orden actual');
            Swal.fire('Error', 'No hay orden actual para gestionar personas', 'error');
            return;
        }

        // Cargar personas existentes
        loadPersonsForOrder();

        Swal.fire({
            title: '👥 Gestión de Cuentas Separadas',
            html: `
                <div class="separate-accounts-container">
                    <div class="row mb-3">
                        <div class="col-8">
                            <input type="text" id="personNameInput" class="form-control" placeholder="Nombre de la persona">
                        </div>
                        <div class="col-4">
                            <button type="button" class="btn btn-primary w-100" onclick="addPerson()">
                                <i class="fas fa-plus"></i> Agregar
                            </button>
                        </div>
                    </div>
                    
                    <div class="persons-list" id="personsList">
                        <div class="text-center text-muted">
                            <i class="fas fa-spinner fa-spin"></i> Cargando personas...
                        </div>
                    </div>
                    
                    <div class="row mt-3">
                        <div class="col-12">
                            <button type="button" class="btn btn-success w-100" onclick="showItemAssignmentModal()">
                                <i class="fas fa-list"></i> Asignar Items a Personas
                            </button>
                        </div>
                    </div>
                </div>
            `,
            width: '600px',
            showCancelButton: true,
            confirmButtonText: 'Cerrar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            customClass: {
                popup: 'separate-accounts-popup'
            },
            didOpen: () => {
                // Enfocar el input de nombre
                document.getElementById('personNameInput').focus();
            }
        });

        console.log('✅ [SeparateAccounts] showPersonsManagementModal() - Modal mostrado exitosamente');
    } catch (error) {
        console.error('❌ [SeparateAccounts] showPersonsManagementModal() - Error:', error);
        Swal.fire('Error', 'Error al mostrar modal de gestión de personas', 'error');
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: CARGAR PERSONAS DE LA ORDEN
async function loadPersonsForOrder() {
    try {
        console.log('🔍 [SeparateAccounts] loadPersonsForOrder() - Cargando personas...');
        
        const response = await fetch(`/Person/GetPersonsByOrder?orderId=${currentOrderId}`);
        const result = await response.json();
        
        if (result.success) {
            currentPersons = result.data || [];
            renderPersonsList();
            console.log(`📊 [SeparateAccounts] loadPersonsForOrder() - ${currentPersons.length} personas cargadas`);
        } else {
            console.warn('⚠️ [SeparateAccounts] loadPersonsForOrder() - Error al cargar personas:', result.message);
            currentPersons = [];
            renderPersonsList();
        }
    } catch (error) {
        console.error('❌ [SeparateAccounts] loadPersonsForOrder() - Error:', error);
        currentPersons = [];
        renderPersonsList();
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: RENDERIZAR LISTA DE PERSONAS
function renderPersonsList() {
    try {
        console.log('🔍 [SeparateAccounts] renderPersonsList() - Renderizando lista...');
        
        const personsList = document.getElementById('personsList');
        if (!personsList) return;

        if (currentPersons.length === 0) {
            personsList.innerHTML = `
                <div class="text-center text-muted py-3">
                    <i class="fas fa-users"></i><br>
                    No hay personas agregadas<br>
                    <small>Agrega personas para crear cuentas separadas</small>
                </div>
            `;
            return;
        }

        let html = '';
        currentPersons.forEach(person => {
            html += `
                <div class="person-item d-flex justify-content-between align-items-center p-2 mb-2 border rounded">
                    <div>
                        <i class="fas fa-user text-primary"></i>
                        <strong>${person.name}</strong>
                        ${person.notes ? `<br><small class="text-muted">${person.notes}</small>` : ''}
                    </div>
                    <div>
                        <button type="button" class="btn btn-sm btn-outline-danger" onclick="deletePerson('${person.id}')">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            `;
        });

        personsList.innerHTML = html;
        console.log(`✅ [SeparateAccounts] renderPersonsList() - ${currentPersons.length} personas renderizadas`);
    } catch (error) {
        console.error('❌ [SeparateAccounts] renderPersonsList() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: AGREGAR PERSONA
async function addPerson() {
    try {
        console.log('🔍 [SeparateAccounts] addPerson() - Agregando persona...');
        
        const nameInput = document.getElementById('personNameInput');
        const name = nameInput?.value?.trim();
        
        if (!name) {
            console.warn('⚠️ [SeparateAccounts] addPerson() - Nombre vacío');
            Swal.fire('Error', 'Por favor ingresa un nombre', 'warning');
            return;
        }

        if (currentPersons.some(p => p.name.toLowerCase() === name.toLowerCase())) {
            console.warn('⚠️ [SeparateAccounts] addPerson() - Nombre duplicado');
            Swal.fire('Error', 'Ya existe una persona con ese nombre', 'warning');
            return;
        }

        const response = await fetch('/Person/CreatePerson', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                name: name,
                orderId: currentOrderId
            })
        });

        const result = await response.json();
        
        if (result.success) {
            console.log('✅ [SeparateAccounts] addPerson() - Persona creada exitosamente');
            nameInput.value = '';
            await loadPersonsForOrder();
            Swal.fire('Éxito', 'Persona agregada exitosamente', 'success');
        } else {
            console.error('❌ [SeparateAccounts] addPerson() - Error:', result.message);
            Swal.fire('Error', result.message, 'error');
        }
    } catch (error) {
        console.error('❌ [SeparateAccounts] addPerson() - Error:', error);
        Swal.fire('Error', 'Error al agregar persona', 'error');
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: ELIMINAR PERSONA
async function deletePerson(personId) {
    try {
        console.log('🔍 [SeparateAccounts] deletePerson() - Eliminando persona...');
        
        const result = await Swal.fire({
            title: '¿Eliminar persona?',
            text: 'Esta acción no se puede deshacer',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d'
        });

        if (result.isConfirmed) {
            const response = await fetch(`/Person/DeletePerson?personId=${personId}`, {
                method: 'DELETE'
            });

            const deleteResult = await response.json();
            
            if (deleteResult.success) {
                console.log('✅ [SeparateAccounts] deletePerson() - Persona eliminada exitosamente');
                await loadPersonsForOrder();
                Swal.fire('Éxito', 'Persona eliminada exitosamente', 'success');
            } else {
                console.error('❌ [SeparateAccounts] deletePerson() - Error:', deleteResult.message);
                Swal.fire('Error', deleteResult.message, 'error');
            }
        }
    } catch (error) {
        console.error('❌ [SeparateAccounts] deletePerson() - Error:', error);
        Swal.fire('Error', 'Error al eliminar persona', 'error');
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: MOSTRAR MODAL DE ASIGNACIÓN DE ITEMS
async function showItemAssignmentModal() {
    try {
        console.log('🔍 [SeparateAccounts] showItemAssignmentModal() - Iniciando modal de asignación...');
        
        if (currentPersons.length === 0) {
            Swal.fire('Error', 'Debes agregar al menos una persona antes de asignar items', 'warning');
            return;
        }

        // Obtener items de la orden actual
        const orderItems = await getCurrentOrderItems();
        if (!orderItems || orderItems.length === 0) {
            Swal.fire('Error', 'No hay items en la orden actual', 'warning');
            return;
        }

        Swal.fire({
            title: '📋 Asignar Items a Personas',
            html: `
                <div class="item-assignment-container">
                    <div class="row mb-3">
                        <div class="col-12">
                            <h6>Items de la Orden:</h6>
                        </div>
                    </div>
                    
                    <div class="items-list" id="itemsAssignmentList" style="max-height: 400px; overflow-y: auto;">
                        ${renderItemsForAssignment(orderItems)}
                    </div>
                </div>
            `,
            width: '700px',
            showCancelButton: true,
            confirmButtonText: 'Guardar Asignaciones',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            customClass: {
                popup: 'item-assignment-popup'
            },
            preConfirm: () => {
                return saveItemAssignments();
            }
        });

        console.log('✅ [SeparateAccounts] showItemAssignmentModal() - Modal mostrado exitosamente');
    } catch (error) {
        console.error('❌ [SeparateAccounts] showItemAssignmentModal() - Error:', error);
        Swal.fire('Error', 'Error al mostrar modal de asignación', 'error');
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: RENDERIZAR ITEMS PARA ASIGNACIÓN
function renderItemsForAssignment(items) {
    try {
        console.log('🔍 [SeparateAccounts] renderItemsForAssignment() - Renderizando items...');
        
        let html = '';
        items.forEach(item => {
            const assignedPersonName = item.assignedToPersonName || 'Sin asignar';
            const isShared = item.isShared;
            
            html += `
                <div class="item-assignment-item border rounded p-3 mb-3">
                    <div class="row align-items-center">
                        <div class="col-md-6">
                            <h6 class="mb-1">${item.productName}</h6>
                            <small class="text-muted">
                                Cantidad: ${item.quantity} × $${item.unitPrice.toFixed(2)} = $${((item.quantity * item.unitPrice) - item.discount).toFixed(2)}
                            </small>
                        </div>
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-8">
                                    <select class="form-select form-select-sm" id="assignment_${item.id}" onchange="updateItemAssignment('${item.id}')">
                                        <option value="">Sin asignar</option>
                                        <option value="shared" ${isShared ? 'selected' : ''}>Compartido</option>
                                        ${currentPersons.map(person => 
                                            `<option value="${person.id}" ${item.assignedToPersonId === person.id ? 'selected' : ''}>${person.name}</option>`
                                        ).join('')}
                                    </select>
                                </div>
                                <div class="col-4">
                                    <span class="badge ${item.assignedToPersonId ? 'bg-primary' : isShared ? 'bg-warning' : 'bg-secondary'}">
                                        ${assignedPersonName}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });

        console.log(`✅ [SeparateAccounts] renderItemsForAssignment() - ${items.length} items renderizados`);
        return html;
    } catch (error) {
        console.error('❌ [SeparateAccounts] renderItemsForAssignment() - Error:', error);
        return '<div class="text-center text-muted">Error al cargar items</div>';
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: ACTUALIZAR ASIGNACIÓN DE ITEM
async function updateItemAssignment(itemId) {
    try {
        console.log('🔍 [SeparateAccounts] updateItemAssignment() - Actualizando asignación...');
        
        const select = document.getElementById(`assignment_${itemId}`);
        const selectedValue = select.value;
        
        if (!selectedValue) {
            // Sin asignar - marcar como compartido por defecto
            await assignItemToShared(itemId);
        } else if (selectedValue === 'shared') {
            // Compartido
            await assignItemToShared(itemId);
        } else {
            // Asignar a persona específica
            const personId = selectedValue;
            const person = currentPersons.find(p => p.id === personId);
            if (person) {
                await assignItemToPerson(itemId, personId, person.name);
            }
        }
    } catch (error) {
        console.error('❌ [SeparateAccounts] updateItemAssignment() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: ASIGNAR ITEM A PERSONA
async function assignItemToPerson(itemId, personId, personName) {
    try {
        console.log('🔍 [SeparateAccounts] assignItemToPerson() - Asignando item a persona...');
        
        const response = await fetch('/Person/AssignItemToPerson', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                itemId: itemId,
                personId: personId,
                personName: personName
            })
        });

        const result = await response.json();
        
        if (result.success) {
            console.log('✅ [SeparateAccounts] assignItemToPerson() - Item asignado exitosamente');
            // Actualizar badge visual
            updateItemBadge(itemId, personName, 'bg-primary');
        } else {
            console.error('❌ [SeparateAccounts] assignItemToPerson() - Error:', result.message);
        }
    } catch (error) {
        console.error('❌ [SeparateAccounts] assignItemToPerson() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: ASIGNAR ITEM COMO COMPARTIDO
async function assignItemToShared(itemId) {
    try {
        console.log('🔍 [SeparateAccounts] assignItemToShared() - Marcando item como compartido...');
        
        const response = await fetch('/Person/MarkItemAsShared', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                itemId: itemId
            })
        });

        const result = await response.json();
        
        if (result.success) {
            console.log('✅ [SeparateAccounts] assignItemToShared() - Item marcado como compartido exitosamente');
            // Actualizar badge visual
            updateItemBadge(itemId, 'Compartido', 'bg-warning');
        } else {
            console.error('❌ [SeparateAccounts] assignItemToShared() - Error:', result.message);
        }
    } catch (error) {
        console.error('❌ [SeparateAccounts] assignItemToShared() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: ACTUALIZAR BADGE VISUAL
function updateItemBadge(itemId, text, className) {
    try {
        const badge = document.querySelector(`#assignment_${itemId}`).parentNode.nextElementSibling.querySelector('.badge');
        if (badge) {
            badge.textContent = text;
            badge.className = `badge ${className}`;
        }
    } catch (error) {
        console.error('❌ [SeparateAccounts] updateItemBadge() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: OBTENER ITEMS DE LA ORDEN ACTUAL
async function getCurrentOrderItems() {
    try {
        console.log('🔍 [SeparateAccounts] getCurrentOrderItems() - Obteniendo items...');
        
        if (!currentOrderId) return [];
        
        const response = await fetch(`/Order/GetOrderItems?orderId=${currentOrderId}`);
        const result = await response.json();
        
        if (result.success) {
            console.log(`📊 [SeparateAccounts] getCurrentOrderItems() - ${result.data.length} items obtenidos`);
            return result.data;
        } else {
            console.warn('⚠️ [SeparateAccounts] getCurrentOrderItems() - Error al obtener items');
            return [];
        }
    } catch (error) {
        console.error('❌ [SeparateAccounts] getCurrentOrderItems() - Error:', error);
        return [];
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: GUARDAR ASIGNACIONES
async function saveItemAssignments() {
    try {
        console.log('🔍 [SeparateAccounts] saveItemAssignments() - Guardando asignaciones...');
        
        // Las asignaciones ya se guardaron individualmente
        // Solo mostrar mensaje de éxito
        Swal.fire('Éxito', 'Asignaciones guardadas exitosamente', 'success');
        
        // Actualizar la UI de la orden
        if (typeof updateOrderUI === 'function') {
            updateOrderUI();
        }
        
        console.log('✅ [SeparateAccounts] saveItemAssignments() - Asignaciones guardadas');
        return true;
    } catch (error) {
        console.error('❌ [SeparateAccounts] saveItemAssignments() - Error:', error);
        Swal.fire('Error', 'Error al guardar asignaciones', 'error');
        return false;
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: INICIALIZAR SISTEMA DE CUENTAS SEPARADAS
function initializeSeparateAccounts(orderId) {
    try {
        console.log('🔍 [SeparateAccounts] initializeSeparateAccounts() - Inicializando sistema...');
        
        currentOrderId = orderId;
        currentPersons = [];
        
        console.log(`✅ [SeparateAccounts] initializeSeparateAccounts() - Sistema inicializado para orden: ${orderId}`);
    } catch (error) {
        console.error('❌ [SeparateAccounts] initializeSeparateAccounts() - Error:', error);
    }
}

// 🎯 FUNCIÓN ESTRATÉGICA: OBTENER RESUMEN DE CUENTAS SEPARADAS
async function getSeparateAccountsSummary() {
    try {
        console.log('🔍 [SeparateAccounts] getSeparateAccountsSummary() - Obteniendo resumen...');
        
        if (!currentOrderId) return null;
        
        // Obtener personas
        await loadPersonsForOrder();
        
        if (currentPersons.length === 0) {
            return null; // No hay cuentas separadas
        }
        
        // Obtener items
        const items = await getCurrentOrderItems();
        
        // Calcular totales por persona
        const summary = {
            hasSeparateAccounts: true,
            persons: [],
            sharedItems: [],
            totalOrder: 0
        };
        
        let totalOrder = 0;
        
        // Calcular total por persona
        for (const person of currentPersons) {
            const personItems = items.filter(item => item.assignedToPersonId === person.id);
            const personTotal = personItems.reduce((sum, item) => sum + ((item.quantity * item.unitPrice) - item.discount), 0);
            
            summary.persons.push({
                id: person.id,
                name: person.name,
                items: personItems,
                total: personTotal
            });
            
            totalOrder += personTotal;
        }
        
        // Obtener items compartidos
        const sharedItems = items.filter(item => item.isShared);
        const sharedTotal = sharedItems.reduce((sum, item) => sum + ((item.quantity * item.unitPrice) - item.discount), 0);
        
        summary.sharedItems = sharedItems;
        summary.sharedTotal = sharedTotal;
        summary.totalOrder = totalOrder + sharedTotal;
        
        console.log(`📊 [SeparateAccounts] getSeparateAccountsSummary() - Resumen generado: ${summary.persons.length} personas, $${summary.totalOrder.toFixed(2)} total`);
        return summary;
    } catch (error) {
        console.error('❌ [SeparateAccounts] getSeparateAccountsSummary() - Error:', error);
        return null;
    }
}

// Exportar funciones globales
window.showPersonsManagementModal = showPersonsManagementModal;
window.initializeSeparateAccounts = initializeSeparateAccounts;
window.getSeparateAccountsSummary = getSeparateAccountsSummary;

// 🔍 DEBUG: Verificar que las funciones se exportaron correctamente
console.log('🔍 [SeparateAccounts] Funciones exportadas:');
console.log('  - showPersonsManagementModal:', typeof window.showPersonsManagementModal);
console.log('  - initializeSeparateAccounts:', typeof window.initializeSeparateAccounts);
console.log('  - getSeparateAccountsSummary:', typeof window.getSeparateAccountsSummary);
