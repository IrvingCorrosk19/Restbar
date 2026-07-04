# Certificación de Pagos

**Fecha:** 2026-07-04  
**Resultado:** ✅ PASS

## Pruebas ejecutadas

| ID | Caso | Resultado |
|----|------|-----------|
| PAY-01 | Pago parcial $1.00 efectivo | ✅ PASS |
| PAY-02 | Resumen de pagos por orden | ✅ PASS |
| PAY-03 | Idempotencia (misma key 2x) | ✅ PASS |
| MT-03 | IDOR cross-tenant → 403 | ✅ PASS |

## Flujo validado

1. Orden activa con saldo pendiente
2. Cajero registra pago parcial con `IdempotencyKey` UUID
3. Sistema retorna `{ success: true, isFullyPaid: false }`
4. Resumen accesible para cajero de misma sucursal
5. Reintento con misma key no duplica cobro
6. Admin Empresa B recibe 403 al consultar orden Empresa A

## Controles financieros

| Control | Estado |
|---------|--------|
| Idempotency key (DB + EF) | ✅ |
| Advisory lock PostgreSQL | ✅ Implementado |
| Guard branch IDOR POST | ✅ |
| Guard branch IDOR GET summary | ✅ Corregido |
| Rechazo sobrepago | ✅ Implementado en código |
| Pagos a orden cancelada | ✅ Bloqueado |

## Pago de prueba registrado

- **PaymentId:** `b56e474b-d815-454c-80cb-729cb5fe8809`
- **Monto:** $1.00
- **Método:** Efectivo

| PAY-04 | Sobrepago rechazado | ✅ PASS |
| PAY-05 | Orden inexistente 404 | ✅ PASS |
- Pago mixto (efectivo + tarjeta)
- Split bill / cuentas separadas
- Void / reversión de pago
- Dos cajeros simultáneos misma orden
- Propina y descuentos

## Veredicto pagos

**PASS** — Pagos parciales, idempotencia y aislamiento tenant funcionan correctamente.
