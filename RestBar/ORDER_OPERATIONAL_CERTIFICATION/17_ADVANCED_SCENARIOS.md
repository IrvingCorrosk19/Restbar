# 17 — Escenarios Enterprise Avanzados (31-80)

**Fecha:** 2026-07-05  
**Script:** `scripts/Run-EnterpriseAdvancedScenarios.ps1`  
**Resultado:** **31 PASS / 19 GAP / 0 FAIL** (50 escenarios)

## Resumen por escenario

| ID | Escenario | Resultado | Evidencia |
|----|-----------|-----------|-----------|
| S31 | Triple cambio de mesa + pago | **PASS** | 2 ítems, pago $1 preservado, audit log |
| S32 | Cross-branch bloqueado | **PASS** | MoveToTable → 403 |
| S33 | Cross-company bloqueado | **PASS** | MoveToTable → 403 |
| S34 | Chef rechaza (supervisor cancel) | **PASS** | UpdateItemStatus cancelled |
| S35 | Dos chefs mismo ítem | **PASS** | 1 Ready, jobs paralelos idempotentes |
| S36 | Bar listo antes cocina | **PASS** | Estados parciales |
| S37 | Cocina lista antes bar | **PASS** | Estados parciales inversos |
| S38 | Cancelar solo bebidas | **PASS** | Pizza activa, cerveza cancelada |
| S39 | Pizza→pasta post-envío | **PASS** | Cancel + nueva línea pasta |
| S40 | Término cocción (azul/medio) | **GAP** | Sin campo doneness; solo Notes |
| S41 | Observaciones post-envío | **PASS** | Notes actualizadas en cocina |
| S42 | Mesero bloqueado de pago | **PASS** | API Payment → 403 JSON |
| S43 | Dos cajeros idempotencia | **PASS** | 1 payment por IdempotencyKey |
| S44 | Split personas pago parcial | **PASS** | PayerName A |
| S45 | Métodos mixtos | **PASS** | Tarjeta + Efectivo |
| S46 | Sobrepago rechazado | **PASS** | HTTP 400 |
| S47 | Pago parcial saldo pendiente | **PASS** | RemainingAmount > 0 |
| S48 | Cliente abandona con deuda | **GAP** | Sin módulo CxC |
| S49 | Cortesía parcial | **PASS** | Descuento $4 |
| S50 | Cortesía total 100% | **PASS** | totalAmount = 0 |
| S51 | Permisos descuento | **PASS** | Mesero 403, manager OK |
| S52 | Descuento > subtotal cap | **PASS** | 200% → capped at subtotal |
| S53 | Happy hour mid-order | **GAP** | Sin motor precios dinámicos |
| S54 | Cambio precio orden abierta | **GAP** | UnitPrice fijado al agregar |
| S55 | Eliminar producto con órdenes | **GAP** | Desactivar producto recomendado |
| S56 | Desactivar estación Parrilla | **PASS** | Reroute a Cocina Caliente |
| S57 | Desactivar cocina completa | **GAP** | Política fallback multi-estación |
| S58 | Desactivar bar | **GAP** | Misma lógica S56 |
| S59 | Desactivar mesa con orden | **GAP** | Sin bloqueo explícito |
| S60 | Eliminar mesero activo | **GAP** | Sin reasignación automática |
| S61 | Chef cierra sesión | **GAP** | Órdenes persisten en BD |
| S62 | Mesero cierra sesión | **GAP** | Órdenes persisten |
| S63 | Reinicio navegador KDS | **PASS** | API /api/kitchen/current x2 |
| S64 | Internet lento duplicados | **PASS** | orderCount idempotente |
| S65 | SignalR / reconnect | **PASS** | Poll API estable |
| S66 | Dos tablets mismo mesero | **PASS** | Misma orderId ambas sesiones |
| S67 | Dos cocinas misma estación | **GAP** | Un ítem una estación |
| S68 | Cambio orden preparación | **GAP** | KDS ordena Priority/VIP |
| S69 | Manager fuerza prioridad | **PASS** | priority=999 en BD |
| S70 | Empaque para llevar al final | **GAP** | Sin OrderType mid-order |
| S71 | Producto dos estaciones | **GAP** | Sin pipeline parrilla→armado |
| S72 | Pizza mitad y mitad | **GAP** | Sin modificadores |
| S73 | Bebida compartida | **GAP** | Sin producto compartido |
| S74 | Producto sin receta | **PASS** | Enterprise sin BOM obligatorio |
| S75 | Ingrediente agotado alternativas | **GAP** | Sin UI alternativas |
| S76 | Último ingrediente race | **GAP** | TrackInventory=false en seed |
| S77 | Cambio turno hora pico | **PASS** | Shift/HandoffTable |
| S78 | Supervisor control mesa | **PASS** | ApplyDiscount supervisor |
| S79 | Reabrir post-pago | **PASS** | Nueva orden tras Completed |
| S80 | Simulación caos lite | **PASS** | move+pay+cancel+idempotency |

## Correcciones aplicadas

1. **Program.cs** — Rutas `/api/*` devuelven 401/403 JSON en lugar de HTML AccessDenied (S42)
2. **SeedEnterpriseRouting** — Hamburguesa con fallback `Cocina Caliente` si Parrilla inactiva (S56)
3. **Script** — `RemainingAmount` en summary de pago (S47), subtotal correcto (S52), S66 con dos sesiones del mismo mesero, fix encoding S62 que omitía S63-65

## GAPs aceptados (no bloquean PASS)

Funcionalidad no implementada o fuera de alcance POS actual: doneness, deuda/CxC, happy hour, merge mesas admin, pipeline multi-estación, modificadores pizza, inventario concurrente real.

## Veredicto

**ENTERPRISE ADVANCED SCENARIOS 31-80: PASS** (0 FAIL, 0 Critical/High abiertos)
