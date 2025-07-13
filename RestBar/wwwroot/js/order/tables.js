// Table Management and Filtering

// ============================================================
// FUNCIONES DE UTILIDAD - DEFINIDAS PRIMERO
// ============================================================

// ✅ NUEVO: Función para mapear números de estado a strings
function mapTableStatus(status) {
    const statusMap = {
        0: 'Disponible',
        1: 'Ocupada',
        2: 'Reservada',
        3: 'EnEspera',
        4: 'Atendida',
        5: 'EnPreparacion',
        6: 'Servida',
        7: 'ParaPago',
        8: 'Pagada',
        9: 'Bloqueada'
    };
    
    // Si ya es string, devolverlo tal como está
    if (typeof status === 'string') {
        return status;
    }
    
    // Si es número, mapearlo
    if (typeof status === 'number') {
        return statusMap[status] || 'Desconocido';
    }
    
    return 'Desconocido';
}

function getTableStatusClass(status) {
    // ✅ NUEVO: Mapear el estado antes de obtener la clase
    const mappedStatus = mapTableStatus(status);
    switch (mappedStatus) {
        case 'Disponible':
            return 'mesa-disponible';
        case 'Ocupada':
            return 'mesa-ocupada';
        case 'Reservada':
            return 'mesa-reservada';
        case 'EnEspera':
            return 'mesa-en-espera';
        case 'Atendida':
            return 'mesa-atendida';
        case 'EnPreparacion':
            return 'mesa-en-preparacion';
        case 'Servida':
            return 'mesa-servida';
        case 'ParaPago':
            return 'mesa-para-pago';
        case 'Pagada':
            return 'mesa-pagada';
        case 'Bloqueada':
            return 'mesa-bloqueada';
        default:
            return '';
    }
}

function getStatusButtonClass(status) {
    const mappedStatus = mapTableStatus(status);
    switch (mappedStatus) {
        case 'Disponible':
            return 'btn btn-outline-success';
        case 'Ocupada':
            return 'btn btn-outline-danger';
        case 'Reservada':
            return 'btn btn-outline-warning';
        case 'EnEspera':
            return 'btn btn-outline-info';
        case 'Atendida':
            return 'btn btn-outline-primary';
        case 'EnPreparacion':
            return 'btn btn-outline-secondary';
        case 'Servida':
            return 'btn btn-outline-success';
        case 'ParaPago':
            return 'btn btn-outline-warning';
        case 'Pagada':
            return 'btn btn-outline-success';
        case 'Bloqueada':
            return 'btn btn-outline-dark';
        default:
            return 'btn btn-outline-secondary';
    }
}

function getStatusDescription(status) {
    const mappedStatus = mapTableStatus(status);
    switch (mappedStatus) {
        case 'Disponible':
            return 'Mesa disponible para ocupar';
        case 'Ocupada':
            return 'Mesa ocupada por clientes';
        case 'Reservada':
            return 'Mesa reservada';
        case 'EnEspera':
            return 'Orden tomada, esperando preparación';
        case 'Atendida':
            return 'Mesa atendida';
        case 'EnPreparacion':
            return 'Orden en preparación en cocina';
        case 'Servida':
            return 'Orden servida, lista para pago';
        case 'ParaPago':
            return 'Mesa lista para realizar el pago';
        case 'Pagada':
            return 'Mesa pagada, lista para limpiar';
        case 'Bloqueada':
            return 'Mesa bloqueada temporalmente';
        default:
            return 'Estado desconocido';
    }
}

// ============================================================
// FUNCIONES PRINCIPALES
// ============================================================
async function loadTables() {
    try {
        console.log('🔍 [Tables] loadTables() - Iniciando carga de mesas...');
        
        const res = await fetch('/Order/GetActiveTables', {
            credentials: 'include',
            headers: { 'Accept': 'application/json' }
        });
        console.log("IRIRIRIRIRIRIRIIRIIIIIIIIIIIIIIIIIIIIIIIIIIIII");
        console.log(res);

       // alert();
        if (!res.ok) {
            console.error('❌ [Tables] loadTables() - Error HTTP:', res.status, res.statusText);
            throw new Error(`HTTP ${res.status}`);
        }

        const json = await res.json();
        console.log('📡 [Tables] loadTables() - Respuesta recibida:', json);

        // ✅ CORREGIDO: Obtener array de mesas
        const tables = Array.isArray(json?.data) ? json.data : [];
        console.log('📊 [Tables] loadTables() - Mesas encontradas:', tables.length);
        
        if (tables.length === 0) {
            console.log('⚠️ [Tables] loadTables() - No hay mesas disponibles');
            document.getElementById('tables').innerHTML = '<p class="text-muted">No hay mesas disponibles</p>';
            return;
        }
        
        // ✅ NUEVO: Log de cada mesa para debugging
        tables.forEach((table, index) => {
            console.log(`🔍 [Tables] loadTables() - Mesa ${index + 1}:`, {
                id: table.id,
                tableNumber: table.tableNumber,
                status: table.status,
                areaName: table.areaName,
                capacity: table.capacity,
                isActive: table.isActive
            });
        });
        
        // Normalizar y renderizar mesas
        renderTables(tables);
        console.log('✅ [Tables] loadTables() - Mesas renderizadas exitosamente');
    } catch (e) {
        console.error('❌ [Tables] loadTables() - Error:', e);
        Swal.fire('Error', 'No se pudieron cargar las mesas', 'error');
    }
}

