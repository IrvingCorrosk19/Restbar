# 01 — Navegación pedidos: causa raíz

## Defecto
En el módulo POS de pedidos (`/Order/Index`), el usuario entra al flujo operativo y **no puede regresar** a la pantalla principal.

## Pantalla afectada
| Campo | Valor |
|-------|--------|
| URL | `/Order/Index` |
| Layout | `Views/Shared/_OrderLayout.cshtml` |
| Entrada | Home → card Pedidos / navbar Órdenes |
| Roles | OrderAccess (admin, manager, supervisor, waiter, cashier) |

## Causa raíz
`_OrderLayout` renderiza **solo título + body**, sin navbar del sistema y **sin botones Volver/Inicio**.

Evidencia previa:
- `_OrderLayout` body: container + `@RenderBody()` únicamente
- `_Layout` (sistema) no se usa en POS
- Contraste: KDS (`StationOrders` / `_KitchenLayout`) sí tenía enlace a Home (hardcoded)

## Qué NO era
- No hay loop de redirect
- No hay overlay bloqueante (`z-index: -1`, `pointer-events: none`)
- No es fallo de autorización al volver
- No depende de pedido abierto vs cerrado: el atrapamiento es del **chrome de layout**

## Contexto atrapado
Usuario permanece en SPA/AJAX de mesas-productos sobre la misma URL `/Order/Index` sin ruta de salida in-app.

## Severidad
**P0** — bloquea operación de turno (mesero no puede salir del POS).
