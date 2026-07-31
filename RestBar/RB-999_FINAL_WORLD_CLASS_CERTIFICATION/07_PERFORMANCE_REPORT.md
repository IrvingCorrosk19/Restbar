# 07 — Performance Report

| Objetivo | Evidencia | Estado |
|----------|-----------|--------|
| Cockpit / analytics percibido &lt; 3–5s | PERF Playwright soft budget 5s | PARCIAL |
| P95 consultas críticas &lt; 2s | Explain indexes RB-025; no lab formal 2026-07-31 | PARCIAL |
| Analytics no degrada POS | Lecturas + jobs ligeros; sin evidencia de contención | DISEÑO OK |
| Export asíncrono volumen alto | No implementado universalmente | GAP |
| Forecast pesado off-request | Persist run sync ligero | OK piloto |

**Conclusión:** Aceptable para piloto multi-sucursal pequeña/mediana. **No** certificado para picos Toast-scale.
