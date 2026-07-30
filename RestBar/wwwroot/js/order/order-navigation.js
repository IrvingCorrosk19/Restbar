/**
 * RestBar POS exit navigation — dirty-state aware leave/back.
 * Exit URLs come from server-rendered data-exit-url (no hardcoded paths).
 */
(function (window) {
  'use strict';

  function hasUnsavedDraft() {
    try {
      var order = window.currentOrder;
      if (!order || !Array.isArray(order.items) || order.items.length === 0) {
        return false;
      }
      // Draft not yet persisted to server
      if (!order.orderId) {
        return true;
      }
      // Local pending flag set by order operations when editing after load
      if (order.hasPendingLocalChanges === true) {
        return true;
      }
      return false;
    } catch (_) {
      return false;
    }
  }

  function resolveExitUrl(el) {
    if (!el) return null;
    var url = el.getAttribute('data-exit-url');
    if (url && url.trim()) return url.trim();
    return null;
  }

  function navigateTo(url) {
    if (!url) return;
    window.location.assign(url);
  }

  async function confirmIfDirty() {
    if (!hasUnsavedDraft()) {
      return { leave: true, save: false };
    }

    if (typeof Swal === 'undefined') {
      return {
        leave: window.confirm('Hay cambios sin guardar en el pedido. ¿Salir de todos modos?'),
        save: false,
      };
    }

    var result = await Swal.fire({
      icon: 'warning',
      title: 'Cambios sin guardar',
      text: 'Hay productos en el pedido que aún no se enviaron a cocina. ¿Qué deseas hacer?',
      showDenyButton: true,
      showCancelButton: true,
      confirmButtonText: 'Permanecer',
      denyButtonText: 'Salir sin guardar',
      cancelButtonText: 'Cancelar',
      reverseButtons: true,
      focusConfirm: true,
    });

    if (result.isConfirmed) {
      return { leave: false, save: false };
    }
    if (result.isDenied) {
      return { leave: true, save: false };
    }
    return { leave: false, save: false };
  }

  function confirmLeave(event, el) {
    var url = resolveExitUrl(el);
    if (!url) return true;

    if (!hasUnsavedDraft()) {
      return true; // allow default <a href> navigation
    }

    if (event && event.preventDefault) event.preventDefault();
    confirmIfDirty().then(function (decision) {
      if (decision.leave) navigateTo(url);
    });
    return false;
  }

  function goBack(el) {
    var url = resolveExitUrl(el);
    confirmIfDirty().then(function (decision) {
      if (!decision.leave) return;
      if (url) {
        navigateTo(url);
        return;
      }
      if (window.history.length > 1) {
        window.history.back();
      }
    });
  }

  function onBeforeUnload(e) {
    if (!hasUnsavedDraft()) return;
    e.preventDefault();
    e.returnValue = '';
  }

  window.addEventListener('beforeunload', onBeforeUnload);

  window.RestBarOrderNav = {
    hasUnsavedDraft: hasUnsavedDraft,
    confirmLeave: confirmLeave,
    goBack: goBack,
    confirmIfDirty: confirmIfDirty,
  };
})(window);
