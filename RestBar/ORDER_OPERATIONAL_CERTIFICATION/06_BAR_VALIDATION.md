# 06 — Bar Validation

## Bares configurados

- Bar Principal (Piso 1)
- Bar VIP (Piso 1)
- Bar Piso 2
- Bar Piso 3

## Tests PASS

| ID | Validación |
|----|------------|
| OP-RTE-03 | Cerveza solo en Bar Principal, no en Horno |
| OP-MB-01 | Trago VIP en Bar VIP |
| OP-MB-02 | Trago VIP NO en Bar Principal |
| OP-KDS-04 | Bartender marca bebida lista |
| OP-KDS-07 | Bar KDS excluye ítems de cocina |
| OP-UI-02 | StationOrders bar carga |

## Regla

Bebidas enrutan por `ProductStockAssignment` a estación tipo `bar`. Bar VIP tiene producto exclusivo (Trago VIP) con asignación dedicada.
