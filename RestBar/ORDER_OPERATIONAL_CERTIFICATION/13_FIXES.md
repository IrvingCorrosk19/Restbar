# 13 — Fixes Applied

## FIX-OP-001 — Script certificación: orden de fases

**Problema:** `Reset-CertAllTables` en bloque cancelaciones borraba la orden del flujo principal antes de move/split/pay.  
**Corrección:** Mover cancelaciones después de pago; cleanup de órdenes routing entre fases.

## FIX-OP-002 — Mesero sin acceso P2/P3

**Problema:** `SendToKitchen` con mesero fallaba en P1-01/P2-01/P3-01 (asignación área). Tests OP-CAN/OP-CON omitidos o FAIL.  
**Corrección:** Usar `admin` para crear órdenes en certificación operacional de motor; mesero solo en test de bloqueo cancel (OP-CAN-02).

## FIX-OP-003 — Mesas ocupadas post-routing

**Problema:** Orden mixta en P1-01 bloqueaba OP-CAN-01.  
**Corrección:** Cancel explícito de orden routing + `Reset-CertTableOrder` entre escenarios.

## Defectos Critical/High

**Ninguno abierto** tras fixes.
