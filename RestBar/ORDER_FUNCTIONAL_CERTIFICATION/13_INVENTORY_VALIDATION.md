# 13 — INVENTORY VALIDATION

## Modelo de descuento

| Nivel | Entidad | Uso |
|-------|---------|-----|
| Asignación producto→estación | `ProductStockAssignment` | Routing + stock por estación |
| Movimiento | `InventoryMovement` | Compras, ajustes, mermas |
| Transferencia | `StockTransfer` | Bar/cocina sin stock |
| Receta BOM | `Recipe` + `RecipeLine` | Explosión ingredientes al vender |

## Flujo al enviar cocina (`SendToKitchen`)

1. `CheckItemStockAvailability` valida stock asignado
2. `InventoryOperationsService.DeductInventoryForOrderItemAsync` descuenta
3. Si producto tiene receta → descuenta **ingredientes** (ENT-REC-01 PASS)
4. Si no tiene receta → descuenta **producto terminado**

## Pruebas ejecutadas

| ID | Caso | Resultado |
|----|------|-----------|
| S02-03 | CheckItemStockAvailability | PASS |
| ENT-INV-01 | Movimiento compra | PASS |
| ENT-INV-02 | Consulta movimientos por rango | PASS |
| ENT-XFER-01..03 | Transferencia solicitud/aprobación | PASS |
| ENT-REC-01..02 | Receta BOM guardar/consultar | PASS |

## Productos analizados (escenario 14-15)

| Producto | Descuenta | Evidencia |
|----------|-----------|-----------|
| Pizza Enterprise | Ingredientes vía BOM | ENT-REC |
| Hamburguesa | Estación Parrilla stock | RT-S01-02 |
| Cerveza | Bar Principal assignment | RT-S02-02 |
| Café Americano | Producto directo | POS-02 |
| Postre | Pastelería assignment | RT-S02-03 |

## Cancelación / restauración

`RestoreInventoryForCancelAsync` en `CancelOrderAsync` — ítems no cancelados restauran stock.

## Gaps

- Inventario por piso como almacén separado: modelado vía área, no validado E2E transferencia piso-a-piso
- Whisky/vino: mismo mecanismo bar; sin prueba específica de botella parcial

## Veredicto inventario

**PASS** para operación estándar con recetas, transferencias y descuento por estación.
