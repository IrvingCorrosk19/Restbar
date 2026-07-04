# Pruebas Funcionales E2E en Navegador

**Fecha:** 2026-07-04  
**Herramienta:** Cursor Browser (pestaña lateral)  
**URL:** `http://localhost:5001`  
**View ID:** `77e856`

## Resumen

| Resultado | Cantidad |
|-----------|----------|
| PASS | 10 |
| FAIL | 0 |
| **TOTAL** | **10** |

## Veredicto Browser E2E

```
BROWSER E2E: PASS
```

## Pruebas ejecutadas en UI real

| ID | Escenario | Resultado | Evidencia |
|----|-----------|-----------|-----------|
| B-E2E-01 | Login admin en formulario web | ✅ PASS | Dashboard tras submit |
| B-E2E-02 | Dashboard admin carga | ✅ PASS | Tarjetas Acciones Rápidas |
| B-E2E-03 | Pantalla Tomar Pedido (POS) | ✅ PASS | Mesas T-02..N-01, categorías |
| B-E2E-04 | Seleccionar Mesa N-01 | ✅ PASS | Estado Disponible → Ocupada |
| B-E2E-05 | Filtrar categoría Bebidas | ✅ PASS | Botón activo |
| B-E2E-06 | Auditoría admin | ✅ PASS | Tabla logs + filtros |
| B-E2E-07 | Cerrar sesión | ✅ PASS | Vuelve a Login |
| B-E2E-08 | Login mesero | ✅ PASS | Redirect automático a POS |
| B-E2E-09 | Mesero bloqueado en Company | ✅ PASS | Página Acceso Denegado |
| B-E2E-10 | KDS Cocina | ✅ PASS | Órdenes por mesa visibles |

## Flujos validados visualmente

1. **Formulario login** — campos email/password, botón "Iniciando sesión..."
2. **POS completo** — grid de mesas con estados (EnPreparacion, ParaPago, Disponible, Ocupada)
3. **Notificación toast** — "Mesa cambió a OCUPADA"
4. **Access Denied** — mensaje "No tienes permisos para acceder a esta página"
5. **KDS** — pedidos kitchen con botones "Marcar como listo", "Completar Orden"

## Complemento a suite API

Las pruebas PowerShell (`Run-FullCertification.ps1`, 43 casos) validan APIs y permisos HTTP.

Las pruebas browser (este documento, 10 casos) validan **interacción real del usuario** en la interfaz.

## Archivo CSV

`04_BROWSER_EXECUTED_TESTS.csv`

## Cómo reproducir

1. `dotnet run --urls "http://localhost:5001"`
2. Abrir `http://localhost:5001/Auth/Login` en browser
3. Ejecutar flujos manualmente o vía automatización browser MCP

## Pendiente browser (fase 3)

- Agregar producto al carrito y "Agregar a Cocina" click
- Modal pago parcial desde POS
- Login chef con redirect automático a KDS
- Inventarista pantalla inventario
- Reportes export CSV desde UI
