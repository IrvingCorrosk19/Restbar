# 10 — ROADMAP 24 MONTHS

De **41/100 → 90+/100** madurez comercial/empresarial.

Ordenado por ROI × impacto × dependencia (no por “nice to have”).

---

# Vista ejecutiva

| Fase | Meses | Score target | Tema |
|------|-------|--------------|------|
| **F0 Harden** | 0–1 | 43 | Ocultar rotos, unificar menús, lock seed |
| **F1 Money Ops** | 1–5 | 62 | Caja, precuenta, export, cierre |
| **F2 Cost Control** | 5–9 | 74 | Proveedor, PO, food cost, merma |
| **F3 Revenue** | 9–12 | 82 | Combos, HH, CRM mínimo, fiscal v1 |
| **F4 Intelligence** | 12–18 | 88 | Command Center, BI, forecast, SaaS |
| **F5 Advantage** | 18–24 | 92+ | Copilot, labor, delivery, franchise pack |

---

# FASE 0 — Harden (Mes 0–1) · ROI inmediato confianza

| # | Epic | Valor |
|---|------|-------|
| H1 | Ocultar SupplierAnalysis / exports stub / settings rotas | Confianza |
| H2 | Unificar Payment + User admin entry points | UX |
| H3 | Seed solo Development | Seguridad |
| H4 | Conectar JS AdvancedReports faltante | Gerencia |

---

# FASE 1 — Money Ops (Mes 1–5) · 41→62

| # | Epic | Gana | Ahorra | Controla | Decide | Semanas |
|---|------|------|--------|----------|--------|---------|
| E1 | Cash Management + arqueo | | ● | ●●● | ● | 6–8 |
| E2 | Precuenta + print térmica | ●● | | ● | | 4–6 |
| E3 | Export PDF/Excel real | | ● | | ●● | 3–4 |
| E4 | Cierre día / Z-report | | ● | ●● | ● | 3–4 |
| E5 | Shift UI + handoff | | | ● | | 2–3 |

**Exit:** turno completo sin Excel caja · SB-02/03/05/08 cerrados

---

# FASE 2 — Cost Control (Mes 5–9) · 62→74

| # | Epic | Semanas |
|---|------|---------|
| E6 | Supplier CRUD + multitenant | 3–4 |
| E7 | Purchase Order → Goods Receipt → stock/cost | 8–10 |
| E8 | Recipe UI + theoretical food cost | 4–6 |
| E9 | Waste/merma motivos | 2–3 |
| E10 | Actual vs theoretical variance | 3–4 |
| E11 | SupplierAnalysis real (reemplaza stub) | 2 |

**Exit:** PKS Purchasing ≥70% · dueño ve FC%

---

# FASE 3 — Revenue & Compliance (Mes 9–12) · 74→82

| # | Epic | Semanas |
|---|------|---------|
| E12 | Combos / bundles POS | 4–6 |
| E13 | Promo engine / Happy Hour (PriceSchedule) | 4–6 |
| E14 | Fiscal adapter país piloto | 8–12 |
| E15 | Customer capture POS + loyalty básico | 4–5 |
| E16 | Upsell suggestions ligeras | 2–3 |

**Exit:** lift ticket medible en piloto · factura legal piloto

---

# FASE 4 — Intelligence & SaaS (Mes 12–18) · 82→88

| # | Epic | Semanas |
|---|------|---------|
| E17 | Command Center CC-1…CC-3 | 6–8 |
| E18 | BI schema + nightly jobs | 6–8 |
| E19 | Forecast + reorder suggestions | 6–8 |
| E20 | Alert rules engine | 3–4 |
| E21 | SaaS plans + billing + onboarding wizard | 10–14 |
| E22 | Backup job real | 2 |

**Exit:** Comercialización regional + self-serve inicial

---

# FASE 5 — Competitive Advantage (Mes 18–24) · 88→92+

| # | Epic |
|---|------|
| E23 | Copilot rules → LLM tools |
| E24 | Labor schedule + labor cost light |
| E25 | Delivery connectors |
| E26 | Franchise pack + benchmarks |
| E27 | Offline POS resilience |
| E28 | Integraciones contables |

**Exit:** Competidor Enterprise LATAM · score ≥90

---

# Secuencia crítica (qué primero / segundo / tercero)

```
1º CAJA + PRECUENTA + EXPORT     → cliente paga y confía
2º COMPRAS + FOOD COST + MERMA   → cliente ahorra
3º COMBOS + HH + FISCAL          → cliente gana + legal
4º COMMAND CENTER + BI + SAAS    → escala y decide
5º COPILOT + LABOR + FRANQUICIA  → ventaja sostenible
```

---

# Qué POSPONER explícitamente

Hotel · Casino · Payroll nativo · ML deep · App marketplace · Reescritura SPA total

---

# Qué ELIMINAR / no invertir

- Superficies stub permanentes  
- Segundo motor de reportes paralelo  
- Features sin KPI de dinero  

---

# Tracking madurez

| Hito | Score |
|------|-------|
| Hoy | 41 |
| Post F1 | 62 |
| Post F2 | 74 |
| Post F3 | 82 |
| Post F4 | 88 |
| Post F5 | 92+ |
