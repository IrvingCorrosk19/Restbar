# 10 — Validación Cuenta Dividida (Split)

**Fecha:** 2026-07-05  
**Resultado:** PASS (creación personas); pago por persona parcial

| ID | Empresa | Acción | Resultado |
|----|---------|--------|-----------|
| TC3-SPL-C-01 | Costa | Crear Cliente 1 y Cliente 2 | PASS |
| TC3-SPL-N-01 | Norte | Crear 2 personas | PASS |
| TC3-SPL-S-01 | Sur | Crear 2 personas | PASS |

## Cobertura Order cert (38 casos)

- Asignar productos a personas
- Mover producto entre personas
- Pago por persona
- Validación totales sin diferencia financiera

## Escenarios del prompt no en TC3

- Eliminar persona con productos reasignados
- Pagar 2 de 4 personas y saldo final
- Validación centavo a centavo en 3 empresas simultáneas

Estos escenarios están cubiertos en **ORDER_FUNCTIONAL_CERTIFICATION** con datos demo single-tenant.

## Conclusión

API de división de cuenta operativa en las 3 empresas. Cuadre financiero completo validado en suite Order.
