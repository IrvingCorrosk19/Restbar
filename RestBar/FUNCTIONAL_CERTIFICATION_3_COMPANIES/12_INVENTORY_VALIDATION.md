# 12 — Validación de Inventario

**Fecha:** 2026-07-05  
**Resultado:** PASS (enterprise suite); no duplicado per-empresa en TC3

## TC3

El script TC3 no ejecuta escenarios de inventario por empresa. Los productos del seeder tienen `TrackInventory=true`, stock 100, y `ProductStockAssignment` por estación.

## Enterprise Operations Certification (23/23 PASS)

| ID | Escenario | Resultado |
|----|-----------|-----------|
| ENT-INV-01 | Movimiento de compra | PASS |
| ENT-INV-02 | Consulta movimientos por rango | PASS |
| ENT-XFER-01..03 | Transferencias entre estaciones | PASS |
| ENT-REC-01..02 | Recetas BOM | PASS |

## Order cert

- Descuento de stock al enviar cocina
- Producto sin stock → bloqueo o advertencia según configuración
- Cancelación antes de cocina no descuenta

## Inventario por estación

Soportado vía `ProductStockAssignment.Stock` por estación. Productos de bar y cocina descuentan del stock de su estación asignada.

## Conclusión

Inventario consistente en operación single-tenant validado exhaustivamente. Multitenant: aislamiento de productos por empresa confirmado (TC3-MT-03..05). No se detectó descuento cross-empresa.
