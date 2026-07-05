# 05 — Validación Sucursales / Pisos / Áreas / Mesas

**Fecha:** 2026-07-05  
**Resultado:** PASS

## Estructura por empresa

### Costa Centro
- 4 áreas (2 pisos × salón/terraza)
- 10 mesas C-01..C-10 rotando entre áreas
- Mesas aisladas de Norte (10) y Sur (15) — **TC3-MT-01/02 PASS**

### Norte Mall
- 2 áreas: Piso 1 Principal, Piso 2 VIP
- 10 mesas NM-01..NM-10

### Sur Hotel
- 3 áreas: Piso 1 Hotel, Piso 2 Hotel, Piso 3 Rooftop
- 15 mesas S-01..S-15

## Validaciones ejecutadas

| ID | Validación | Resultado |
|----|------------|-----------|
| TC3-MT-01 | Costa no comparte números de mesa con Norte | PASS |
| TC3-MT-02 | Costa no comparte números con Sur | PASS |
| TC3-MOV-C/N/S-01 | Cambio de mesa dentro de misma empresa/sucursal | PASS |
| TC3-MOV-SEC-01 | Orden Costa no puede moverse a mesa Norte | PASS (403) |

## Integridad orden-mesa

Al crear orden vía `SendToKitchen`, la orden queda ligada a `TableId` de la sucursal del mesero. El movimiento cross-company es rechazado.

## Conclusión

Jerarquía empresa → sucursal → área → mesa se respeta. No hay cruce de mesas entre empresas.
