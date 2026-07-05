# 09 — Validación Cambio de Mesa

**Fecha:** 2026-07-05  
**Resultado:** PASS

| ID | Escenario | Resultado |
|----|-----------|-----------|
| TC3-MOV-C-01 | Costa: mover orden a mesa libre misma sucursal | PASS |
| TC3-MOV-N-01 | Norte: mover orden | PASS |
| TC3-MOV-S-01 | Sur: mover orden | PASS |
| TC3-MOV-SEC-01 | Costa orden → mesa Norte (otra empresa) | PASS (403) |

## Validaciones adicionales (Order cert)

- Cambio a mesa ocupada → rechazado
- Cambio después de enviar cocina → permitido con estados consistentes
- Mesa origen/destino actualizadas correctamente

## Conclusión

El cambio de mesa respeta límites de empresa/sucursal. Movimientos cross-company bloqueados.
