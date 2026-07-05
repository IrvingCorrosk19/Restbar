# 06 — Enrutamiento Estaciones / Cocina / Bar

**Fecha:** 2026-07-05  
**Resultado:** PASS

## Mecanismo de routing

RestBar enruta productos mediante **`ProductStockAssignment`** (producto → estación con prioridad). Al enviar a cocina, cada ítem recibe `preparedByStationId` según la asignación activa del producto en la sucursal.

## Validación TC3 (por empresa)

| ID | Empresa | Evidencia |
|----|---------|-----------|
| TC3-RTE-C-01 | Costa | `preparedByStationId` presente en ítem |
| TC3-RTE-N-01 | Norte | `preparedByStationId` presente |
| TC3-RTE-S-01 | Sur | `preparedByStationId` presente |

## Asignaciones del seeder

| Producto | Estación destino |
|----------|------------------|
| Hamburguesa | Parrilla (si existe) o Cocina principal |
| Pizza | Cocina |
| Postre | Cocina |
| Cerveza | Bar |
| Mojito | Bar |

## Suite complementaria — ORDER_ROUTING_CERTIFICATION

15/15 PASS — valida:
- Orden mixta (comida + bebida) se divide por estación
- Cocina no recibe bebidas; bar no recibe comida
- Producto sin estación → error claro
- Estación inactiva / otra sucursal → bloqueado

## Cocinas y bares por piso (Costa)

- Cocina Piso 1 / Bar Piso 1 → área Piso 1 Salón
- Cocina Piso 2 / Bar Piso 2 → área Piso 2 Salón

Routing por piso depende de asignación producto-estación del área/piso correspondiente (validado en routing cert con datos demo).

## Conclusión

El enrutamiento por estación funciona correctamente en las 3 empresas. No se envía la orden completa a todas las estaciones.
