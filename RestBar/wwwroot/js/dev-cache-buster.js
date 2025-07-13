// ✅ SCRIPT DE DESARROLLO: Cache Buster para archivos estáticos
// Este script se ejecuta solo en desarrollo para forzar la recarga de archivos

(function() {
    'use strict';
    
    // Solo ejecutar en desarrollo
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
        
        // Función para agregar timestamp a URLs de archivos estáticos
        function addCacheBuster(url) {
            if (url && url.includes('~')) {
                const separator = url.includes('?') ? '&' : '?';
                return url + separator + '_cb=' + Date.now();
            }
            return url;
        }
        
        // Función para recargar todos los scripts
        function reloadScripts() {
            console.log('🔄 [CacheBuster] Recargando scripts...');
            
            // Recargar scripts con timestamp
            const scripts = document.querySelectorAll('script[src*="~/js/"]');
            scripts.forEach(script => {
                const newSrc = addCacheBuster(script.src);
                if (newSrc !== script.src) {
                    console.log('🔄 [CacheBuster] Recargando:', script.src);
                    script.src = newSrc;
                }
            });
        }
        
        // Función para recargar todos los CSS
        function reloadCSS() {
            console.log('🔄 [CacheBuster] Recargando CSS...');
            
            // Recargar CSS con timestamp
            const links = document.querySelectorAll('link[href*="~/css/"]');
            links.forEach(link => {
                const newHref = addCacheBuster(link.href);
                if (newHref !== link.href) {
                    console.log('🔄 [CacheBuster] Recargando:', link.href);
                    link.href = newHref;
                }
            });
        }
        
        // Interceptar fetch para agregar cache buster a archivos estáticos
        const originalFetch = window.fetch;
        window.fetch = function(url, options) {
            if (typeof url === 'string' && (url.includes('~/js/') || url.includes('~/css/'))) {
                url = addCacheBuster(url);
                console.log('🔄 [CacheBuster] Fetch con cache buster:', url);
            }
            return originalFetch.call(this, url, options);
        };
        
        // Agregar botones de desarrollo para forzar recarga
        function addDevControls() {
            if (document.getElementById('dev-controls')) return;
            
            const devControls = document.createElement('div');
            devControls.id = 'dev-controls';
            devControls.style.cssText = `
                position: fixed;
                top: 10px;
                right: 10px;
                z-index: 9999;
                background: #007bff;
                color: white;
                padding: 10px;
                border-radius: 5px;
                font-family: monospace;
                font-size: 12px;
                box-shadow: 0 2px 10px rgba(0,0,0,0.3);
            `;
            
            devControls.innerHTML = `
                <div style="margin-bottom: 5px;">DEV TOOLS</div>
                <button onclick="location.reload(true)" style="margin: 2px; padding: 5px; background: #28a745; color: white; border: none; border-radius: 3px; cursor: pointer;">🔄 Reload</button>
                <button onclick="window.reloadScripts()" style="margin: 2px; padding: 5px; background: #ffc107; color: black; border: none; border-radius: 3px; cursor: pointer;">📜 JS</button>
                <button onclick="window.reloadCSS()" style="margin: 2px; padding: 5px; background: #17a2b8; color: white; border: none; border-radius: 3px; cursor: pointer;">🎨 CSS</button>
                <button onclick="localStorage.clear(); sessionStorage.clear(); location.reload(true)" style="margin: 2px; padding: 5px; background: #dc3545; color: white; border: none; border-radius: 3px; cursor: pointer;">🗑️ Clear</button>
            `;
            
            document.body.appendChild(devControls);
        }
        
        // Exponer funciones globalmente
        window.reloadScripts = reloadScripts;
        window.reloadCSS = reloadCSS;
        
        // Agregar controles cuando el DOM esté listo
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', addDevControls);
        } else {
            addDevControls();
        }
        
        console.log('✅ [CacheBuster] Script de desarrollo cargado - Cache deshabilitado');
        console.log('🛠️ [CacheBuster] Controles de desarrollo disponibles en la esquina superior derecha');
        
        // Auto-reload cada 30 segundos en desarrollo (opcional)
        // setInterval(() => {
        //     console.log('🔄 [CacheBuster] Auto-reload en 30 segundos...');
        //     setTimeout(() => location.reload(true), 30000);
        // }, 30000);
    }
})();
