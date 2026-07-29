# 11 — ROI MATRIX

Estimaciones de esfuerzo en **semanas-dev** (1–2 engineers). Costos en **USD** a $4k/sem-equipo promedio LATAM senior mix. Beneficio cliente = restaurante mediano full-service.

Escala prioridad: **P0 > P1 > P2 > P3**  
ROI: Alto / Medio / Bajo (cualitativo + payback)

---

| ID | Funcionalidad | Sem | Costo est. | Complejidad | Riesgo | Beneficio cliente | Beneficio $ cliente/año | Beneficio comercial RestBar | Beneficio competitivo | Pri | ROI |
|----|---------------|-----|------------|-------------|--------|-------------------|-------------------------|----------------------------|-----------------------|-----|-----|
| H1–H4 | Harden UI/stubs | 2 | 8k | Baja | Bajo | Confianza | Evita churn | Credibilidad | Paridad higiene | P0 | **Muy Alto** |
| E1 | Caja/arqueo | 7 | 28k | Alta | Medio | Control/fraude | 2–8k | Desbloquea venta | Table stake | P0 | **Muy Alto** |
| E2 | Precuenta+print | 5 | 20k | Media | Bajo | Rotación/CX | 8–25k | Demo wow | Table stake | P0 | **Muy Alto** |
| E3 | Export real | 3.5 | 14k | Media | Bajo | Tiempo gerencia | 2–5k | Cierra objeción | Paridad | P0 | **Alto** |
| E4 | Cierre Z | 3.5 | 14k | Media | Medio | Ritual cierre | 1–3k | Ops serio | Paridad | P0 | **Alto** |
| E5 | Shift UI | 2.5 | 10k | Baja | Bajo | Handoff | 1–2k | Completitud | Menor | P0 | **Alto** |
| E6 | Suppliers | 3.5 | 14k | Media | Bajo | Base compras | Enabler | Demo compras | Gap→paridad | P0 | **Alto** |
| E7 | PO+recepción | 9 | 36k | Alta | Medio | Food cost | 15–30k | Plan Business | vs R365/MM | P0 | **Muy Alto** |
| E8 | Recipe UI+cost | 5 | 20k | Media | Medio | Margen plato | 5–15k | CFO story | Paridad | P0 | **Alto** |
| E9 | Merma | 2.5 | 10k | Baja | Bajo | Desperdicio | 3–8k | Ahorro story | Paridad | P1 | **Alto** |
| E10 | FC variance | 3.5 | 14k | Media | Medio | Control costo | 5–10k | Diferencia mid | vs MarginEdge | P1 | **Alto** |
| E11 | Supplier report | 2 | 8k | Baja | Bajo | Decisión compra | 1–3k | Quita stub | Higiene | P1 | **Alto** |
| E12 | Combos | 5 | 20k | Media | Medio | Ticket | 8–20k | Ingresos | Paridad Toast | P1 | **Alto** |
| E13 | HH/Promo | 5 | 20k | Media | Medio | Bar revenue | 5–15k | Ingresos | Paridad | P1 | **Alto** |
| E14 | Fiscal país | 10 | 40k | Muy alta | Alto | Legal | Evita multa | Abre mercado | Debe tener | P1 | **Alto** |
| E15 | Loyalty básico | 4.5 | 18k | Media | Medio | Recompra | 5–12k | Stickiness | Paridad Square | P2 | **Medio** |
| E16 | Upsell hints | 2.5 | 10k | Baja | Bajo | Ticket | 3–8k | Nice lift | Diferencia UX | P2 | **Medio-Alto** |
| E17 | Command Center | 7 | 28k | Alta | Medio | Decisiones | 5–10k* | Enterprise feel | Diferencia | P1 | **Alto** |
| E18 | BI schema/jobs | 7 | 28k | Alta | Medio | Base intel | Enabler | Escala | Base | P2 | **Alto** |
| E19 | Forecast+reorder | 7 | 28k | Alta | Medio | Auto compras | 5–12k | Automatiza | vs MarketMan | P2 | **Alto** |
| E20 | Alert engine | 3.5 | 14k | Media | Bajo | Proactivo | 2–5k | Ops | Paridad | P2 | **Alto** |
| E21 | SaaS billing | 12 | 48k | Alta | Medio | — (RestBar) | ARPU scale | **Crítico SaaS** | Square-like | P2 | **Muy Alto** (vendor) |
| E22 | Backup real | 2 | 8k | Baja | Bajo | Continuidad | Riesgo↓ | Enterprise | Higiene | P2 | **Medio** |
| E23 | Copilot | 10 | 40k | Muy alta | Alto | Decisiones | 5–15k | Premium ARPU | Diferencia LATAM | P3 | **Medio-Alto** |
| E24 | Labor light | 8 | 32k | Alta | Medio | Prime cost | 8–20k | Upsell | vs 7Shifts | P3 | **Medio** |
| E25 | Delivery hub | 8 | 32k | Alta | Alto | Canal | Variable | Completitud | vs Olo | P3 | **Medio** |
| E26 | Franchise pack | 8 | 32k | Alta | Medio | Escala red | HQ value | Segmento nuevo | Diferencia | P3 | **Medio-Alto** |
| E27 | Offline POS | 10 | 40k | Muy alta | Alto | Continuidad | Riesgo↓ | Verticales | vs NCR | P3 | **Medio** |
| E28 | Contabilidad API | 6 | 24k | Media | Medio | Cierre contable | 2–5k | Enterprise | Paridad | P3 | **Medio** |

\*Valor Command Center es multiplicador de adopción/retención más que caja directa.

---

# Resumen inversión por fase

| Fase | Inversión est. | Payback cliente | Payback RestBar |
|------|----------------|-----------------|-----------------|
| F0 | ~8k | Inmediato (confianza) | Inmediato |
| F1 | ~86k | 3–6 meses | Desbloquea pilots pagos |
| F2 | ~102k | 4–8 meses | Plan Business |
| F3 | ~108k | 4–9 meses | Mercado regulado + ARPU |
| F4 | ~126k | 6–12 meses | SaaS scale |
| F5 | ~176k | 9–18 meses | Premium / enterprise |

**Total 24m (orden magnitud):** ~$600k desarrollo producto (equipo lean).  
**No incluye** GTM, soporte, infra.

---

# Orden de máximo ROI (top 10)

1. Harden stubs  
2. Caja  
3. Precuenta  
4. PO+recepción  
5. Export  
6. Recipe food cost  
7. Combos  
8. HH/Promo  
9. Command Center  
10. SaaS billing (ROI vendor)