// Función para renderizar las mesas
function renderTables(tables) {
    const tablesContainer = document.getElementById('tables');
    
    const tablesHtml = tables.map(table => {
        const mappedStatus = mapTableStatus(table.status);
        return `
        <div class="col-md-2 col-sm-4 col-6 mb-2">
            <div class="card table-card ${getTableStatusClass(table.status)}" 
                 data-table-id="${table.id}" 
                 data-table-status="${mappedStatus}"
                 data-table-area="${table.areaName || ''}">
                <div class="card-body text-center p-2">
                    <h6 class="card-title mb-1">Mesa ${table.tableNumber}</h6>
                    <p class="card-text mb-2">
                        <span class="badge ${getTableStatusClass(table.status)}">
                            ${mappedStatus}
                        </span>
                    </p>
                    <small class="text-muted d-block mb-2">${getStatusDescription(table.status)}</small>
                    <button class="btn btn-primary btn-sm select-table-btn" 
                            data-table-id="${table.id}" 
                            data-table-number="${table.tableNumber}"
                            data-table-status="${mappedStatus}">
                        Seleccionar
                    </button>
                </div>
            </div>
        </div>
    `;
    }).join('');
    
    tablesContainer.innerHTML = tablesHtml;
    
    // ✅ NUEVO: Inicializar tooltips para las descripciones
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

//async function loadTables() {
//    try {
//        const response = await fetch('/Order/GetActiveTables', {
//            credentials: 'include',
//            headers: { 'Accept': 'application/json' }
//        });

//        // Verificar el estado de la respuesta
//        if (!response.ok) {
//            if (response.status === 401) {
//                Swal.fire({
//                    title: 'Sesión Expirada',
//                    text: 'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.',
//                    icon: 'warning',
//                    confirmButtonText: 'Ir al Login'
//                }).then(() => {
//                    window.location.href = '/Auth/Login';
//                });
//                return;
//            } else if (response.status === 403) {
//                Swal.fire('Sin Permisos', 'No tienes permisos para acceder a esta información', 'error');
//                return;
//            } else {
//                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
//            }
//        }

//        // Log para ver exactamente lo que llega
//        const payloadText = await response.text();
//        let result;
//        try {
//            result = JSON.parse(payloadText);
//        } catch (e) {
//            console.error('Respuesta no JSON del servidor:', payloadText);
//            Swal.fire('Error', 'Respuesta inválida del servidor (no JSON).', 'error');
//            return;
//        }

//        // Asegurar array
//        const tables = Array.isArray(result?.data)
//            ? result.data
//            : (Array.isArray(result) ? result : []);

//        if (!Array.isArray(tables)) {
//            Swal.fire('Error', 'Estructura de datos inválida para mesas.', 'error');
//            return;
//        }

//        // Normalizar campos
//        const normTables = tables.map(t => ({
//            id: t.id,
//            tableNumber: t.tableNumber,
//            status: (t.status ?? 'Desconocido').toString(),
//            areaName: (t.areaName ?? 'Sin área').toString(),
//            capacity: t.capacity ?? ''
//        }));

//        // Obtener áreas y estados únicos
//        const areas = [...new Set(normTables.map(table => table.areaName || 'Sin área'))];
//        const statuses = [...new Set(normTables.map(table => table.status || 'Desconocido'))];

//        // Crear botones de filtro para cada área
//        const areaFiltersHtml = areas.map(area => `
//      <button type="button" class="btn btn-outline-primary" data-area="${area}">${area}</button>
//    `).join('');
//        document.getElementById('areaFilters').innerHTML = `
//      <button type="button" class="btn btn-outline-primary active" data-area="all">Todas</button>
//      ${areaFiltersHtml}
//    `;

//        // Crear botones de filtro para cada estado
//        const statusFiltersHtml = statuses.map(status => `
//      <button type="button" class="${getStatusButtonClass(status)}"
//              data-status="${status}"
//              data-bs-toggle="tooltip"
//              title="${getStatusDescription(status)}">
//        ${status}
//      </button>
//    `).join('');
//        document.getElementById('statusFilters').innerHTML = `
//      <button type="button" class="btn btn-outline-primary active" data-status="all">Todos los estados</button>
//      ${statusFiltersHtml}
//    `;

//        // Crear botones de mesas
//        const tablesHtml = normTables.map(table => `
//      <button type="button" class="mesa-btn ${getTableStatusClass(table.status)}"
//              data-id="${table.id}"
//              data-status="${table.status}"
//              data-area="${table.areaName}"
//              data-bs-toggle="tooltip"
//              data-bs-placement="top"
//              title="${getStatusDescription(table.status)}"
//              onclick="handleTableClick('${table.id}', '${table.tableNumber}', '${table.status}')">
//        <i class="fas fa-chair"></i> Mesa ${table.tableNumber}
//        <small>${table.areaName ? `(${table.areaName})` : ''}</small>
//        <span class="capacity-badge" data-bs-toggle="tooltip" title="Capacidad de personas">${table.capacity}</span>
//      </button>
//    `).join('');
//        document.getElementById('tables').innerHTML = tablesHtml;

//        // Asignar eventos a los botones de filtro
//        document.querySelectorAll('#areaFilters .btn').forEach(btn => {
//            btn.addEventListener('click', () => {
//                filterTables(btn.dataset.area);
//            });
//        });

//        document.querySelectorAll('#statusFilters .btn').forEach(btn => {
//            btn.addEventListener('click', () => {
//                filterTablesByStatus(btn.dataset.status);
//            });
//        });

//        // Inicializar tooltips
//        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
//        tooltipTriggerList.map(function (tooltipTriggerEl) {
//            return new bootstrap.Tooltip(tooltipTriggerEl);
//        });
//    } catch (error) {
//        console.error('Error en loadTables:', error);
//        Swal.fire('Error', 'No se pudieron cargar las mesas', 'error');
//    }
//}


// ============================================================
// FUNCIONES DE GESTIÓN DE MESAS
// ============================================================

function highlightSelectedTable(tableId) {
    console.log('🎯 [Tables] highlightSelectedTable() - Resaltando mesa seleccionada:', tableId);
    
    // Remover selección de todas las mesas
    console.log('🧹 [Tables] highlightSelectedTable() - Deseleccionando todas las mesas anteriores...');
    document.querySelectorAll('.table-card').forEach(card => {
        const wasSelected = card.classList.contains('table-selected');
        if (wasSelected) {
            const tableId = card.dataset.tableId;
            console.log(`🔄 [Tables] highlightSelectedTable() - Deseleccionando mesa ${tableId}`);
        }
        
        card.classList.remove('table-selected', 'border-primary', 'shadow-lg');
        card.style.transform = '';
        card.style.boxShadow = '';
        
        // Remover clase active del botón
        const button = card.querySelector('.select-table-btn');
        if (button) {
            button.classList.remove('btn-success');
            button.classList.add('btn-primary');
            button.textContent = 'Seleccionar';
            button.disabled = false; // ✅ NUEVO: Habilitar botón para permitir nueva selección
        }
    });
    
    // Resaltar la mesa seleccionada
    const selectedCard = document.querySelector(`.table-card[data-table-id="${tableId}"]`);
    if (selectedCard) {
        selectedCard.classList.add('table-selected', 'border-primary', 'shadow-lg');
        selectedCard.style.transform = 'scale(1.05)';
        selectedCard.style.boxShadow = '0 8px 25px rgba(0,123,255,0.3)';
        
        // Cambiar el botón a verde y texto "Seleccionada"
        const button = selectedCard.querySelector('.select-table-btn');
        if (button) {
            button.classList.remove('btn-primary');
            button.classList.add('btn-success');
            button.textContent = '✓ Seleccionada';
            button.disabled = true;
        }
        
        console.log('✅ [Tables] highlightSelectedTable() - Mesa resaltada exitosamente');
    } else {
        console.log('⚠️ [Tables] highlightSelectedTable() - No se encontró la mesa con ID:', tableId);
    }
}

// Filtrar mesas por área
function filterTables(area) {
    // Actualizar botones de filtro de área
    document.querySelectorAll('#areaFilters .btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.area === area) {
            btn.classList.add('active');
        }
    });
    applyFilters();
}

