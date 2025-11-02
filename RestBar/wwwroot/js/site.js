// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ✅ Inicialización y manejo de dropdowns de Bootstrap
document.addEventListener('DOMContentLoaded', function() {
    // Esperar a que Bootstrap esté completamente cargado
    if (typeof bootstrap === 'undefined') {
        console.warn('Bootstrap no está disponible');
        return;
    }

    // Inicializar dropdowns si no están ya inicializados
    const dropdownElementList = document.querySelectorAll('.dropdown-toggle[data-bs-toggle="dropdown"]');
    dropdownElementList.forEach(dropdownToggleEl => {
        // Verificar si ya tiene una instancia
        let bsDropdown = bootstrap.Dropdown.getInstance(dropdownToggleEl);
        
        if (!bsDropdown) {
            try {
                // Crear nueva instancia con configuración
                bsDropdown = new bootstrap.Dropdown(dropdownToggleEl, {
                    autoClose: true,
                    boundary: 'clippingParents'
                });
            } catch (error) {
                console.warn('Error al inicializar dropdown:', error);
            }
        }
    });

    // Cerrar dropdowns al hacer clic fuera de ellos (si Bootstrap no lo hace automáticamente)
    document.addEventListener('click', function(event) {
        const dropdowns = document.querySelectorAll('.dropdown-toggle');
        dropdowns.forEach(function(dropdown) {
            const dropdownMenu = dropdown.parentElement.querySelector('.dropdown-menu');
            
            if (dropdownMenu && 
                !dropdown.contains(event.target) && 
                !dropdownMenu.contains(event.target) &&
                dropdownMenu.classList.contains('show')) {
                
                const bsDropdown = bootstrap.Dropdown.getInstance(dropdown);
                if (bsDropdown) {
                    bsDropdown.hide();
                }
            }
        });
    });

    // Asegurar que los dropdowns se cierren al hacer clic en un item
    document.querySelectorAll('.dropdown-item').forEach(function(item) {
        item.addEventListener('click', function() {
            const dropdownMenu = this.closest('.dropdown-menu');
            if (dropdownMenu) {
                const dropdownToggle = document.querySelector(`[aria-labelledby="${dropdownMenu.getAttribute('aria-labelledby')}"]`);
                if (dropdownToggle) {
                    const bsDropdown = bootstrap.Dropdown.getInstance(dropdownToggle);
                    if (bsDropdown) {
                        // Pequeño delay para permitir que el navegador siga el link
                        setTimeout(function() {
                            bsDropdown.hide();
                        }, 100);
                    }
                }
            }
        });
    });

    // ✅ Asegurar que todos los dropdowns estén ocultos al cargar la página
    document.querySelectorAll('.dark-dropdown').forEach(function(dropdownMenu) {
        dropdownMenu.classList.remove('show');
        dropdownMenu.style.display = 'none';
    });

    // ✅ Asegurar que el aria-expanded sea false por defecto
    document.querySelectorAll('.dropdown-toggle').forEach(function(toggle) {
        toggle.setAttribute('aria-expanded', 'false');
    });
});