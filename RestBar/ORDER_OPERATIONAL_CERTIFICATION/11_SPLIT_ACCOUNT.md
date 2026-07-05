# 11 — Split Account

| ID | Escenario | Resultado |
|----|-----------|-----------|
| OP-SPL-01 | Crear 3 comensales en orden | PASS |
| OP-SPL-02 | GetPersonsByOrder retorna 3 | PASS |

## API

- `POST /Person/CreatePerson`
- `GET /Person/GetPersonsByOrder?orderId=`

## Cuadre financiero

Pago parcial post-split validado (OP-PAY-01). Cuadre centavo a centavo cubierto en ORDER_FUNCTIONAL_CERTIFICATION S08/S09.