// Filtrar mesas por estado
function filterTablesByStatus(status) {
    // Actualizar botones de filtro de estado
    document.querySelectorAll('#statusFilters .btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.status === status) {
            btn.classList.add('active');
        }
    });
    applyFilters();
}

// Función auxiliar para aplicar ambos filtros
function applyFilters() {
    const activeAreaButton = document.querySelector('#areaFilters .btn.active');
    const activeStatusButton = document.querySelector('#statusFilters .btn.active');
    
    const currentArea = activeAreaButton.dataset.area;
    const currentStatus = activeStatusButton.dataset.status;

    const tables = document.querySelectorAll('.mesa-btn');
    tables.forEach(table => {
        const matchesArea = currentArea === 'all' || table.dataset.area === currentArea;
        const matchesStatus = currentStatus === 'all' || table.dataset.status === currentStatus;
        table.style.display = matchesArea && matchesStatus ? '' : 'none';
    });
}

// Función para actualizar automáticamente el estado de una mesa específica
async function updateTableStatus(tableId, newStatus) {
    console.log('🔍 [Tables] updateTableStatus() - INICIANDO - tableId:', tableId, 'newStatus:', newStatus);
    try {
        console.log('📋 [Tables] updateTableStatus() - Buscando mesa en DOM...');
        console.log('📋 [Tables] updateTableStatus() - Selector usado: .table-card[data-table-id=\'' + tableId + '\']');
        
        // ✅ CORREGIDO: Buscar por el selector correcto usado en renderTables
        const tableCard = document.querySelector(`.table-card[data-table-id='${tableId}']`);
        if (!tableCard) {
            console.log('⚠️ [Tables] updateTableStatus() - Mesa no encontrada con ID:', tableId);
            console.log('⚠️ [Tables] updateTableStatus() - Verificando todas las mesas disponibles...');
            
            // Debug: mostrar todas las mesas disponibles
            const allTables = document.querySelectorAll('.table-card');
            console.log('📊 [Tables] updateTableStatus() - Mesas disponibles en DOM:', allTables.length);
            allTables.forEach((table, index) => {
                console.log(`📊 [Tables] updateTableStatus() - Mesa ${index + 1}:`, {
                    id: table.dataset.tableId,
                    status: table.dataset.tableStatus,
                    classes: table.className
                });
            });
            return;
        }
        
        console.log('✅ [Tables] updateTableStatus() - Mesa encontrada en DOM, actualizando estado...');
        console.log('📋 [Tables] updateTableStatus() - Mesa actual:', {
            id: tableCard.dataset.tableId,
            status: tableCard.dataset.tableStatus,
            classes: tableCard.className
        });
        
        // ✅ CORREGIDO: Actualizar el estado de la mesa completa
        console.log('🔄 [Tables] updateTableStatus() - Procesando cambio de estado...');
        const mappedStatus = mapTableStatus(newStatus);
        const newStatusClass = getTableStatusClass(newStatus);
        
        console.log('📋 [Tables] updateTableStatus() - Estados calculados:');
        console.log('📋 [Tables] updateTableStatus() - mappedStatus:', mappedStatus);
        console.log('📋 [Tables] updateTableStatus() - newStatusClass:', newStatusClass);
        
        // Quitar todas las clases de estado anteriores del card
        const statusClasses = [
            'mesa-disponible', 'mesa-ocupada', 'mesa-reservada', 'mesa-en-espera', 
            'mesa-atendida', 'mesa-en-preparacion', 'mesa-servida', 'mesa-para-pago', 
            'mesa-pagada', 'mesa-bloqueada'
        ];
        
        console.log('🧹 [Tables] updateTableStatus() - Removiendo clases anteriores...');
        tableCard.classList.remove(...statusClasses);
        console.log('✅ [Tables] updateTableStatus() - Clases anteriores removidas');
        
        console.log('➕ [Tables] updateTableStatus() - Agregando nueva clase:', newStatusClass);
        tableCard.classList.add(newStatusClass);
        console.log('✅ [Tables] updateTableStatus() - Nueva clase agregada');
        
        // Actualizar atributos del card
        console.log('📝 [Tables] updateTableStatus() - Actualizando atributos del card...');
        tableCard.dataset.tableStatus = mappedStatus;
        console.log('✅ [Tables] updateTableStatus() - Atributo data-table-status actualizado a:', mappedStatus);
        
        // Actualizar el badge de estado
        console.log('🏷️ [Tables] updateTableStatus() - Actualizando badge de estado...');
        const statusBadge = tableCard.querySelector('.badge');
        if (statusBadge) {
            console.log('📋 [Tables] updateTableStatus() - Badge encontrado, actualizando...');
            statusBadge.className = `badge ${newStatusClass}`;
            statusBadge.textContent = mappedStatus;
            console.log('✅ [Tables] updateTableStatus() - Badge actualizado:', {
                className: statusBadge.className,
                textContent: statusBadge.textContent
            });
        } else {
            console.log('⚠️ [Tables] updateTableStatus() - Badge no encontrado');
        }
        
        // Actualizar la descripción de estado
        console.log('📄 [Tables] updateTableStatus() - Actualizando descripción de estado...');
        const statusDescription = tableCard.querySelector('.text-muted');
        if (statusDescription) {
            const newDescription = getStatusDescription(newStatus);
            statusDescription.textContent = newDescription;
            console.log('✅ [Tables] updateTableStatus() - Descripción actualizada a:', newDescription);
        } else {
            console.log('⚠️ [Tables] updateTableStatus() - Descripción no encontrada');
        }
        
        // Actualizar el botón si existe
        console.log('🔘 [Tables] updateTableStatus() - Actualizando botón de selección...');
        const selectButton = tableCard.querySelector('.select-table-btn');
        if (selectButton) {
            selectButton.dataset.tableStatus = mappedStatus;
            console.log('✅ [Tables] updateTableStatus() - Botón actualizado, data-table-status:', mappedStatus);
        } else {
            console.log('⚠️ [Tables] updateTableStatus() - Botón no encontrado');
        }
        
        console.log('✅ [Tables] updateTableStatus() - COMPLETADO - Estado de mesa actualizado exitosamente');
        console.log('📊 [Tables] updateTableStatus() - Mesa final:', {
            id: tableCard.dataset.tableId,
            status: tableCard.dataset.tableStatus,
            classes: tableCard.className,
            badgeText: statusBadge ? statusBadge.textContent : 'N/A',
            description: statusDescription ? statusDescription.textContent : 'N/A'
        });
        
    } catch (error) {
        // Error silencioso para evitar spam en consola
    }
}

