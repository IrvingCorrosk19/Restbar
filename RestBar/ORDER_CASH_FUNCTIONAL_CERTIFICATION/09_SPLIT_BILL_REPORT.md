# 09 — Informe Split Bill / División de cuenta

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs relacionadas** | `Payments/payments.spec.js` (sin split dedicado) |

## Cobertura Playwright

| ID | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| — | Split bill UI / API | **NOT_COVERED** | **Ningún test ID** en `tests/Browser/` |
| PAY-01 | PaymentView page loads | **PASS** | Solo carga de vista |
| PAY-02 | partial API rejects empty order | **PASS** | API parcial negativa, no split |
| PAY-04 | send kitchen then payment summary | **PASS** | Summary endpoint, no división |

## Gap declarado

No hay flujo browser: crear pedido multi-item → dividir por comensales → pagar parcialmente → verificar saldos.

## Veredicto

**NOT_COVERED / PARTIAL** — pagos parciales vía API smoke únicamente; **split bill E2E no certificado**.
