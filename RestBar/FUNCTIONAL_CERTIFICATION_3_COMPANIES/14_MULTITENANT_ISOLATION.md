# 14 — Aislamiento Multitenant

**Fecha:** 2026-07-05  
**Resultado:** PASS

| ID | Validación | Resultado |
|----|------------|-----------|
| TC3-MT-01 | Mesas Costa ≠ Norte | PASS (10 vs 10, sin overlap) |
| TC3-MT-02 | Mesas Costa ≠ Sur | PASS (sur=15) |
| TC3-MT-03 | Costa ve producto exclusivo propio | PASS |
| TC3-MT-04 | Norte NO ve exclusivo Costa | PASS |
| TC3-MT-05 | Sur NO ve exclusivo Norte | PASS |
| TC3-SEC-01 | Norte no lee pago orden Costa | PASS (403) |
| TC3-SEC-02 | Norte no cancela orden Costa | PASS (403) |
| TC3-SEC-03 | Norte no lee orden activa mesa Costa | PASS |
| TC3-MOV-SEC-01 | No mover orden a mesa otra empresa | PASS (403) |

## Vectores probados

- Listado mesas activas
- Catálogo productos por categoría
- Órdenes y pagos (IDOR)
- Cambio de mesa cross-company

## Vectores no probados en TC3

- Inventario cross-branch manual
- Reportes agregados cross-company
- Auditoría cross-company
- SignalR cross-tenant (ver doc 13)

## Conclusión

**No se detectaron fugas de datos entre las 3 empresas** en los vectores automatizados. Defecto crítico multitenant: **ninguno abierto**.
