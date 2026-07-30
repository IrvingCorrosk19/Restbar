# 02 — Fix de navegación pedidos

## Solución
Chrome sticky en `_OrderLayout` con:

- **Volver** (`data-testid=order-nav-back`)
- **Inicio** (`data-testid=order-nav-home`)
- URL de salida vía `NavigationHelper.ResolveSafeReturnUrl` + `ViewBag.SafeReturnUrl`
- Tag helpers / `Url.Action` (sin hardcode de host)
- Rechazo de open-redirect (`https://evil…` → fallback Home)
- Evita returnUrl que apunte de nuevo a `/Order` o StationOrders

## Dirty-state
`wwwroot/js/order/order-navigation.js`:

- Detecta borrador local (`currentOrder.items` sin `orderId` o `hasPendingLocalChanges`)
- Confirma con SweetAlert antes de salir
- `beforeunload` en refresh/cerrar pestaña si hay borrador

## Archivos
- `Helpers/NavigationHelper.cs`
- `Controllers/OrderController.cs` (Index / StationOrders + returnUrl)
- `Views/Shared/_OrderLayout.cshtml`
- `Views/Shared/_KitchenLayout.cshtml`
- `Views/Order/StationOrders.cshtml`
- `Views/Home/Index.cshtml` / `_Layout.cshtml` (returnUrl de entrada)
- `wwwroot/js/order/order-navigation.js`
- `tests/Browser/Orders/orders-navigation.spec.js`
