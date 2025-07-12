// Table Management and Filtering

// ============================================================
// FUNCIONES DE UTILIDAD - DEFINIDAS PRIMERO
// ============================================================

function getTableStatusClass(status) {
    switch (status) {
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
    switch (status) {
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
    switch (status) {
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
        console.log('[Tables] Cargando mesas del servidor...');
        const response = await fetch('/Order/GetActiveTables');
        
        // Verificar el estado de la respuesta
        if (!response.ok) {
            if (response.status === 401) {
                console.error('[Tables] ❌ Usuario no autenticado');
                Swal.fire({
                    title: 'Sesión Expirada',
                    text: 'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.',
                    icon: 'warning',
                    confirmButtonText: 'Ir al Login'
                }).then(() => {
                    window.location.href = '/Auth/Login';
                });
                return;
            } else if (response.status === 403) {
                console.error('[Tables] ❌ Sin permisos para acceder');
                Swal.fire('Sin Permisos', 'No tienes permisos para acceder a esta información', 'error');
                return;
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        }
        
        const tables = await response.json();
        console.log(`[Tables] ✅ ${tables.length} mesas recibidas del servidor`);
        
        // Obtener áreas únicas y normalizar los nombres
        const areas = [...new Set(tables.map(table => table.areaName || 'Sin área'))];
        
        // Obtener estados únicos
        const statuses = [...new Set(tables.map(table => table.status))];
        
        // Crear botones de filtro para cada área
        const areaFiltersHtml = areas.map(area => `
            <button class="btn btn-outline-primary" data-area="${area}">${area}</button>
        `).join('');
        document.getElementById('areaFilters').innerHTML = `
            <button class="btn btn-outline-primary active" data-area="all">Todas</button>
            ${areaFiltersHtml}
        `;

        // Crear botones de filtro para cada estado
        const statusFiltersHtml = statuses.map(status => `
            <button class="${getStatusButtonClass(status)}" 
                    data-status="${status}"
                    data-bs-toggle="tooltip"
                    title="${getStatusDescription(status)}">
                ${status}
            </button>
        `).join('');
        document.getElementById('statusFilters').innerHTML = `
            <button class="btn btn-outline-primary active" data-status="all">Todos los estados</button>
            ${statusFiltersHtml}
        `;

        // Crear botones de mesas
        const tablesHtml = tables.map(table => `
            <button class="mesa-btn ${getTableStatusClass(table.status)}" 
                    data-id="${table.id}" 
                    data-status="${table.status}"
                    data-area="${table.areaName || 'Sin área'}"
                    data-bs-toggle="tooltip"
                    data-bs-placement="top"
                    title="${getStatusDescription(table.status)}"
                    onclick="handleTableClick('${table.id}', '${table.number}', '${table.status}')">
                <i class="fas fa-chair"></i> Mesa ${table.number}
                <small>${table.areaName ? `(${table.areaName})` : ''}</small>
                <span class="capacity-badge" data-bs-toggle="tooltip" title="Capacidad de personas">${table.capacity}</span>
            </button>
        `).join('');
        document.getElementById('tables').innerHTML = tablesHtml;

        // Asignar eventos a los botones de filtro
        document.querySelectorAll('#areaFilters .btn').forEach(btn => {
            btn.addEventListener('click', () => {
                filterTables(btn.dataset.area);
            });
        });

        document.querySelectorAll('#statusFilters .btn').forEach(btn => {
            btn.addEventListener('click', () => {
                filterTablesByStatus(btn.dataset.status);
            });
        });

        // Inicializar tooltips
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    } catch (error) {
        console.error('Error al cargar mesas:', error);
        Swal.fire('Error', 'No se pudieron cargar las mesas', 'error');
    }
}

// ============================================================
// FUNCIONES DE GESTIÓN DE MESAS
// ============================================================

function highlightSelectedTable(tableId) {
    document.querySelectorAll('.mesa-btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.id === tableId) {
            btn.classList.add('active');
        }
    });
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
    try {
        console.log(`[Frontend] updateTableStatus iniciado - TableId: ${tableId}, NewStatus: ${newStatus}`);
        
        const tableButton = document.querySelector(`.mesa-btn[data-id='${tableId}']`);
        if (!tableButton) {
            console.log(`[Frontend] ❌ No se encontró el botón de la mesa ${tableId}`);
            console.log(`[Frontend] Buscando botones disponibles:`);
            const allButtons = document.querySelectorAll('.mesa-btn');
            allButtons.forEach((btn, index) => {
                console.log(`[Frontend]   Botón ${index + 1}: data-id="${btn.dataset.id}", status="${btn.dataset.status}"`);
            });
            return;
        }
        
        console.log(`[Frontend] ✅ Botón de mesa encontrado: ${tableId}`);
        console.log(`[Frontend] Estado anterior de la mesa: ${tableButton.dataset.status}`);
        console.log(`[Frontend] Clases anteriores: ${tableButton.className}`);
        
        // Quitar todas las clases de estado anteriores
        const statusClasses = [
            'mesa-disponible', 'mesa-ocupada', 'mesa-reservada', 'mesa-en-espera', 
            'mesa-atendida', 'mesa-en-preparacion', 'mesa-servida', 'mesa-para-pago', 
            'mesa-pagada', 'mesa-bloqueada'
        ];
        tableButton.classList.remove(...statusClasses);
        
        // Agregar la nueva clase de estado
        const newStatusClass = getTableStatusClass(newStatus);
        tableButton.classList.add(newStatusClass);
        
        // Actualizar el atributo data-status
        tableButton.dataset.status = newStatus;
        
        console.log(`[Frontend] Clase CSS aplicada: ${newStatusClass}`);
        console.log(`[Frontend] Clases después del cambio: ${tableButton.className}`);
        console.log(`[Frontend] data-status después del cambio: ${tableButton.dataset.status}`);
        
        // Actualizar tooltip - destruir y recrear para asegurar actualización
        const tooltip = bootstrap.Tooltip.getInstance(tableButton);
        if (tooltip) {
            tooltip.dispose();
        }
        
        // Crear nuevo tooltip
        const newTooltip = new bootstrap.Tooltip(tableButton, {
            title: getStatusDescription(newStatus),
            placement: 'top'
        });
        
        console.log(`[Frontend] Tooltip actualizado con: ${getStatusDescription(newStatus)}`);
        console.log(`[Frontend] Estado actualizado a: ${newStatus} (clase: ${newStatusClass})`);
        console.log(`[Frontend] ✅ updateTableStatus completado exitosamente`);
        
    } catch (error) {
        console.error('[Frontend] ❌ Error en updateTableStatus:', error);
    }
}

// Función para actualizar visualmente una mesa
function updateTableUI(tableData) {
    console.log('[Frontend] updateTableUI iniciado');
    console.log('[Frontend] tableData recibido:', tableData);
    
    const tableButton = document.querySelector(`.mesa-btn[data-id='${tableData.id}']`);
    if (!tableButton) {
        console.log(`[Frontend] No se encontró el botón de la mesa ${tableData.id}`);
        return;
    }

    console.log(`[Frontend] Estado anterior de la mesa: ${tableButton.dataset.status}`);
    console.log(`[Frontend] Nuevo estado: ${tableData.status}`);

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

    console.log(`[Frontend] Clase CSS aplicada: ${newStatusClass}`);

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
    
    console.log(`[Frontend] Tooltip actualizado con: ${getStatusDescription(tableData.status)}`);
    console.log(`[Frontend] updateTableUI completado exitosamente`);
}

// ✅ Tables cargadas correctamente - v2.0 
console.log('[Tables] ✅ Funciones de gestión de mesas cargadas correctamente v2.0');