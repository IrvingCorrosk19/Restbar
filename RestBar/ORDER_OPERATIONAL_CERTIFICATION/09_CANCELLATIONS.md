# 09 — Cancellations

| ID | Escenario | Resultado |
|----|-----------|-----------|
| OP-CAN-01 | Cancelar orden completa antes de pago → mesa libre | PASS |
| OP-CAN-02 | Mesero bloqueado cancelar ítem en cocina | PASS (403) |
| OP-CAN-03 | Supervisor puede cancelar ítem en cocina | PASS |

## Reglas operativas

- **Antes de cocina:** admin/manager puede cancelar orden completa
- **En cocina:** mesero → 403; supervisor/manager → permitido
- Inventario: productos enterprise con `TrackInventory=false` en seed; restauración validada en ENTERPRISE cert

## Estados post-cancelación

Mesa vuelve disponible (OP-CAN-01). Ítem cancelado por supervisor removido del flujo KDS activo.
