/**
 * NOTIFICACIONES RESPONSIVAS
 * Mejora la experiencia de SweetAlert2 en dispositivos móviles
 */

// Configuración responsiva para SweetAlert2
(function() {
    'use strict';

    // Detectar si es dispositivo móvil
    function isMobile() {
        return window.innerWidth <= 768;
    }

    // Detectar si es móvil pequeño
    function isSmallMobile() {
        return window.innerWidth <= 480;
    }

    // Configuración por defecto para móviles
    const mobileConfig = {
        toast: true,
        position: 'top',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        width: 'auto',
        padding: '0.75rem',
        background: '#ffffff',
        color: '#333333',
        customClass: {
            container: 'swal2-mobile-container',
            popup: 'swal2-mobile-popup',
            title: 'swal2-mobile-title',
            content: 'swal2-mobile-content'
        }
    };

    // Configuración para móviles pequeños
    const smallMobileConfig = {
        ...mobileConfig,
        padding: '0.5rem',
        timer: 2500,
        customClass: {
            container: 'swal2-small-mobile-container',
            popup: 'swal2-small-mobile-popup',
            title: 'swal2-small-mobile-title',
            content: 'swal2-small-mobile-content'
        }
    };

    // Configuración para diálogos modales en móviles
    const mobileDialogConfig = {
        width: '95%',
        padding: '1rem',
        customClass: {
            container: 'swal2-mobile-dialog-container',
            popup: 'swal2-mobile-dialog-popup',
            title: 'swal2-mobile-dialog-title',
            content: 'swal2-mobile-dialog-content',
            actions: 'swal2-mobile-dialog-actions',
            confirmButton: 'swal2-mobile-dialog-confirm',
            cancelButton: 'swal2-mobile-dialog-cancel'
        }
    };

    // Función para obtener configuración optimizada
    function getOptimizedConfig(originalConfig) {
        if (!isMobile()) {
            return originalConfig;
        }

        const isToast = originalConfig.toast;
        const hasButtons = originalConfig.showConfirmButton || originalConfig.showCancelButton || originalConfig.showDenyButton;
        
        let optimizedConfig = { ...originalConfig };

        if (isToast) {
            // Configuración para toasts
            if (isSmallMobile()) {
                optimizedConfig = { ...optimizedConfig, ...smallMobileConfig };
            } else {
                optimizedConfig = { ...optimizedConfig, ...mobileConfig };
            }
        } else if (hasButtons) {
            // Configuración para diálogos modales
            optimizedConfig = { ...optimizedConfig, ...mobileDialogConfig };
        }

        return optimizedConfig;
    }

    // Función para mostrar notificación responsiva
    window.showResponsiveNotification = function(config) {
        const optimizedConfig = getOptimizedConfig(config);
        return Swal.fire(optimizedConfig);
    };

    // Función para mostrar toast responsivo
    window.showResponsiveToast = function(title, text, icon = 'info', timer = null) {
        const config = {
            title: title,
            text: text,
            icon: icon,
            toast: true,
            position: isMobile() ? 'top' : 'top-end',
            showConfirmButton: false,
            timer: timer || (isMobile() ? 3000 : 4000),
            timerProgressBar: true
        };

        return showResponsiveNotification(config);
    };

    // Función para mostrar confirmación responsiva
    window.showResponsiveConfirm = function(title, text, icon = 'question') {
        const config = {
            title: title,
            text: text,
            icon: icon,
            showCancelButton: true,
            confirmButtonText: 'Confirmar',
            cancelButtonText: 'Cancelar',
            reverseButtons: isMobile(),
            focusCancel: false,
            allowEscapeKey: true,
            allowOutsideClick: false
        };

        return showResponsiveNotification(config);
    };

    // Función para mostrar alerta responsiva
    window.showResponsiveAlert = function(title, text, icon = 'info') {
        const config = {
            title: title,
            text: text,
            icon: icon,
            confirmButtonText: 'Entendido',
            allowEscapeKey: true,
            allowOutsideClick: true
        };

        return showResponsiveNotification(config);
    };

    // Sobrescribir Swal.fire para hacer automáticamente responsivo
    const originalSwalFire = Swal.fire;
    Swal.fire = function(config) {
        if (typeof config === 'object' && config !== null) {
            const optimizedConfig = getOptimizedConfig(config);
            return originalSwalFire.call(this, optimizedConfig);
        }
        return originalSwalFire.apply(this, arguments);
    };

    // Ajustar múltiples toasts para móviles
    let toastCount = 0;
    const originalToast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });

    // Función para manejar múltiples toasts en móviles
    window.showStackedToast = function(title, text, icon = 'info') {
        if (isMobile()) {
            toastCount++;
            const topOffset = 10 + (toastCount - 1) * 65; // Espaciado entre toasts
            
            const config = {
                title: title,
                text: text,
                icon: icon,
                toast: true,
                position: 'top',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true,
                customClass: {
                    container: 'swal2-stacked-toast'
                },
                onOpen: (toast) => {
                    toast.style.top = `${topOffset}px`;
                },
                onClose: () => {
                    toastCount = Math.max(0, toastCount - 1);
                }
            };
            
            return Swal.fire(config);
        } else {
            return originalToast.fire({
                title: title,
                text: text,
                icon: icon
            });
        }
    };

    // Función para limpiar todos los toasts
    window.clearAllToasts = function() {
        Swal.close();
        toastCount = 0;
    };

    // Ajustar configuración cuando cambie el tamaño de pantalla
    let resizeTimer;
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function() {
            // Reiniciar contador de toasts si cambia a desktop
            if (!isMobile()) {
                toastCount = 0;
            }
        }, 250);
    });

    // Función para mostrar notificación de pago responsiva
    window.showPaymentNotification = function(title, text, icon = 'success', isFullPayment = false) {
        const timer = isFullPayment ? 5000 : 3000;
        const config = {
            title: title,
            text: text,
            icon: icon,
            toast: true,
            position: isMobile() ? 'top' : 'top-end',
            showConfirmButton: false,
            timer: timer,
            timerProgressBar: true,
            customClass: {
                popup: 'swal2-payment-notification'
            }
        };

        return showResponsiveNotification(config);
    };

    // Función para mostrar notificación de orden responsiva
    window.showOrderNotification = function(title, text, icon = 'info') {
        const config = {
            title: title,
            text: text,
            icon: icon,
            toast: true,
            position: isMobile() ? 'top' : 'top-end',
            showConfirmButton: false,
            timer: 4000,
            timerProgressBar: true,
            customClass: {
                popup: 'swal2-order-notification'
            }
        };

        return showResponsiveNotification(config);
    };

    // Función para mostrar notificación de error responsiva
    window.showErrorNotification = function(title, text) {
        const config = {
            title: title,
            text: text,
            icon: 'error',
            toast: true,
            position: isMobile() ? 'top' : 'top-end',
            showConfirmButton: false,
            timer: 5000,
            timerProgressBar: true,
            customClass: {
                popup: 'swal2-error-notification'
            }
        };

        return showResponsiveNotification(config);
    };

    // Función para mostrar notificación de éxito responsiva
    window.showSuccessNotification = function(title, text) {
        const config = {
            title: title,
            text: text,
            icon: 'success',
            toast: true,
            position: isMobile() ? 'top' : 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            customClass: {
                popup: 'swal2-success-notification'
            }
        };

        return showResponsiveNotification(config);
    };

    // Función para mostrar notificación de advertencia responsiva
    window.showWarningNotification = function(title, text) {
        const config = {
            title: title,
            text: text,
            icon: 'warning',
            toast: true,
            position: isMobile() ? 'top' : 'top-end',
            showConfirmButton: false,
            timer: 4000,
            timerProgressBar: true,
            customClass: {
                popup: 'swal2-warning-notification'
            }
        };

        return showResponsiveNotification(config);
    };

    // Configuración global para SweetAlert2 en móviles
    if (isMobile()) {
        // Configurar valores por defecto para móviles
        Swal.mixin({
            allowEscapeKey: false,
            allowOutsideClick: false,
            showClass: {
                popup: 'swal2-show-mobile'
            },
            hideClass: {
                popup: 'swal2-hide-mobile'
            }
        });
    }

})(); 