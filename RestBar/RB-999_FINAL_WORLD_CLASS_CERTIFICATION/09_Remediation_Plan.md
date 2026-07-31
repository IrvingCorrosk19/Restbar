# 09 — Remediation Plan (Commercial ROI Only)

No desarrollar por cantidad. Solo ítems con retorno de venta.

| # | Tarea | Impacto comercial | Costo técnico | Riesgo | Tiempo | Deps | Beneficio |
|---|-------|-------------------|---------------|--------|--------|------|-----------|
| 1 | Definir oferta comercial (SKU, precio, contrato, SLA piloto) | Crítico | Bajo (legal/biz) | Bajo | 1–2 sem | — | Desbloquea “comprar” |
| 2 | Partner de implementación + runbook onboarding | Crítico | Medio | Bajo | 2–4 sem | RB-026 guides | Cierra journey 3–5 |
| 3 | Wizard onboarding (company→branch→floor→products→register) | Alto | Medio | Medio | 2–3 sem | Admin APIs | Reduce time-to-value |
| 4 | Integración procesador pagos (1 partner LATAM) | Crítico | Alto | Alto | 6–12 sem | PCI scope | Paridad venta vs Square/Toast |
| 5 | Estrategia offline (cola local / PWA) o requisito contractual de red | Crítico | Alto | Alto | 8–16 sem | Arquitectura | Quita deal-breaker |
| 6 | Cerrar CSRF JSON + rotar secretos | Alto confianza | Medio | Medio | 1–2 sem | RB-026 | Pasa security review |
| 7 | Cron backup + drill restore trimestral | Alto ops | Bajo | Bajo | 3–5 días | Scripts existentes | Confianza continuidad |
| 8 | Tender UI pago completo en POS (evidencia demo) | Alto demo | Medio | Medio | 2–4 sem | Payment API | Demo más creíble |
| 9 | App móvil nativa | Medio-Alto | Muy alto | Alto | 3–6 meses | — | **Diferir** hasta 4–5 |
| 10 | Loyalty / reservas / hotel | Bajo ahora | Alto | — | — | — | **No hacer** |

## Orden recomendado

`1 → 2 → 7 → 6 → 3 → 8 → 4 → 5` · Diferir 9–10.
