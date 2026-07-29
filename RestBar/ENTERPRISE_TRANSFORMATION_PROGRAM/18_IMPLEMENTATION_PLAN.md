# 18 — IMPLEMENTATION PLAN

Plan de ejecución **sin código en este documento**. Secuencia para el equipo.

---

# 1. Modelo de equipo sugerido

| Rol | FTE |
|-----|-----|
| Tech lead / arquitectura | 0.5–1 |
| Backend | 1–2 |
| Frontend Razor/JS | 1 |
| QA / certificación | 0.5–1 |
| Product / PO | 0.5 |
| CS piloto | 0.5 |

---

# 2. Cadencia

- Sprints 2 semanas  
- Cada epic: diseño ADR corto → implement → cert script → feature flag on  
- Definition of Done incluye: tenant tests + “¿genera valor dinero?” check  

---

# 3. Wave plan (primeros 6 meses)

### Wave 0 (2 sem) — Harden
H1–H4 · Release notes honestos · Demo script actualizado

### Wave 1 (8–10 sem) — Cash + Prebill
E1, E2, E5 · Cert cash scenarios · UAT 1 local

### Wave 2 (4 sem) — Close & Export
E3, E4 · Z-report · Excel ventas/inventario

### Wave 3 (12 sem) — Purchasing core
E6, E7 · Recepción → stock cost · MT tests PO

### Wave 4 (6–8 sem) — Costing
E8, E9, E10, E11 · FC dashboard · Merma

**Paralelo light:** diseño Command Center + BI schema (docs → spikes)

---

# 4. Gobernanza de scope

Committee semanal: Product + Tech  
Cualquier feature nueva debe pasar:

> ¿Gana / ahorra / controla / decide?  
> ¿Extiende building block existente?  
> ¿Rompe MT/perf?  
> ¿ROI vs top backlog?

Si no: **backlog icebox**.

---

# 5. Piloto transformation

| Mes | Cliente piloto | Objetivo |
|-----|----------------|----------|
| 0–1 | Actual POS+KDS | Baseline KPIs |
| 3 | Mismo + caja | Cierre sin Excel |
| 6 | + compras | FC visible |
| 9 | + combos/HH | Ticket +5% |
| 12 | + CC | Decisiones diarias |

---

# 6. Riesgos de implementación

| Riesgo | Plan |
|--------|------|
| OrderService demasiado grande | Extraer Cash/Promo adapters |
| Fiscal delay | Feature flag país |
| Over-design BI | Empezar views SQL simples |
| Stub creep | Policy: no merge UI sin datos |

---

# 7. Criterios de go/no-go por fase

| Fase | Go |
|------|-----|
| F1 | 1 piloto cierra caja 14 días |
| F2 | FC% calculado y validado vs Excel ±2pts |
| F3 | Ticket lift o factura fiscal emitida |
| F4 | 3 tenants self-serve trial |
| F5 | Copilot accept rate >25% beta |
