# 04 — Validación de Asignaciones

**Fecha:** 2026-07-05  
**Resultado:** PASS

## Meseros por área (Costa)

| Caso | Detalle | Resultado |
|------|---------|-----------|
| TC3-ASG-01 | mesero1 ve 3 mesas; mesero2 ve 2 mesas; overlap=0 | PASS |

**Configuración seeder:**
- `mesero1@costa.restbar.com` → área **Piso 1 Salón** (hasta 3 mesas asignadas)
- `mesero2@costa.restbar.com` → área **Piso 2 Salón**

Equivalente en Norte y Sur con áreas `Piso 1 Principal` / `Piso 2 VIP` y `Piso 1 Hotel` / `Piso 3 Rooftop`.

## Chef / Bartender por estación

Seeder asigna:
- Chef → primera estación cocina de la sucursal
- Bartender → primera estación bar

Vía `UserAssignment` con `StationId`.

## Escenarios no automatizados en TC3

| Escenario | Estado |
|-----------|--------|
| Mesero intenta abrir mesa no asignada | Cubierto indirectamente en Order cert (waiter table filter) |
| Chef Cocina Piso 1 vs Piso 2 | Cubierto en Routing cert (15 casos PASS) |
| Cambio de turno con órdenes abiertas | Manual / no en TC3 |

## Conclusión

Las asignaciones mesero-área respetan aislamiento entre meseros de la misma empresa. Aislamiento chef/bartender por estación validado en suite de routing.