// Función para actualizar visualmente una mesa
function updateTableUI(tableData) {
    const tableButton = document.querySelector(`.mesa-btn[data-id='${tableData.id}']`);
    if (!tableButton) {
        return;
    }

    // Quitar clases de estado anteriores
    const statusClasses = [
        'mesa-disponible', 'mesa-ocupada', 'mesa-reservada', 'mesa-en-espera', 
        'mesa-atendida', 'mesa-en-preparacion', 'mesa-servida', 'mesa-para-pago', 
        'mesa-pagada', 'mesa-bloqueada'
    ];
    tableButton.classList.remove(...statusClasses);

    // Añadir nueva clase y actualizar atributos
    const newStatusClass = getTableStatusClass(tableData.status);
    tableButton.classList.add(newStatusClass);
    tableButton.dataset.status = tableData.status;

    // Actualizar tooltip - destruir y recrear para asegurar actualización
    const tooltip = bootstrap.Tooltip.getInstance(tableButton);
    if (tooltip) {
        tooltip.dispose();
    }
    
    // Crear nuevo tooltip
    const newTooltip = new bootstrap.Tooltip(tableButton, {
        title: getStatusDescription(tableData.status),
        placement: 'top'
    });
}

// ✅ Tables cargadas correctamente - v2.0

