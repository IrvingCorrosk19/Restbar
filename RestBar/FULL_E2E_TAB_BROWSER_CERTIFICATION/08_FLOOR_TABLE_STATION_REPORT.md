# 08 — FLOOR / TABLE / STATION REPORT (Tab Browser)

**Dominio:** Areas, mesas, estaciones cocina/bar  
**Pack fecha:** 2026-08-01  
**Build:** Release deploy `871abc7`

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-FLR-01 | Pisos / áreas visibles por tenant | NOT STARTED (deep) | Seed ThreeCompaniesCertSeeder documentado; no caso Tab dedicado |
| E2E-FLR-02 | Mesa → apertura pedido | **PASS (indirecto)** | Cubierto vía E2E-POS-01 (`Evidence/POS/E2E-POS-01/order.png`) |
| E2E-FLR-03 | Estaciones kitchen/bar asignadas | **PASS (indirecto)** | Cubierto vía E2E-POS-02 KDS contexts |
| E2E-FLR-04 | CRUD Area/Table/Station UI | NOT STARTED | No ejecutado en este pack |
| E2E-POS-03 | MoveToTable | NOT STARTED | En plan maestro; no corrido |

## Gaps vs mandato

- CRUD de pisos/mesas/estaciones no certificado en browser en este pack  
- Unión de mesas: NOT IMPLEMENTED (ver POS report)  
- Aislamiento de layout entre tenants: parcial vía MT seeds, no deep SQL/UI

**Veredicto dominio Floor/Table/Station:** IN PROGRESS (uso POS/KDS únicamente; CRUD NOT STARTED).
