# Escenarios de Prueba — Certificación Funcional

## 1. Autenticación y sesión

| ID | Escenario | Prioridad |
|----|-----------|-----------|
| AUTH-01 | Credenciales inválidas rechazadas | Alta |
| AUTH-02..12 | Login exitoso por cada rol | Alta |
| AUTH-S01 | Usuario inactivo bloqueado | Media |
| AUTH-S02 | Cerrar sesión | Media |
| AUTH-S03 | Sesión expirada (8h) | Baja |
| AUTH-S04 | Dos navegadores misma cuenta | Media |

## 2. Multi-tenant

| ID | Escenario | Prioridad |
|----|-----------|-----------|
| MT-01 | Empresa B ve solo sus productos | Crítica |
| MT-02 | Empresa A no ve productos de B | Crítica |
| MT-03 | IDOR: pago de orden ajena → 403 | Crítica |
| MT-04 | Usuarios aislados por sucursal | Alta |
| MT-05 | Configuraciones no cruzadas | Alta |

## 3. Permisos por rol

| ID | Escenario | Prioridad |
|----|-----------|-----------|
| SEC-01 | Mesero sin acceso Company | Alta |
| SEC-02 | Cajero sin acceso Product | Alta |
| SEC-03 | Mesero sin acceso Audit | Alta |
| SEC-04 | Admin acceso Audit | Alta |
| SEC-05 | Inventarista acceso Inventory | Alta |
| SEC-06 | SuperAdmin panel global | Alta |

## 4. POS

| ID | Escenario | Prioridad |
|----|-----------|-----------|
| POS-01 | Listar mesas activas | Alta |
| POS-02 | Crear orden y enviar a cocina | Crítica |
| POS-03 | Consultar orden activa por mesa | Alta |
| POS-04 | Mesa ocupada vs disponible | Media |
| POS-05 | Mover pedido entre mesas | Media |
| POS-06 | Cancelar ítem pre-cocina | Alta |
| POS-07 | Dos órdenes misma mesa (bloqueado) | Alta |

## 5. Pagos

| ID | Escenario | Prioridad |
|----|-----------|-----------|
| PAY-01 | Pago parcial efectivo | Crítica |
| PAY-02 | Resumen de pagos por orden | Alta |
| PAY-03 | Idempotencia anti-duplicado | Crítica |
| PAY-04 | Pago total y cierre orden | Alta |
| PAY-05 | Sobrepago rechazado | Alta |
| PAY-06 | Split bill | Media |

## 6. Cocina (KDS)

| ID | Escenario | Prioridad |
|----|-----------|-----------|
| NAV-01 | Chef accede pantalla cocina | Alta |
| KDS-01 | Orden aparece en KDS tras SendToKitchen | Alta |
| KDS-02 | Marcar ítem listo | Media |
| KDS-03 | SignalR actualización tiempo real | Media |

## 7. Reportes

| ID | Escenario | Prioridad |
|----|-----------|-----------|
| RPT-01 | Contador accede reportes | Alta |
| RPT-02 | Mesero bloqueado en reportes | Alta |

## Estado de ejecución (fase actual)

✅ Ejecutados y aprobados: AUTH-01..13, SEC-01..10, MT-01..05, POS-01..03, PAY-01..05, NAV-01..02, RPT-01..03

⏳ Pendientes fase 2: resto de escenarios listados