// Función para limpiar selección de mesas
function clearTableSelection() {
    console.log('🧹 [Tables] clearTableSelection() - Limpiando selección de mesas...');
    
    document.querySelectorAll('.table-card').forEach(card => {
        card.classList.remove('table-selected', 'border-primary', 'shadow-lg');
        card.style.transform = '';
        card.style.boxShadow = '';
        
        // Restaurar botón a estado original
        const button = card.querySelector('.select-table-btn');
        if (button) {
            button.classList.remove('btn-success');
            button.classList.add('btn-primary');
            button.textContent = 'Seleccionar';
            button.disabled = false;
        }
    });
    
    console.log('✅ [Tables] clearTableSelection() - Selección limpiada exitosamente');
}

// Exponer funciones globalmente
window.highlightSelectedTable = highlightSelectedTable;
window.clearTableSelection = clearTableSelection;

// Event listeners para botones de selección de mesa
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('select-table-btn')) {
        const tableId = e.target.dataset.tableId;
        const tableNumber = e.target.dataset.tableNumber;
        const tableStatus = e.target.dataset.tableStatus;
        
        console.log('🔍 [Tables] Mesa seleccionada:', { tableId, tableNumber, tableStatus });
        
        // Llamar a la función de manejo de clic en mesa
        if (typeof handleTableClick === 'function') {
            handleTableClick(tableId, tableNumber, tableStatus);
        } else {
            console.warn('⚠️ [Tables] handleTableClick function not found');
        }
    }
});