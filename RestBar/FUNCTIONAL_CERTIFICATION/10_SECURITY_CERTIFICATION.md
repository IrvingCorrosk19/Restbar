# Certificación de Seguridad

**Fecha:** 2026-07-04  
**Resultado:** ✅ PASS (alcance ejecutado)

## Controles validados

### Autorización por rol

| Test | Rol | Recurso | Esperado | Resultado |
|------|-----|---------|----------|-----------|
| SEC-01 | waiter | /Company/Index | Denegado | ✅ |
| SEC-02 | cashier | /Product/Index | Denegado | ✅ |
| SEC-03 | waiter | /Audit/Index | Denegado | ✅ |
| SEC-04 | admin | /Audit/Index | Permitido | ✅ |
| SEC-05 | inventarista | /Inventory/Index | Permitido | ✅ |
| SEC-06 | superadmin | /SuperAdmin/Index | Permitido | ✅ |
| RPT-02 | waiter | /Reports/Index | Denegado | ✅ |

### IDOR (Insecure Direct Object Reference)

| Test | Descripción | Resultado |
|------|-------------|-----------|
| MT-03 | Consulta pago orden otra empresa | 403 ✅ |
| PAY partial | Guard branch en POST | Implementado ✅ |

### Autenticación

| Test | Resultado |
|------|-----------|
| Credenciales inválidas | Rechazadas ✅ |
| 12 roles válidos | Login OK ✅ |

### SignalR

- `OrderHub` requiere `[Authorize]` — implementado

### Hardening DB

- `idempotency_key` en pagos — índice único parcial
- `idx_unique_active_order_per_table` — previene órdenes duplicadas
- Enum `inventarista` en PostgreSQL

## Pendiente fase 2

- Manipulación manual de GUIDs en URL (fuzzing)
- Modificación de claims en cookie
- CSRF en endpoints sin token
- Rate limiting en producción
- Penetration test externo

## Veredicto seguridad

**PASS** — Controles de autorización e IDOR funcionan en escenarios probados. Sin vulnerabilidades críticas abiertas detectadas.
