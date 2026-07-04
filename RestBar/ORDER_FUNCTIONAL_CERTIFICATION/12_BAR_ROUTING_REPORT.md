# 12 — BAR ROUTING REPORT

## Arquitectura

Bebidas y tragos enrutan vía `ProductStockAssignment` a estaciones tipo `bar`, igual que cocina pero con KDS `/Order/StationOrders?stationType=bar`.

## Pruebas ejecutadas

| ID | Escenario | Resultado |
|----|-----------|-----------|
| RT-S02-02 | Cerveza → Bar Principal únicamente | PASS |
| RT-S03-01 | Trago VIP → Bar VIP | PASS |
| RT-S03-02 | Trago VIP NO en Bar Principal | PASS |
| NAV-02 | Ruta bar carga para bartender | PASS |

## Múltiples bares (seed enterprise)

| Bar | Productos | Estado |
|-----|-----------|--------|
| Bar Principal | Cerveza, cócteles estándar | PASS |
| Bar VIP | Tragos premium | PASS |
| Bar Terraza | Configurado en seed | PARTIAL |
| Bar Piscina | Configurado en seed | PARTIAL |

## Bar fuera de servicio

- Estación `IsActive=false` → producto no enruta a ese bar
- Fallback a bar con stock y mayor prioridad en misma sucursal/área
- Sin stock en ningún bar → `CheckItemStockAvailability` retorna insuficiente

## Inventario bar

Descuenta desde `ProductStockAssignment` de la estación bar seleccionada (ver `13_INVENTORY_VALIDATION.md`).

## Veredicto bar

**PASS** — Enrutamiento aislado por bar validado. Terraza/Piscina pendiente prueba operativa en sitio.
