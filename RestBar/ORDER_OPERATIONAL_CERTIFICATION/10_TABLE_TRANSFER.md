# 10 — Table Transfer

| ID | Escenario | Resultado |
|----|-----------|-----------|
| OP-MOV-01 | Mover orden con ítems parcialmente listos | PASS |

## Validación complementaria (Order cert)

- Mesa ocupada → rechazado (400)
- Cross-empresa → 403
- Orden activa en mesa destino post-move

## Nota operativa

Cambio de mesa **no re-rutea** automáticamente `PreparedByStationId` al cambiar de piso. Documentado como limitación conocida — requiere `UpdateItemStation` manual si aplica.
