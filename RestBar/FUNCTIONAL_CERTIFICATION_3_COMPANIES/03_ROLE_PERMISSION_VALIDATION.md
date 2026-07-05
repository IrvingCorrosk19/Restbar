# 03 — Validación de Roles y Permisos

**Fecha:** 2026-07-05  
**Resultado:** PASS (5/5 casos TC3-ROL)

| ID | Rol probado | Acción | Esperado | Resultado |
|----|-------------|--------|----------|-----------|
| TC3-ROL-01 | Mesero (Costa) | Acceder `/Company/Index` | Denegado | PASS |
| TC3-ROL-02 | Chef (Costa) | Acceder `/Reports/Index` | Denegado | PASS |
| TC3-ROL-03 | Mesero (Costa) | Acceder `/Order/Index` | Permitido | PASS |
| TC3-ROL-04 | Manager (Costa) | Acceder `/Reports/Index` | Permitido | PASS |
| TC3-ROL-05 | SuperAdmin | Acceder `/SuperAdmin/Index` | Permitido | PASS |

## Cobertura adicional (suites previas)

- **Enterprise ENT-CANC-01/02:** Mesero bloqueado cancelar ítem en cocina (403); supervisor puede cancelar.
- **Order cert:** `ApplyDiscount` restringido a admin/manager/supervisor.

## Roles no probados exhaustivamente en TC3

| Rol | Estado | Notas |
|-----|--------|-------|
| Cajero | Parcial | Pagos parciales en flujo orden; rutas admin no probadas en TC3 |
| Bartender | No UI en TC3 | Asignación a estación en seeder; pantalla cocina/bar en enterprise |
| Admin | Parcial | Seed, mesas, productos vía API; no CRUD completo en TC3 |

## Conclusión

Los controles de acceso por rol funcionan para los escenarios críticos de la certificación 3 empresas. No se detectaron fugas de permisos en rutas probadas.
